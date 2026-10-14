
var app = angular.module('InfinityLab');

app.controller('BuyBackSchemeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Navigation.SetMode('SEARCH');

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.Items = restService.Get('ReferencelistService/ITEMS', true).then(function (results) {
            $scope.Items = results;
        });

        $scope.Entity =
            {
                id: "",
                supplierid: "",
                productid: "",
                itemid: "",
                status: "",
                fromdate: new Date(),
                todate: new Date(),
                minamt: "",
                maxamt: "",
                items: [],
                isactive: true,
                createdby: "",
                modifiedby: "",
                version: "",
                createdon: new Date(),
                modifiedon: new Date()
            };

        $scope.BuyBackScheme = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ShowSearch = false;

        $scope.AddItem = function () {

            var found = $scope.Items.find(function (arritem) {
                return arritem.id == $scope.BuyBackScheme.itemid;
            });

            if (typeof (found) == 'undefined' || found == null) {
                $rootScope.Toaster.Info("Please select the item");
                return;
            }
            $scope.BuyBackScheme.items.push({
                'itemid': $scope.BuyBackScheme.itemid, 'min': $scope.BuyBackScheme.minamt,
                'max': $scope.BuyBackScheme.maxamt, 'itemname': found.printingname
            });
        }

        $scope.DeleteItem = function (item) {
            var index = $scope.BuyBackScheme.items.indexOf(item);
            $scope.BuyBackScheme.items.splice(index, 1);
        }

        $scope.FilterItemList = function () {
            var productid = typeof ($scope.BuyBackScheme.productid) ? 0 : $scope.BuyBackScheme.productid;
            var brandid = typeof ($scope.BuyBackScheme.brandid) ? 0 : $scope.BuyBackScheme.brandid;

            $scope.ItemList = [];
            $.each($scope.Items, function (k, v) {
                if ((productid == 0 || v.productid == productid) && (brandid == 0 || v.brandid == brandid))
                    $scope.ItemList.push(v);
            });
        }

        $scope.New = function () {
            $scope.BuyBackScheme = (JSON.parse(JSON.stringify($scope.Entity)));

        }
        $scope.New();
        $scope.ShowSearch = false;
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.igrid = null;
        $scope.ConfigGrid = function () {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', 'Scheme', 150);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 200, $scope.Suppliers);
            igrid.AddFormatedColumn('schemedate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('productid', 'Product', 200, $scope.Products);
            igrid.AddReferenceColumn('brandid', 'Brand', 200, $scope.Products);
            igrid.AddFormatedColumn('fromdate', 'Valid from', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('todate', 'Valid to', 80, 'dd-MM-yyyy');
            igrid.Initialize();
            $scope.igrid = igrid;
        }

        $scope.Delete = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length = 1) {

                var request = {
                    "Item":  seletedRows[0] ,
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BuyBackSchemeService/Delete",
                    data: srequest
                }).success(function (result) {
                    if (result) {
                        var index = $scope.SearchResult.indexOf(this.igrid.GetSelectedRows());
                        $scope.SearchResult.splice(index, 1);
                        $scope.igrid.UpdateData($scope.SearchResult);
                    }
                }).error(function (result) {

                });

            }
            else {
                $rootScope.Toaster.Info("Please select one item to delete");
            }
        }

        $scope.Edit = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length = 1) {

                var request = {
                    "Item": { 'id': seletedRows[0].id},
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BuyBackSchemeService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length == 1) {
                        $scope.BuyBackScheme = result[0];
                        $scope.ShowSearch = false;
                    }
                }).error(function (result) {

                });
            }
            else {
                $rootScope.Toaster.Info("Please select one item to edit");
            }

        }

        $scope.SearchResult = [];
        $scope.SearchRegister = function () {
            if (typeof ($scope.SearchRequest) == 'undefined' || $scope.SearchRequest == null)
                $scope.SearchRequest = {};

            $scope.SearchRequest.hideitems = true;
            var request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BuyBackSchemeService/Search",
                data: srequest
            }).success(function (result) {
                if ($scope.igrid == null)
                    $scope.ConfigGrid();
                $scope.SearchResult = result;
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        }

        $scope.Save = function () {

            var request = {
                "Item": $scope.BuyBackScheme,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BuyBackSchemeService/Save",
                data: request
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Order Saved Successfully");
                    $scope.BuyBackScheme = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }


        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);


    }]);

