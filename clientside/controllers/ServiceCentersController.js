
var app = angular.module('InfinityLab');

app.controller('ServiceCentersController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = (JSON.parse(JSON.stringify(results)));
            $.each($scope.Locations, function (k, v) {
                v.selected = false;
            });
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.Brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brands = results;
        });

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
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
        $scope.ServiceCenters = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.ServiceCenters = (JSON.parse(JSON.stringify($scope.Entity)));
        }
        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }
        $scope.New = function () {
            $scope.ServiceCenters = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ServiceCenters.locations = [];
            $.each($scope.Locations, function (k, v) {
                v.selected = false;
            });

        }
        $scope.Save = function () {

            if ($scope.ServiceCenters.name == null || $scope.ServiceCenters.name == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Fill Out Service Center Name");
                return;
            }

            if ($scope.ServiceCenters.contactno == null || $scope.ServiceCenters.contactno == 0) {
                $scope['setFocuscontactno']();
                $rootScope.Toaster.Error("Please Fill Out Contact Number");
                return;
            }

            $scope.ServiceCenters.locations = [];
            $.each($scope.Locations, function (k, v) {
                if (v.selected) {
                    $scope.ServiceCenters.locations.push(v);
                }
            });

            request = {
                "Item": $scope.ServiceCenters,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ServicecentersService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("ServiceCenters Saved Successfully.!");
                    $scope.ServiceCenters = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

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
                                    url: "ServicecentersService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
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
                $scope.ServiceCenters = item;
                $.each($scope.Locations, function (k, v) {
                    v.selected = false;
                    if ($scope.ServiceCenters.locations != null) {
                        var location = $scope.ServiceCenters.locations.find(function (item) {
                            return item.id == v.id;
                        });
                        if (typeof (location) != 'undefined' && location != null)
                            v.selected = true;
                    }
                });
                $scope.Navigation.SetMode('EDIT');
            }

            else if (action == "DeleteItem") {
                $scope.ServiceCenters.loadingcharges.splice($scope.ServiceCenters.loadingcharges.indexOf(item), 1);
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
                url: "ServicecentersService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.TransportConfig();
            }).error(function (result) {
            });

        }


        $scope.TransportConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true,0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddSimpleColumn('contactperson', 'Contact Person', 150);
            igrid.AddSimpleColumn('contactno', 'Contact No', 100);
            igrid.AddSimpleColumn('emailid', 'Email Id', 150);
            igrid.AddSimpleColumn('address', 'Address', 150);
            igrid.AddReferenceColumn('cityid', 'City', 120, $scope.Cities);
           
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
            $scope.setFocusfirst();
        }, 10);
    }]);