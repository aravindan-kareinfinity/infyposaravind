
var app = angular.module('InfinityLab');

app.controller('TransportController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;

        $scope.BusinessModes = [{ 'id': 0, 'name': 'Lorry / Courier' },
            { 'id': 1, 'name': 'Courier' },
            { 'id': 2, 'name': 'Lorry' },
            { 'id': 3, 'name': 'Charges' }];

        $scope.Cities = [];
        restService.Get('ReferencelistService/CITY', true).then(function (results) {
            var cities = [];
            cities.push({ 'id': 0, 'name': 'Local' });
            cities.push({ 'id': -1, 'name': 'Local State' });
            cities.push({ 'id': -2, 'name': 'Other State' });
            $.each(results, function (k, v) {
                cities.push(v);
            });
            $scope.Cities = cities;
        });

        $scope.PaymentModes = [{ 'id': 0, 'name': 'NEFT/RTGS' },
        { 'id': 1, 'name': 'Cheque/DD' },
        { 'id': 2, 'name': 'Cash' },
        { 'id': 3, 'name': 'Multiple Cheque/DD' },
        { 'id': 4, 'name': 'PDC' }
        ];
        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });

        $scope.TaxList = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.TaxList = results;
        });

        $scope.TransportList = restService.Get('ReferencelistService/TRANSPORT/4', true).then(function (results) {
            $scope.TransportList = results;
        });

        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });
        $scope.Countries = restService.Get('ReferencelistService/COUNTRY', true).then(function (results) {
            $scope.Countries = results;
        });

        $scope.Entity = {
            //"id": 0,
            "name": "",
            "contactperson": "",
            "contactno": "",
            "emailid": "",
            "address": "",
            "cityid": 0,
            "stateid": 0,
            "countryid": 0,
            "pincode": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            pan: "",
            gst: ""
        };
        $scope.Transports = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Transports = (JSON.parse(JSON.stringify($scope.Entity)));
        }
        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }
        $scope.New = function () {
            $scope.Transports = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.cgrid.UpdateData([]);
        }
        $scope.Save = function () {

            if ($scope.Transports.name == null || $scope.Transports.name == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Fill Out Transport Name");
                return;
            }
            if ($scope.Transports.contactperson == null || $scope.Transports.contactperson == 0) {
                $scope['setFocuscontactperson']();
                $rootScope.Toaster.Error("Please Fill Out Contact Person");
                return;
            }
            if ($scope.Transports.contactno == null || $scope.Transports.contactno == 0) {
                $scope['setFocuscontactno']();
                $rootScope.Toaster.Error("Please Fill Out Contact Number");
                return;
            }

            request = {
                "Item": $scope.Transports,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TransportService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Transports Saved Successfully.!");
                    $scope.Transports = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.cgrid.UpdateData([]);
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }

        $scope.AddCity = function () {
            if (typeof ($scope.Transports.loadingcharges) == 'undefined' || $scope.Transports.loadingcharges == null)
                $scope.Transports.loadingcharges = [];

            var existing = $scope.Transports.loadingcharges.find(function (item) {
                return item.cityid == $scope.addcityid;
            });

            if (typeof (existing) != 'undefined' && existing != null) {
                $rootScope.Toaster.Error("Alread exist.");
                return;
            }

            $scope.Transports.loadingcharges.push({
                'cityid': $scope.addcityid, 'charge': $scope.chargeperkg,
                'boxcharge': $scope.chargeperbox, 'bundlecharge': $scope.chargeperbundle
            });
            $scope.cgrid.UpdateData($scope.Transports.loadingcharges);
            $scope.chargeperkg = '';
            $scope.chargeperbox = '';
            $scope.chargeperbundle = '';
            $scope.setFocusaddcityid();
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
                    buttons: {
                        formSubmit: {
                            text: 'Submit',
                            btnClass: 'btn-blue',
                            action: function () {
                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "TransportService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.New();
                                    }

                                }).error(function (result) {

                                });
                            }
                        },
                        cancel: function () {
                            //close
                        },
                    },

                    onContentReady: function () {
                        // bind to events
                        var jc = this;
	jc.$$formSubmit.trigger('focus');
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });


            }
            else if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.Transports = item;
                if ($scope.Transports.loadingcharges == null)
                    $scope.Transports.loadingcharges = [];

                $scope.cgrid.UpdateData($scope.Transports.loadingcharges);

                $scope.Navigation.SetMode('EDIT');
            }

            else if (action == "DeleteItem") {
                $scope.Transports.loadingcharges.splice($scope.Transports.loadingcharges.indexOf(item), 1);
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TransportService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.TransportConfig();
            }).error(function (result) {
            });

        }

        $scope.TransportChargeConfig = function () {
            if ($scope.cgrid != null)
                $scope.cgrid.destroy();

            var igrid = new InfyGrid('gridchargecontainer', '450px', '100%', true, false, true, 80);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('cityid', 'City', 200, $scope.Cities);

            igrid.AddColumn(
                {
                    id: "charge", name: "Per KG", field: "charge",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );


            igrid.AddColumn(
                {
                    id: "boxcharge", name: "Per Box", field: "boxcharge",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            igrid.AddColumn(
                {
                    id: "bundlecharge", name: "Per Bundle", field: "bundlecharge",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );


            var commands = "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('DeleteItem', args.item);
                });
            });
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.cgrid = igrid;
        }

        $scope.TransportConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddSimpleColumn('contactperson', 'Contact Person', 150);
            igrid.AddSimpleColumn('contactno', 'Contact No', 100);
            igrid.AddSimpleColumn('emailid', 'Email Id', 150);
            igrid.AddReferenceColumn('cityid', 'City', 120, $scope.Cities);
            igrid.AddReferenceColumn('stateid', 'State', 100, $scope.States);
            igrid.AddReferenceColumn('countryid', 'Country', 100, $scope.Countries);

            var commands = "";
            if ($rootScope.UserInfo.p_TRANSPORTMASTER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_TRANSPORTMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_TRANSPORTMASTER.modify || $rootScope.UserInfo.p_TRANSPORTMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }

        $timeout(function () {
            $scope.TransportChargeConfig();
            $scope.setFocusfirst();
        }, 10);
    }]);