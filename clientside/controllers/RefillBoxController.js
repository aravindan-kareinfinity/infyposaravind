
var app = angular.module('InfinityLab');

app.controller('RefillBoxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Floor = [{ 'id': 1, 'name': 'First Flour' }, { 'id': 2, 'name': 'Second Flour' }];

        $scope.Section = [{ 'id': 1, 'name': 'Section1' }, { 'id': 2, 'name': 'Section2' }];

        $scope.BoxType = [{ 'id': 1, 'name': 'type1' }, { 'id': 2, 'name': 'type2' }];

        //   $scope.Countries = [{ 'ID': 1, 'Name': 'India' }, { 'ID': 2, 'Name': 'Usa' }];
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });
        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "companyid": 0,
            "boxno": null,
            "floorid": 0,
            "sectionid": 0,
            "boxtype": 0,
            "capacity": 0,
            "productid": 0,
            "brandid": 0,
            "sizeid": 0,
            "materialid": 0,
            "indisplay": false
        };

        $scope.RefillBox = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.RefillBox = (JSON.parse(JSON.stringify($scope.Entity)));
        }
        $scope.Clear = function () {
            $scope.RefillBox = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";



        $scope.SearchCondition = {
            productid: 0,
            boxtype: 0,
            boxno: 0,
            companyid: 0
        }

        $scope.Clear = function () {
            $scope.RefillBox = (JSON.parse(JSON.stringify($scope.Entity)));
        }

      
        $scope.Save = function () {

            if ($scope.RefillBox.companyid == null || $scope.RefillBox.companyid == 0) {
                $scope['setFocusfirst']();

                $rootScope.Toaster.Error("Please select company");
                return;
            }
            if ($scope.RefillBox.floorid == null || $scope.RefillBox.floorid == 0) {

                $scope['setFocusfloorid']();
                $rootScope.Toaster.Error("Please select Floor");
                return;
            }

            if ($scope.RefillBox.sectionid == null || $scope.RefillBox.sectionid == 0) {

                $scope['setFocussectionid']();
                $rootScope.Toaster.Error("Please select section");
                return;
            }

            if ($scope.RefillBox.boxtype == null || $scope.RefillBox.boxtype == 0) {
                $scope['setFocusboxtype']();

                $rootScope.Toaster.Error("Please select boxtype");
                return;
            }

            if ($scope.RefillBox.boxno == null || $scope.RefillBox.boxno == 0) {
                $scope['setFocusboxno']();

                $rootScope.Toaster.Error("Please select boxno");
                return;
            }

            if ($scope.RefillBox.capacity == null || $scope.RefillBox.capacity == 0) {
                $scope['setFocuscapacity']();

                $rootScope.Toaster.Error("Please select capacity");
                return;
            }

            if ($scope.RefillBox.productid == null || $scope.RefillBox.productid == 0) {
                $scope['setFocusproductid']();

                $rootScope.Toaster.Error("Please select productid");
                return;
            }


            request = {
                "Item": $scope.RefillBox,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RefillboxService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("RefillBox Saved Successfully.!");
                    $scope.RefillBox = (JSON.parse(JSON.stringify($scope.Entity)));
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
                                    url: "RefillboxService/Delete",
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
                $scope.RefillBox = item;
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
                url: "RefillboxService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);