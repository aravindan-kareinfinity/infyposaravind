
var app = angular.module('InfinityLab');

app.controller('TailorController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;

        $scope.Cities = [];
        restService.Get('ReferencelistService/CITY', true).then(function (results) {
            var cities = [];
            $.each(results, function (k, v) {
                cities.push(v);
            });
            $scope.Cities = cities;
        });

        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
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
            gst: "",
            "config":[]
        };


        restService.Get('ReferencelistService/FULL-TAILORINGCHARGE', true).then(function (results) {
            $scope.ProductChargeList = results;
            $.each(results, function (k, v) {
                v.measurement = [];
                v.charge = v.additionalinfo.mfgconfig.charge;
                v.taxid = v.additionalinfo.mfgconfig.taxid;
                v.additionalchargeid = v.additionalinfo.mfgconfig.additionalchargeid;
                v.additionalchargeamount = v.additionalinfo.mfgconfig.additionalchargeamount;
                if (v.additionalinfo.mfgconfig &&
                    v.additionalinfo.mfgconfig.measurement &&
                    v.additionalinfo.mfgconfig.measurement != null) {
                    $.each(v.additionalinfo.mfgconfig.measurement.split(','), function (kk, vv) {
                        v.measurement.push({
                            'key': vv, 'value': '', 'focusonenter': 'mfe_' + k, 'focusid': 'mf_' + k, 'islast': false
                        });
                    });
                }
                if (v.measurement.length > 0)
                    v.measurement[v.measurement.length - 1].islast = true;

                $scope.Entity.config.push({
                    'chargeid': v.id,
                    'charge':0,
                    'delayperday': 0,
                    'delaymaximum': 0,
                    'chargeonpriority':0
                });
            });
            if ($scope.Tailors == undefined || $scope.Tailors == null || $scope.Tailors.id == undefined || $scope.Tailors.id == 0)
                $scope.Tailors = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.TransportChargeConfig();
        });


        $scope.Tailors = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Tailors = (JSON.parse(JSON.stringify($scope.Entity)));
        }
        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }
        $scope.New = function () {
            $scope.Tailors = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.cgrid.UpdateData([]);
            $scope.TransportChargeConfig();
        }
        $scope.Save = function () {

            if ($scope.Tailors.name == null || $scope.Tailors.name == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Fill Out tailor Name");
                return;
            }

            if ($scope.Tailors.contactno == null || $scope.Tailors.contactno == 0) {
                $scope['setFocuscontactno']();
                $rootScope.Toaster.Error("Please Fill Out Contact Number");
                return;
            }

            request = {
                "Item": $scope.Tailors,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TailorService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Tailors Saved Successfully.!");
                    $scope.Tailors = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.cgrid.UpdateData($scope.Tailors.config);
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }

        $scope.AddCity = function () {
            if (typeof ($scope.Tailors.loadingcharges) == 'undefined' || $scope.Tailors.loadingcharges == null)
                $scope.Tailors.loadingcharges = [];

            var existing = $scope.Tailors.loadingcharges.find(function (item) {
                return item.cityid == $scope.addcityid;
            });

            if (typeof (existing) != 'undefined' && existing != null) {
                $rootScope.Toaster.Error("Alread exist.");
                return;
            }

            $scope.Tailors.loadingcharges.push({
                'cityid': $scope.addcityid, 'charge': $scope.chargeperkg,
                'boxcharge': $scope.chargeperbox, 'bundlecharge': $scope.chargeperbundle
            });
            $scope.cgrid.UpdateData($scope.Tailors.loadingcharges);
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
                                    url: "TailorService/Delete",
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
                $scope.Tailors = item;
                $scope.Navigation.SetMode('EDIT');

                $timeout(function () {
                    $scope.TransportChargeConfig();
                }, 30);
            }

            else if (action == "DeleteItem") {
                $scope.Tailors.loadingcharges.splice($scope.Tailors.loadingcharges.indexOf(item), 1);
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
                url: "TailorService/Search",
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

            var igrid = new InfyGrid('gridchargecontainer', '450px', '100%', true, true, true, 60);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('chargeid', 'Work', 200, $scope.ProductChargeList);

            igrid.AddColumn(
                {
                    id: "charge", name: "Charge", field: "charge",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            
            igrid.AddColumn(
                {
                    id: "delayperday", name: "Delay/Per Day", field: "delayperday",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            igrid.AddColumn(
                {
                    id: "delaymaximum", name: "Delay Maximum", field: "delaymaximum",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );


            igrid.AddColumn(
                {
                    id: "chargeonpriority", name: "Prioriy", field: "chargeonpriority",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            if ($scope.Tailors.config == undefined || $scope.Tailors.config == null)
                $scope.Tailors.config = [];
            $.each($scope.ProductChargeList, function (k, v) {
                var exist = $scope.Tailors.config.find(function (item) {
                    return item.chargeid == v.id;
                });
                if (exist == undefined || exist == null) {
                    $scope.Tailors.config.push({
                        'chargeid': v.id,
                        'charge':0,
                        'delayperday': 0,
                        'delaymaximum': 0,
                        'chargeonpriority': 0
                    });
                }
            });
            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.UpdateData($scope.Tailors.config);
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


    }]);