
var app = angular.module('InfinityLab');

app.controller('RechargeSchemeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Navigation.SetMode('SEARCH');

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
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

        $scope.RechargeScheme = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ShowSearch = false;

        $scope.AddItem = function () {

            $scope.RechargeScheme.items.push({
                'description': $scope.RechargeScheme.description, 'rate': $scope.RechargeScheme.rate,
                'cashback': $scope.RechargeScheme.cashback, 'from': $scope.RechargeScheme.fromdate,
                'to': $scope.RechargeScheme.todate, 'discount': $scope.RechargeScheme.discount
            });
        }

        $scope.DeleteItem = function (item) {
            var index = $scope.RechargeScheme.items.indexOf(item);
            $scope.RechargeScheme.items.splice(index, 1);
        }

        $scope.FilterItemList = function () {
            var productid = typeof ($scope.RechargeScheme.productid) == 'undefined' ? 0 : $scope.RechargeScheme.productid;

            $scope.ItemList = [];
            $.each($scope.Items, function (k, v) {
                if (v.productid == productid)
                    $scope.ItemList.push(v);
            });
        }

        $scope.New = function () {
            $scope.RechargeScheme = (JSON.parse(JSON.stringify($scope.Entity)));

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
            igrid.AddSimpleColumn('name', 'Operator', 150);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 200, $scope.Suppliers);
            igrid.AddReferenceColumn('productid', 'Product', 200, $scope.Products);
            igrid.AddReferenceColumn('itemid', 'Item', 200, $scope.Items);
            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/>"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"
            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.Edit(args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.Delete(args.item);
                });
            });
            igrid.Initialize();
            $scope.igrid = igrid;
        }


        $scope.Delete = function (item) {

            var request = {
                "Item": item,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RechargeSchemeService/Delete",
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

        $scope.Edit = function (item) {
           
                var request = {
                    "Item": { 'id': item.id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "RechargeSchemeService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length == 1) {
                        $scope.RechargeScheme = result[0];
                        $scope.ShowSearch = false;
                    }
                }).error(function (result) {

                });
           

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
                url: "RechargeSchemeService/Search",
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
                "Item": $scope.RechargeScheme,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RechargeSchemeService/Save",
                data: request
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Order Saved Successfully");
                    $scope.RechargeScheme = (JSON.parse(JSON.stringify($scope.Entity)));
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

