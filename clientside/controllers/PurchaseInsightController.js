
var app = angular.module('InfinityLab');

app.controller('PurchaseInsightController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$stateParams', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $stateParams, $timeout,$filter) {
        $scope.SearchRef = restService.SearchReference;



        $scope.SearchRequest = {
            typeid: 0,
            addedfrom: null,
            supplierid: null,
            statusid: null,
            addedto: null
        };



        $scope.GetSelecteItemObject = function (type) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v);
            });
            return retList;
        };

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'WAREHOUSE DASHBOARD - INFORMATION'
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.ConfigGrid = function () {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);

            //igrid.options.showHeaderRow = true;
            var groupColumns = [];

            igrid.AddSimpleColumn('product', 'Name', 80);
            igrid.AddFormatedColumn('BuyingPrice', 'BuyingPrice', 80, 'N0', 0);
            igrid.AddFormatedColumn('BuyingQty', 'BuyingQty', 80, 'N0', 0).total = true;
            var column = igrid.AddCustomColumn('ProductData.stock', 'Stock', 80, function (item, col) {
                return $filter('IndianNumber')(item.ProductData.stock, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.ProductData.stock;
            }
            var column = igrid.AddCustomColumn('ProductData.stockvalue', 'Cost', 80, function (item, col) {
                return $filter('IndianNumber')(item.ProductData.stockvalue, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.ProductData.stockvalue;
            }
            var column = igrid.AddCustomColumn('ProductData.stock90', '90Days', 80, function (item, col) {
                return $filter('IndianNumber')(item.ProductData.stock90, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.ProductData.stock90;
            }
            var column = igrid.AddCustomColumn('ProductData.sales', 'Sales', 80, function (item, col) {
                return $filter('IndianNumber')(item.ProductData.sales, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.ProductData.sales;
            }

            groupColumns.push(
                {
                    id: 'Product',
                    name: 'Product',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];



            igrid.AddSimpleColumn('brand', 'Name', 80);
            var column = igrid.AddCustomColumn('BrandData.stock', 'Stock', 80, function (item, col) {
                return $filter('IndianNumber')(item.BrandData.stock, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.BrandData.stock;
            }
            var column = igrid.AddCustomColumn('BrandData.stockvalue', 'Cost', 80, function (item, col) {
                return $filter('IndianNumber')(item.BrandData.stockvalue, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.BrandData.stockvalue;
            }
            var column = igrid.AddCustomColumn('BrandData.stock90', '90Days', 80, function (item, col) {
                return $filter('IndianNumber')(item.BrandData.stock90, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.BrandData.stock90;
            }
            var column = igrid.AddCustomColumn('BrandData.sales', 'Sales', 80, function (item, col) {
                return $filter('IndianNumber')(item.BrandData.sales, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.BrandData.sales;
            }

            groupColumns.push(
                {
                    id: 'Brand',
                    name: 'Brand',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];




            igrid.AddSimpleColumn('design', 'Name', 80);
            var column = igrid.AddCustomColumn('DesignData.stock', 'Stock', 80, function (item, col) {
                return $filter('IndianNumber')(item.DesignData.stock, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.DesignData.stock;
            }
            var column = igrid.AddCustomColumn('DesignData.stockvalue', 'Cost', 80, function (item, col) {
                return $filter('IndianNumber')(item.DesignData.stockvalue, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.DesignData.stockvalue;
            }
            var column = igrid.AddCustomColumn('DesignData.stock90', '90Days', 80, function (item, col) {
                return $filter('IndianNumber')(item.DesignData.stock90, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.DesignData.stock90;
            }
            var column = igrid.AddCustomColumn('DesignData.sales', 'Sales', 80, function (item, col) {
                return $filter('IndianNumber')(item.DesignData.sales, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.DesignData.sales;
            }

            groupColumns.push(
                {
                    id: 'Design',
                    name: 'Design',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];




            igrid.AddSimpleColumn('SellingPrice', 'Price', 80);
            var column = igrid.AddCustomColumn('SalesPriceData.stock', 'Stock', 80, function (item, col) {
                return $filter('IndianNumber')(item.SalesPriceData.stock, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.SalesPriceData.stock;
            }
            var column = igrid.AddCustomColumn('SalesPriceData.stockvalue', 'Cost', 80, function (item, col) {
                return $filter('IndianNumber')(item.SalesPriceData.stockvalue, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.SalesPriceData.stockvalue;
            }
            var column = igrid.AddCustomColumn('SalesPriceData.stock90', '90Days', 80, function (item, col) {
                return $filter('IndianNumber')(item.SalesPriceData.stock90, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.SalesPriceData.stock90;
            }
            var column = igrid.AddCustomColumn('SalesPriceData.sales', 'Sales', 80, function (item, col) {
                return $filter('IndianNumber')(item.SalesPriceData.sales, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.SalesPriceData.sales;
            }

            groupColumns.push(
                {
                    id: 'SalesPrice',
                    name: 'Sales Price',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];






            var column = igrid.AddCustomColumn('SupplierData.stock', 'Stock', 80, function (item, col) {
                return $filter('IndianNumber')(item.SupplierData.stock, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.SupplierData.stock;
            }
            var column = igrid.AddCustomColumn('SupplierData.stockvalue', 'Cost', 80, function (item, col) {
                return $filter('IndianNumber')(item.SupplierData.stockvalue, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.SalesPriceData.stockvalue;
            }
            var column = igrid.AddCustomColumn('SupplierData.stock90', '90Days', 80, function (item, col) {
                return $filter('IndianNumber')(item.SupplierData.stock90, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.SupplierData.stock90;
            }
            var column = igrid.AddCustomColumn('SupplierData.sales', 'Sales', 80, function (item, col) {
                return $filter('IndianNumber')(item.SupplierData.sales, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.SupplierData.sales;
            }

            groupColumns.push(
                {
                    id: 'Supplier',
                    name: 'Supplier',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];

            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
        }

        $scope.ConfigGrid();
        $timeout(function () {

            if ($stateParams.data.lrid > 0) {
                restService.Get('AnalyserService/Purchase/' + $stateParams.source + '/' + $stateParams.data.lrid, true).then(function (results) {
                    $scope.Insight = results;
                    $scope.igrid.UpdateData(results.items);
                });
            }

            if ($stateParams.data.poid > 0) {
                restService.Get('AnalyserService/Purchase/' + $stateParams.source + '/' + $stateParams.data.poid, true).then(function (results) {
                    $scope.Insight = results;
                    $scope.igrid.UpdateData(results.items);
                });
            }

            

        }, 50);

    }]);
