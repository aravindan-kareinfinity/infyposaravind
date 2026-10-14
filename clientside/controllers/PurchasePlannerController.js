
var app = angular.module('InfinityLab');

app.controller('PurchasePlannerController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', 'cntxService','$state',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, cntxService, $state) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.IsBrandBased = false;

        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
        });

        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });
        $scope.ProductGroups = restService.Get('ReferencelistService/PRODUCTGROUPS', true).then(function (results) {
            $scope.ProductGroups = results;
        });
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });
        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brands = results;
        });
        //$scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
        //    $scope.Sizes = results;
        //    var exist = false;
        //    $.each($scope.Sizes, function (k, v) {
        //        if (v.id == -2)
        //            exist = true;
        //    });
        //    if (!exist) {
        //        $scope.Sizes.push({ id: -2, name: 'Jump' });
        //        $scope.Sizes.push({ id: -3, name: 'Cut' });
        //    }
        //});

        $scope.Types = restService.Get('ReferencelistService/Type', true).then(function (results) {
            $scope.Types = results;
        });
        //$scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
        //    $scope.Pattern = results;
        //});
        //$scope.Style = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
        //    $scope.Style = results;
        //});
        //$scope.Section = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
        //    $scope.Section = results;
        //});

        $scope.pricerange = {}
        $scope.SearchCondition = {
            reporttype: 'PRODUCT',
            locationid: 0,
            counterid: 0,
            fromdate: new Date(),
            todate: new Date(),
            primarycolumn: 0,
            secondarycolumn: 0,
            markup: 0,
            plandays: 15,
            rotation: 4,
            rotationdays: '90 days',
            columns: []
        };

        $scope.rotatiochanged = function () {
            var rot = parseFloat($scope.SearchCondition.rotation);
            if (!isNaN(rot) && rot > 0) {
                $scope.SearchCondition.rotationdays = '' + ((12 / rot) * 30).toFixed(0) + ' days';
            }
        };

        $scope.ProductChange = function () {

            
            $scope.SearchCondition.PriceRange = [];
            $scope.SearchCondition.KeyGroup = [];

            if ($scope.SearchCondition.productid == 0 || $scope.SearchCondition.productid == null)
                return;

            request = {
                "item": { id: $scope.SearchCondition.productid },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ProductsService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $scope.SearchCondition.PriceRange = result[0].pricerange;
                    $scope.SearchCondition.KeyGroup = result[0].keygroup;
                }
            }).error(function (result) {
            });
        }

        $scope.Search = function () {

            if ($scope.SearchCondition.primarycolumn == 18 || $scope.SearchCondition.secondarycolumn == 18) {
                $scope.SearchCondition.pricerange = $scope.pricerange.start + '-' + $scope.pricerange.increment + '-' + $scope.pricerange.end
            }

            $scope.SearchCondition.columns = [];
            $.each($scope.Options, function (k, v) {
                if (v.id != $scope.SearchCondition.primarycolumn &&
                    v.id != $scope.SearchCondition.secondarycolumn &&
                    (typeof (v.selected) != 'undefined' && v.selected)) {
                    $scope.SearchCondition.columns.push(v.name);
                }
            });

            if ($scope.IsBrandBased) {
                $scope.SearchCondition.productid = 0;
                $scope.SearchCondition.productgroupid = 0;
            }

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "AnalyserService/PurchasePlanner",
                data: srequest
            }).success(function (result) {
                $scope.GroupResult = result;
                $scope.ShowGroupResult();
            }).error(function (result) {
            });
        }

        $scope.exporttoExcel = function (infygrid, filename) {
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, filename);
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

        $scope.Convert2PO = function () {
            var order = {};
            var rows = [];

            $.each($scope.GroupResult.Items, function (kk, vv) {
                var baserow = { 'columns': [] };
                var item = vv;
                $.each($scope.GroupResult.Columns, function (k, v) {
                    var uid = 'Col-' + v.Index;
                    var display = item.String[v.Index];
                    var value = item.Values[v.Index];
                    var type = v.Header;
                    baserow.columns.push({ 'uid': uid, 'display': display, 'value': value ,'type':type });
                });

                $.each($scope.GroupResult.Groups, function (k, v) {
                    var row = (JSON.parse(JSON.stringify(baserow)));
                    var uid = 'Grp-' + v.Index;
                    var display = v.Header;
                    var value = v.Value;
                    var type = $scope.GroupResult.Configuration.GroupColumn;
                    row.columns.push({ 'uid': uid, 'display': display, 'value': value, 'type': type });

                    var cellresult = item.GroupValues[v.Index];
                    if (cellresult != null) {
                        row.cqty = cellresult.cqty;
                        row.sqty = cellresult.sqty;
                    }
                    rows.push(row);
                });
            });

            $scope.LocalContext = cntxService.CreatePageContext('POBuilder');
            $scope.LocalContext.Post('ItemList', rows, false);
            $state.go('POBuilder', {
                directlaunch: false,
                source: 'Purchase Plan',
                list: rows
            });

        }

        $scope.igrid = null;
        $scope.ShowGroupResult = function () {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }
            var igrid = new InfyGrid('gridContainer', '575px', '100%', true, false, true, 0);

            var groupColumns = [];

            $scope.GroupResult.ResultColumn = { 'qty': true, 'value': true };
            $.each($scope.GroupResult.Columns, function (k, v) {
                var column = igrid.AddCustomColumn('Col-' + v.Index, v.Header, 150, function (item, col) {
                    return item.String[col.internal.Index];
                });
                column.internal = v;
            })
            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Filter',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            var freecolumns = igrid.columns.length - 1;

            if ($scope.GroupResult.EnablePriority) {

                igrid.columns = [];

                var column = igrid.AddCustomColumn('Grp-pratio', 'P.Ratio', 100, function (item, col) {
                    if (item.priority != null) return $filter('IndianNumber')(item.priority.pratio, 2);
                    return '';
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('Grp-pratio', 'P.Qty', 100, function (item, col) {
                    if (item.priority != null) return $filter('IndianNumber')(item.priority.pqty, 2);
                    return '';
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('Grp-sratio', 'S.Ratio', 100, function (item, col) {
                    if (item.priority != null) return $filter('IndianNumber')(item.priority.sratio, 2);
                    return '';
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('Grp-pratio', 'S.Qty', 100, function (item, col) {
                    if (item.priority != null) return $filter('IndianNumber')(item.priority.sqty, 2);
                    return '';
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('Grp-sratio', 'Performance', 100, function (item, col) {
                    if (item.priority != null) return $filter('IndianNumber')(item.priority.psratio, 2);
                    return '';
                });
                column.cssClass = 'rightalign';
                groupColumns.push(
                    {
                        id: 'Filter',
                        name: 'Purchase Vs Sales (1 Year)',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            $.each($scope.GroupResult.Groups, function (k, v) {

                igrid.columns = [];


                var column = igrid.AddCustomColumn('Grp-SlQty-' + v.Index, 'Sold', 100, function (item, col) {
                    //return $filter('IndianNumber')(item.GroupValues[col.internal.Index], 0);
                    var cellresult = item.GroupValues[col.internal.Index];
                    if (cellresult != null) return $filter('IndianNumber')(cellresult.qty, 2);
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.getValue = function (item, col) {
                    var cellresult = item.GroupValues[col.internal.Index];
                    if (cellresult != null) return cellresult.qty;
                    return 0;
                }

                var column = igrid.AddCustomColumn('Grp-RQty-' + v.Index, 'Required', 100, function (item, col) {
                    //return $filter('IndianNumber')(item.GroupValues[col.internal.Index], 0);
                    var cellresult = item.GroupValues[col.internal.Index];
                    if (cellresult != null) return $filter('IndianNumber')(cellresult.cqty, 2);
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.getValue = function (item, col) {
                    var cellresult = item.GroupValues[col.internal.Index];
                    if (cellresult != null) return cellresult.cqty;
                    return 0;
                }

                var column = igrid.AddCustomColumn('Grp-SQty-' + v.Index, 'Available', 100, function (item, col) {
                    //return $filter('IndianNumber')(item.GroupValues[col.internal.Index], 0);
                    var cellresult = item.GroupValues[col.internal.Index];
                    if (cellresult != null) return $filter('IndianNumber')(cellresult.sqty, 2);
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.getValue = function (item, col) {
                    var cellresult = item.GroupValues[col.internal.Index];
                    if (cellresult != null) return cellresult.sqty;
                    return 0;
                }


                var column = igrid.AddCustomColumn('Grp-OQty-' + v.Index, 'Order', 100, function (item, col) {
                    //return $filter('IndianNumber')(item.GroupValues[col.internal.Index], 0);
                    var cellresult = item.GroupValues[col.internal.Index];
                    if (cellresult != null && cellresult.cqty > cellresult.sqty)
                        return $filter('IndianNumber')(cellresult.cqty - cellresult.sqty, 2);
                    return '';
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.getValue = function (item, col) {
                    var cellresult = item.GroupValues[col.internal.Index];
                    if (cellresult != null && cellresult.cqty > cellresult.sqty)
                        return  cellresult.cqty - cellresult.sqty;
                    return 0;
                }

                if (igrid.columns.length > 0) {
                    groupColumns.push(
                        {
                            id: v.Header,
                            name: v.Header,
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                }

            })

            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.GroupResult.Items);
            igrid.grid.setOptions({ 'frozenColumn': freecolumns });
        }

        var CondtionType =
        {
            Range: 0,
            String: 1,
            Long: 2,
            Date: 3,
            Decmal: 4,
            DayRange: 5
        }

        var Roles =
        {
            All: 0,
            Sales: 1,
            BothStock: 2,
            WHStock: 3,
            RStock: 4
        }

        var Options =
        {
            Product: 0,
            Brand: 1,
            Size: 2,
            Pattern: 3,
            Material: 4,
            Style: 5,
            Colour: 6,
            Type: 7,
            Section: 8,
            SaleLocation: 9,
            StockLocation: 10,
            Item: 105,
            Design: 11,
            SaleRate: 12,
            PurchaseRate: 13,
            DiscountRate: 14,
            SoldRate: 15,
            SaleRate_Range: 16,
            StockAge_Range: 17,
            PurchaseRate_Range: 18,
            DiscountRate_Range: 19,
            SoldRate_Range: 20,
            LREntryNo: 21,
            InvoiceNo: 22,
            InvDate: 23,
            SaleYear: 24,
            PurchaseYear: 25,
            PurchaseMonth: 26,
            SaleMonth: 27,
            BillNo: 28,
            BillDate: 29,
            Supplier: 30,
            Salesman: 31,
            Supplier_City: 32,
            Barcode: 33,
            HSNCode: 34,
            Tax: 35,
            SaleWeekDay: 36,
            SaleMonthDay: 37,
            Company: 49,
            ProductGroup: 50,
            ReturnReason: 51,
            SaleReason: 52,
            Area: 53,
            Season: 54,
            Item: 105
        }

        $scope.Options = [
            { 'id': Options.Product, 'name': 'Product', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Brand, 'name': 'Brand', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Size, 'name': 'Size', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Pattern, 'name': 'Pattern', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Material, 'name': 'Material', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Style, 'name': 'Style', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Colour, 'name': 'Colour', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Type, 'name': 'Type', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Item, 'name': 'Item', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Design, 'name': 'Design', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.Supplier, 'name': 'Supplier', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.PurchaseRate_Range, 'name': 'Purchase Rate', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            { 'id': Options.Supplier_City, 'name': 'Supplier City', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
        ];
    }]);