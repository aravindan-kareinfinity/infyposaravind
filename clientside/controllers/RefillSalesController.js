
var app = angular.module('InfinityLab');

app.controller('RefillSalesController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });

        $scope.RefillBoxList = restService.Get('ReferencelistService/REFILLBOX', true).then(function (results) {
            $scope.RefillBoxList = results;
            $.each($scope.RefillBoxList, function (k, v) {
                v.name = v.boxno + ' ( ' + v.floorname + ', ' + v.sectionname + ' )';
            });
        });
        
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

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
            "companyid": 0,
            "Flourid": 0,
            "sectionid": 0,
            "boxno": "",
            "typeid": 0,
            "capacity": 0,
            "stateid": 0,
            "countryid": 0,
            "productid": 0,
            "sizeid": 0,
            "brandid": 0,
            "materialid": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": ""
        };

        $scope.RefillBox = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ItemList = [];

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Agent = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.SearchByBarcode = function (barcode) {
            $scope.RefillBox.barcode = barcode;
            $scope.SearchItems();
        };


        $scope.SearchItems = function () {
            var request = {
                "Item": $scope.RefillBox,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalablegoodsService/Search4Refill",
                data: srequest
            }).success(function (result) {
                if ($scope.ItemList.length == 0) {
                    $scope.ItemList = result;
                }
                else {
                    $.each(result, function (k, v) {
                        var exist = $scope.ItemList.find(function (itm) {
                            return (itm.stockid == v.stockid);
                        })
                        if (exist == null) {
                            $scope.ItemList.push(v);
                        }
                    });
                }
            }).error(function (result) {
            });
        }


        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }

        $scope.Save = function (isdispatch,isreturn) {
            var items = [];
            $.each($scope.ItemList, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected)
                    items.push(new {
                        'id': v.id,
                        'refillboxid': v.refillboxid
                    });
            });

            var request = {
                "Item":
                    {
                        'Items': items,
                        'Dispatch': isdispatch,
                        'Return': isreturn
                    },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalablegoodsService/ChangeRefillID",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.ErrorMessage = "Successfully saved";
                    $scope.ErrorStyle = "success";
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
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
                                    url: "AgentService/Delete",
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
                $scope.Agent = item;
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
                url: "AgentService/Search",
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