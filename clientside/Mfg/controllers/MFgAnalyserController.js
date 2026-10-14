var app = angular.module('InfinityLab');

app.controller('MfgAnalyserController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.Permission = UserDetails.Info;
        $scope.SearchRef = restService.SearchReference;
        $scope.PrimarySelected = null;
        $scope.SecondarySelected = [];
        $scope.SecondaryName = [];
        $scope.SecondaryCondtions = [];
        $scope.Range = null;
        $scope.ReportList = [];
        $scope.secondarystyle_selected = {
            "color": "white",
            "background-color": "coral",
        };

        $scope.analyser_stock_purchasesales = false;
        $scope.ApplicationSettings = function () {
            var srequest = JSON.stringify({
                "Item": { 'locationid': -9, 'key': 'APPLICATIONSETTINGS' },
                "securitytoken": "String content"
            });

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.analyser_stock_purchasesales = result[0].appsettings.analyser_stock_purchasesales
                        && result[0].appsettings.analyser_stock_withzerostock;
                }
                $scope.ConfigureGrid();
            }).error(function (result) {
                $scope.ConfigureGrid();
            });
        }();

        restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            if (results.length > 0)
                $scope.SearchCondition.warehouselocationid = results[0].id;
        });

        $scope.ResetSelection = function () {
            $scope.filter_name_style = 'fa-plus';
            $scope.filter_name = false;
            $scope.filter_name_value = '';

            $scope.filter_value_style = 'fa-plus';
            $scope.filter_value = false;
            $scope.filter_value_value = '';

            $scope.filter_all_value = true;
            $scope.filter_all_style = 'fa-asterisk';
        }

        $scope.ReportList = restService.Get('ReferencelistService/DIRECT-STOCKANALYSER', true).then(function (results) {
            $scope.ReportList = results;
        });

        $scope.ResetSelection();

        $scope.headerclick = function (action) {
            if (action == 'filter_name') {
                if ($scope.filter_name) {
                    $scope.filter_name = false;
                    $scope.filter_name_style = 'fa-plus';
                }
                else {
                    $scope.filter_name = true;
                    $scope.filter_name_style = 'fa-minus';
                    $scope.filter_name_value = '';
                }
            } else if (action == 'filter_value') {
                if ($scope.filter_value) {
                    $scope.filter_value = false;
                    $scope.filter_value_style = 'fa-plus';
                    $scope.filter_value_value = '';
                }
                else {
                    $scope.filter_value = true;
                    $scope.filter_value_style = 'fa-minus';
                    $scope.filter_value_value = '';
                }
            } else if (action == 'filter_all_value') {

                var bodyrows = $('#stockanalyser tbody tr');
                $scope.filter_value_value = $scope.filter_value_value.toUpperCase();

                if ($scope.filter_all_value) {
                    $scope.filter_all_value = false;
                    $scope.filter_all_style = 'fa-ban';
                    $.each($scope.SearchResult, function (k, v) {
                        if (typeof (v.selected) == null || !v.selected) {
                            if ($scope.filter_value_value == '' || (v.name != null && v.name.indexOf($scope.filter_value_value) >= 0))
                                $scope.secondaryselected(v);
                        }
                    });
                }
                else {
                    $scope.filter_all_value = true;
                    $scope.filter_all_style = 'fa-asterisk';
                    $.each($scope.SearchResult, function (k, v) {
                        if (typeof (v.selected) != null && v.selected) {
                            if ($scope.filter_value_value == '' || (v.name != null && v.name.indexOf($scope.filter_value_value) >= 0))
                                $scope.secondaryselected(v);
                        }
                    });
                }
            }
        }

        $scope.secondarystyle_unselected = {
            "color": "black",
            "background-color": "while"
        };


        $scope.style_selected = {
            "color": "white",
            "background-color": "coral",
            "font-size": "large"
        };

        $scope.style_unselected = {
            "color": "black",
            "background-color": "while"
        };


        $scope.ReportType = [{ 'id': 0, 'name': 'WAREHOUSE STOCK' },
        { 'id': 1, 'name': 'RETAIL STOCK' },
        { 'id': 2, 'name': 'SALES' },
        { 'id': 4, 'name': 'STOCK' }];

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
            SaleMargin_Range: 38,
            RetailMargin_Range: 39,
            Company: 49,
            ProductGroup: 50,
            ReturnReason: 51,
            SaleReason: 52,
            Area: 53,
            Season: 54,
            PurchaseDate: 84,
            SourceSupplier: 94
        }

        $scope.ReportType = [{ 'id': 0, 'name': 'WAREHOUSE STOCK' },
        { 'id': 1, 'name': 'RETAIL STOCK' },
        { 'id': 2, 'name': 'SALES' },
        { 'id': 3, 'name': 'STOCK' }];

        $scope.Options = [
            { 'id': Options.Company, 'name': 'COMPANY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.ProductGroup, 'name': 'PRODUCT GROUP', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Product, 'name': 'PRODUCT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Brand, 'name': 'BRAND', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Size, 'name': 'SIZE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Pattern, 'name': 'PATTERN', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Material, 'name': 'MATERIAL', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Style, 'name': 'STYLE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Colour, 'name': 'COLOUR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Type, 'name': 'TYPE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Section, 'name': 'SECTION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            //{ 'id': Options.SaleLocation, 'name': 'SALE LOCATION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            { 'id': Options.StockLocation, 'name': 'STOCK LOCATION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Design, 'name': 'DESIGN', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.SaleRate, 'name': 'SALE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.PurchaseRate, 'name': 'PURCHASE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.DiscountRate, 'name': 'DISCOUNT RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            //{ 'id': Options.SoldRate, 'name': 'SOLD RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.Sales },
            { 'id': Options.SaleRate_Range, 'name': 'SALE RATE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            { 'id': Options.StockAge_Range, 'name': 'STOCK AGE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.BothStock },
            { 'id': Options.PurchaseRate_Range, 'name': 'PURCHASE RATE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            { 'id': Options.DiscountRate_Range, 'name': 'DISCOUNT RATE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            { 'id': Options.SaleMargin_Range, 'name': 'SALE MARGIN RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            { 'id': Options.RetailMargin_Range, 'name': 'RETAIL MARGIN RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            //{ 'id': Options.SoldRate_Range, 'name': 'SOLD RATE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales },
            { 'id': Options.LREntryNo, 'name': 'LRENTRY NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.InvoiceNo, 'name': 'INVOICE NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.InvDate, 'name': 'INVOICE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All },
            { 'id': Options.PurchaseDate, 'name': 'PURCHASE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All },
            //{ 'id': Options.SaleYear, 'name': 'SALE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            { 'id': Options.PurchaseYear, 'name': 'PURCHASE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.PurchaseMonth, 'name': 'PURCHASE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            //{ 'id': Options.SaleMonth, 'name': 'SALE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            //{ 'id': Options.BillNo, 'name': 'BILL NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.Sale },
            //{ 'id': Options.BillDate, 'name': 'BILL DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.Sale },
            { 'id': Options.Supplier, 'name': 'SUPPLIER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            //{ 'id': Options.Salesman, 'name': 'SALES MAN', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sale },
            { 'id': Options.Supplier_City, 'name': 'SUPPLIER CITY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Barcode, 'name': 'BARCODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.HSNCode, 'name': 'HSN CODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.Tax, 'name': 'TAX', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SourceSupplier, 'name': 'SOURCE SUPPLIER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All }
            //{ 'id': Options.SaleWeekDay, 'name': 'Sale Week Day', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sale },
            //{ 'id': Options.SaleMonthDay, 'name': 'Sale Month Day', 'style': $scope.style_unselected, 'type': CondtionType.DayRange, 'role': Roles.Sale }
        ];

        $scope.Options.sort(function (a, b) {
            var x = a.name.toLowerCase();
            var y = b.name.toLowerCase();
            return x < y ? -1 : x > y ? 1 : 0;
        });

        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
            var exist = false;
            $.each($scope.Sizes, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.Sizes.push({ id: -2, name: 'Jump' });
                $scope.Sizes.push({ id: -3, name: 'Cut' });
            }
        });

        $scope.secondaryselected = function (item) {
            if (typeof (item.selected) != 'undefined' && item.selected) {
                item.selected = false;
                item.style = $scope.secondarystyle_unselected;
                for (var i = 0; i < $scope.SecondarySelected.length; i++) {
                    if ($scope.SecondarySelected[i] == item.id) {
                        $scope.SecondarySelected.splice(i, 1);
                        $scope.SecondaryName.splice(i, 1);
                        break;
                    }
                }
            }
            else {
                item.selected = true;
                item.style = $scope.secondarystyle_selected;
                $scope.SecondarySelected.push(item.id);
                $scope.SecondaryName.push(item.name);
            }
        }

        $scope.history = function (item, index) {
            if ($scope.PrimarySelected != null) {
                $scope.PrimarySelected.style = $scope.style_unselected;
            }
            if (item == null) {
                $scope.PrimarySelected = null;
                $scope.SecondaryCondtions = [];
                $scope.SearchResult = [];
            }
            else {

                var removed = $scope.SecondaryCondtions[index];
                $scope.SecondaryCondtions.splice(index, $scope.SecondaryCondtions.length - index);
                $scope.Range = removed.range;
                $scope.PrimarySelected = $scope.Options.find(function (item) {
                    return item.id == removed.option;
                });
                $scope.PrimarySelected.style = $scope.style_selected;
                $scope.SecondarySelected = [];
                $scope.SecondaryName = removed.nameList;
                if ($scope.PrimarySelected.type == CondtionType.Date)
                    $scope.SecondarySelected = removed.dateList;
                else if ($scope.PrimarySelected.type == CondtionType.Long)
                    $scope.SecondarySelected = removed.longList;
                else if ($scope.PrimarySelected.type == CondtionType.Decmal)
                    $scope.SecondarySelected = removed.decimalList;
                else
                    $scope.SecondarySelected = removed.stringList;

                $.each($scope.SecondaryCondtions, function (k, v) {
                    v.ShowColumn = false;
                })

                $scope.Search(function (result) {

                    var selectedRows = [];
                    $.each(result, function (k, v) {
                        var item = $scope.SecondarySelected.find(function (itm) {
                            return itm == v.id;
                        });

                        if (typeof (item) != 'undefined' && item != null) {
                            v.selected = true;
                            v.style = $scope.secondarystyle_selected;
                            selectedRows.push(k);
                        }
                    })

                    $scope.SearchResult = result;
                    $scope.irgrid.grid.setSelectedRows(selectedRows);
                    $scope.UpdatePercentage($scope.irgrid);
                    $scope.irgrid.UpdateData($scope.SearchResult);
                });
            }

            $scope.UpdatePercentage($scope.irgrid);
            $scope.irgrid.UpdateData($scope.SearchResult);
        };

        $scope.selected = function (item) {
            var addrule = false;
            $scope.SecondarySelected = [];
            $scope.SecondaryName = [];

            $.each(this.irgrid.GetSelectedRows(), function (k, v) {
                $scope.SecondarySelected.push(v.id);
                $scope.SecondaryName.push(v.name);
            });

            if ($scope.PrimarySelected != null) {
                if ($scope.SecondarySelected.length > 0) {
                    $scope.PrimarySelected.style = $scope.style_unselected;
                    addrule = true;
                }
                else {
                    $scope.PrimarySelected.style = $scope.style_unselected;
                }
            }

            if (item.type == CondtionType.Range) {
                $scope.newPrimary = item;
                $rootScope.Prompt('Range', 'Enter range as Start-Incrementer-End', function (value) {
                    var ranges = value.split('-');
                    if (ranges.length == 3) {
                        $scope.triggerSearch($scope.newPrimary, addrule, { 'start': ranges[0], 'incrementor': ranges[1], 'end': ranges[2], });
                    }
                    else {
                        ranges = value.splite(',');
                        if (ranges.length >= 2)
                            $scope.triggerSearch($scope.newPrimary, addrule, { 'start': 0, 'incrementor': 0, 'end': 0, direct: value });
                    }
                });
            }
            else {
                $scope.triggerSearch(item, addrule, null)
            }
        };

        $scope.triggerSearch = function (item, addrule, range) {

            if (item.id == Options.StockLocation && $scope.SearchCondition.reporttype == "0") {
                $rootScope.Toaster.Info("Warehouse Don't have stock location.");
                return;
            }


            if (addrule) {
                var rule = { 'option': $scope.PrimarySelected.id, 'type': $scope.PrimarySelected.type, 'typeName': $scope.PrimarySelected.name, 'primary': $scope.PrimarySelected, 'range': $scope.Range, 'nameList': $scope.SecondaryName };
                if ($scope.PrimarySelected.type == CondtionType.Date)
                    rule.dateList = $scope.SecondarySelected;
                else if ($scope.PrimarySelected.type == CondtionType.Long)
                    rule.longList = $scope.SecondarySelected;
                else if ($scope.PrimarySelected.type == CondtionType.Decmal)
                    rule.decimalList = $scope.SecondarySelected;
                else
                    rule.stringList = $scope.SecondarySelected;
                $scope.SecondaryCondtions.push(rule);
            }
            $scope.PrimarySelected = item;
            $scope.PrimarySelected.style = $scope.style_selected;
            $scope.SecondarySelected = [];
            $scope.SecondaryName = [];
            $scope.Range = range;
            $scope.SearchCondition.GroupColumn = null;
            $.each($scope.SecondaryCondtions, function (k, v) {
                v.ShowColumn = false;
            })
            $scope.Search();
        }

        $scope.SearchCondition = {
            reporttype: 0,
            locationid: 0,
            PrimaryCondtion: null,
            PrimaryRange: null,
            SecondaryCondtions: [],
            ShowReport: false,
            ShowAnalyis: true,
            ShowGroup: false,
            GroupColumn: null
        };
        $scope.SearchResult = [];

        $scope.Back = function () {
            $scope.SearchCondition.ShowAnalyis = true;
            $scope.SearchCondition.ShowReport = false;
            $scope.SearchCondition.ShowGroup = false;
        }

        $scope.Clear = function () {
            $scope.SearchCondition.PrimaryCondtion = null;
            $scope.SearchCondition.PrimaryRange = null;
            $scope.SearchCondition.SecondaryCondtions = [];
            $scope.SearchCondition.ShowReport = false;
            $scope.SearchCondition.ShowAnalyis = true;
            $scope.SearchCondition.ShowGroup = false;
            $scope.SearchCondition.GroupColumn = null;
            $scope.SearchResult = [];
            $scope.ReportResult = [];
            $scope.GroupResult = [];
            if ($scope.PrimarySelected != null) {
                $scope.PrimarySelected.style = $scope.style_unselected;
            }
            $scope.PrimarySelected = null;
            $scope.SecondarySelected = [];
            $scope.SecondaryName = [];
            $scope.SecondaryCondtions = [];
            $scope.Range = null;
            $scope.ReportList = [];
        }

        $scope.SortField = function (type, arrayElement, $event) {
            var icon = $($event.currentTarget).children().first();
            if (icon.hasClass('fa-sort-amount-asc')) {
                arrayElement.sort(function (x, y) {
                    if (x[type] < y[type]) { return -1; }
                    if (x[type] > y[type]) { return 1; }
                    return 0;
                });
                icon.removeClass('fa-sort-amount-asc');
                icon.addClass('fa-sort-amount-desc');
            }
            else {
                arrayElement.sort(function (x, y) {
                    if (x[type] < y[type]) { return 1; }
                    if (x[type] > y[type]) { return -1; }
                    return 0;
                });
                icon.removeClass('fa-sort-amount-desc');
                icon.addClass('fa-sort-amount-asc');
            }
        }

        $scope.Search = function (callback) {

            //$scope.SearchCondition.reporttype = $scope.SearchCondition.reporttype == "0" ? 0 : 1;
            $scope.SearchCondition.PrimaryCondtion = $scope.PrimarySelected.id;    // SEELCT SIZE
            $scope.SearchCondition.PrimaryRange = $scope.Range;
            // TWO CONDITION 1. PRODUCT = PANT AND 2. BRAND IN (LIVE-IN, VAN HEUSEN)
            $scope.SearchCondition.SecondaryCondtions = $scope.SecondaryCondtions;

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/StockAnalyser",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else if (typeof (callback) != 'undefined') {
                    $scope.ResetSelection();
                    callback(result);
                }
                else {
                    $scope.ResetSelection();
                    $scope.SearchResult = result;
                    $scope.ReportResult = [];

                    $scope.ConfigureGrid();
                    $scope.irgrid.grid.setSelectedRows([]);
                    $scope.UpdatePercentage($scope.irgrid);
                    $scope.irgrid.UpdateData($scope.SearchResult);
                }
            }).error(function (result) {
            });
        }

        $scope.ShowCurrentReport = function () {
            var report = $scope.GetCurrentReport();
            if (report.reporttype == 0)
                $scope.SearchCondition.reporttype = "0";
            else if (report.reporttype == 1)
                $scope.SearchCondition.reporttype = "1";
            else
                $scope.SearchCondition.reporttype = "3";

            $scope.PrimarySelected = $scope.Options.find(function (item) {
                return item.id == report.PrimaryCondtion;
            });

            $scope.PrimarySelected.style = $scope.style_selected;
            $scope.SearchCondition.PrimaryCondtion = report.PrimaryCondtion;
            $scope.Range = report.PrimaryRange;
            $scope.SecondaryCondtions = report.SecondaryCondtions;
            $scope.SearchCondition.Columns = [];
            $scope.SearchCondition.ColumnIndex = [];
            $.each($scope.SecondaryCondtions, function (k, v) {
                v.ShowColumn = false;
            })
            $scope.Search();
        }

        $scope.GetCurrentReport = function () {
            var report = $scope.ReportList.find(function (item) {
                return item.id == $scope.reportid;
            });
            if (typeof (report) == 'undefined')
                return null;

            var report = JSON.parse(report.data);
            report.id = $scope.reportid;
            return report;
        };

        $scope.ShowSaveDialog = function (report) {
            $scope.SearchCondition.Columns = [];
            $scope.SearchCondition.ColumnIndex = [];

            $scope.SecondarySelected = [];
            $scope.SecondaryName = [];
            $.each(this.irgrid.GetSelectedRows(), function (k, v) {
                $scope.SecondarySelected.push(v.id);
                $scope.SecondaryName.push(v.name);
            });

            var scondition = (JSON.parse(JSON.stringify($scope.SecondaryCondtions)));

            if ($scope.SecondarySelected.length > 0) {
                var rule = { 'option': $scope.PrimarySelected.id, 'type': $scope.PrimarySelected.type, 'typeName': $scope.PrimarySelected.name, 'primary': $scope.PrimarySelected, 'range': $scope.Range, 'nameList': $scope.SecondaryName };
                if ($scope.PrimarySelected.type == CondtionType.Date)
                    rule.dateList = $scope.SecondarySelected;
                else if ($scope.PrimarySelected.type == CondtionType.Long)
                    rule.longList = $scope.SecondarySelected;
                else
                    rule.stringList = $scope.SecondarySelected;
                scondition.push(rule);
            }
            //else {
            //    var rule = { 'option': $scope.PrimarySelected.id, 'type': $scope.PrimarySelected.type, 'typeName': $scope.PrimarySelected.name, 'primary': $scope.PrimarySelected, 'range': $scope.Range, 'nameList': $scope.SecondaryName };
            //    var secondaryselected = [];
            //    $.each($scope.SearchResult, function (k, v) {
            //        secondaryselected.push(v.id);
            //    });
            //    if ($scope.PrimarySelected.type == CondtionType.Date)
            //        rule.dateList = secondaryselected;
            //    else if ($scope.PrimarySelected.type == CondtionType.Long)
            //        rule.longList = secondaryselected;
            //    else
            //        rule.stringList = secondaryselected;
            //    scondition.push(rule);
            //}

            var dialog = ngDialog.open({
                template: 'SaveReport',
                data: {
                    'SecondaryCondtions': scondition,
                    'Parent': $scope,
                    'Report': (JSON.parse(JSON.stringify(report)))
                },
                width: '650px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {

                    $scope.GroupColumns = [{ 'id': '', 'name': 'None' }, { 'id': 'Product', 'name': 'Product' }, { 'id': 'Brand', 'name': 'Brand' }, { 'id': 'Style', 'name': 'Style' },
                    { 'id': 'Pattern', 'name': 'Pattern' }, { 'id': 'Size', 'name': 'Size' }, { 'id': 'Colour', 'name': 'Colour' },
                    { 'id': 'Type', 'name': 'Type' }, { 'id': 'Design', 'name': 'Design' }];

                    $.each($scope.ngDialogData.SecondaryCondtions, function (k, v) {
                        if (v.typeName != 'PRODUCT' && v.typeName != 'BRAND' && v.typeName != 'SIZE'
                            && v.typeName != 'PATTERN' && v.typeName != 'MATERIAL' && v.typeName != 'STYLE'
                            && v.typeName != 'COLOUR' && v.typeName != 'TYPE' && v.typeName != 'DESIGN')
                            $scope.GroupColumns.push({ 'id': '$' + v.typeName, 'name': v.typeName });
                    });

                    $scope.SecondaryCondtions = $scope.ngDialogData.SecondaryCondtions;
                    $scope.Parent = $scope.ngDialogData.Parent;
                    $scope.Report = {
                        'Title': '', 'Description': '', 'SecondaryCondtions': $scope.SecondaryCondtions,
                        'Column': {}, 'Mode': 'Save', 'id': 0, 'ResultColumn': { 'qty': true, 'value': true }
                    };

                    if ($scope.ngDialogData.Report != null)
                        $scope.Report = $scope.ngDialogData.Report;

                    $scope.Report.Changed = false;

                    $scope.Save = function () {
                        $scope.closeThisDialog($scope.Report);
                    };

                    $scope.ShowReport = function () {
                        $scope.Report.Mode = 'View';
                        $scope.closeThisDialog($scope.Report);
                    }

                    $scope.SelectAll = function (all) {
                        $scope.Report.Column.Product = $scope.Report.Column.Brand = $scope.Report.Column.Style = $scope.Report.Column.Pattern = all;
                        $scope.Report.Column.Size = $scope.Report.Column.Colour = $scope.Report.Column.Type = $scope.Report.Column.Design = all;
                    }

                    $scope.DeleteItem = function (item) {
                        $scope.Report.Changed = true;
                        $scope.SecondaryCondtions.splice($scope.SecondaryCondtions.indexOf(item), 1);
                    }

                    $scope.Delete = function () {
                        $scope.Report.Mode = 'Delete';
                        $scope.closeThisDialog($scope.Report);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        if (data.value.Mode == 'Save') {

                            data.value.reporttype = $scope.SearchCondition.reporttype;
                            data.value.PrimaryCondtion = $scope.SearchCondition.PrimaryCondtion;
                            data.value.PrimaryRange = $scope.SearchCondition.PrimaryRange;
                            var request = {
                                "Item": { 'type': 'STOCKANALYSER', 'name': data.value.Title, 'code': 'STOCKANALYSER', 'data': JSON.stringify(data.value), 'id': data.value.id },
                                "securitytoken": "String content"
                            }

                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "ReferencelistService/SaveDirect",
                                data: srequest
                            }).success(function (result) {

                                if (result != null && result.id > 0) {
                                    $rootScope.Toaster.Info("Saved Successfully.!");
                                    var exist = $scope.ReportList.find(function (item) {
                                        return item.id == result.id;
                                    })
                                    if (typeof (exist) == 'undefined' || exist == null)
                                        $scope.ReportList.push(result)
                                    $scope.reportid = result.id;
                                }

                            }).error(function (result) {
                            });
                        }
                        else if (data.value.Mode == "Delete") {
                            var exist = $scope.ReportList.find(function (item) {
                                return item.id == data.value.id;
                            })
                            if (typeof (exist) != 'undefined' || exist != null) {
                                $scope.ReportList.splice($scope.ReportList.indexOf(exist), 1);

                                var request = {
                                    "Item": exist,
                                    "securitytoken": "String content"
                                }

                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "ReferencelistService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result != null && result) {
                                        $rootScope.Toaster.Info("Deleted Successfully.!");
                                        $scope.reportid = result.id;
                                    }

                                }).error(function (result) {
                                });
                            }
                        }
                        else {
                            $scope.ReportCondition = (JSON.parse(JSON.stringify($scope.SearchCondition)));
                            $scope.ReportCondition.Columns = [];
                            $scope.ReportCondition.ColumnIndex = [];
                            $scope.ReportCondition.GroupColumn = data.value.GroupCoumn;
                            $scope.ReportCondition.ResultColumn = data.value.ResultColumn;
                            if (data.value.Column.Product)
                                $scope.ReportCondition.Columns.push('Product');
                            if (data.value.Column.Brand)
                                $scope.ReportCondition.Columns.push('Brand');
                            if (data.value.Column.Style)
                                $scope.ReportCondition.Columns.push('Style');
                            if (data.value.Column.Pattern)
                                $scope.ReportCondition.Columns.push('Pattern');
                            if (data.value.Column.Size)
                                $scope.ReportCondition.Columns.push('Size');
                            if (data.value.Column.Colour)
                                $scope.ReportCondition.Columns.push('Colour');
                            if (data.value.Column.Type)
                                $scope.ReportCondition.Columns.push('Type');
                            if (data.value.Column.Design)
                                $scope.ReportCondition.Columns.push('Design');

                            $.each($scope.ReportCondition.Columns, function (item) {
                                $scope.ReportCondition.ColumnIndex.push($scope.ReportCondition.ColumnIndex.length);
                            });

                            $scope.ReportCondition.PrimaryCondtion = $scope.PrimarySelected.id;
                            $scope.ReportCondition.PrimaryRange = $scope.Range;
                            $scope.ReportCondition.SecondaryCondtions = data.value.SecondaryCondtions;
                            request = {
                                "item": $scope.ReportCondition,
                                "securitytoken": "String content"
                            }

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "AnalyserService/StockAnalyser",
                                data: srequest
                            }).success(function (result) {

                                $scope.ReportCondition.AdditionalColumns = [];

                                $.each($scope.ReportCondition.SecondaryCondtions, function (k, v) {
                                    if (v.ShowColumn) {
                                        $scope.ReportCondition.AdditionalColumns.push(v.typeName);
                                    }
                                });

                                if (typeof ($scope.ReportCondition.GroupColumn) != 'undefined' &&
                                    $scope.ReportCondition.GroupColumn != null &&
                                    $scope.ReportCondition.GroupColumn.length > 0) {

                                    /*var ResultGroups = [];
                                    for (var idx = 0 ; idx < result.ResultGroups.length; idx = idx + 2)
                                        ResultGroups.push(result.ResultGroups[idx]);
                                    result.ResultGroups = ResultGroups;
                                    */

                                    $scope.GroupResult = result;

                                    $scope.SearchCondition.ShowReport = false;
                                    $scope.SearchCondition.ShowAnalyis = false;
                                    $scope.SearchCondition.ShowGroup = true;

                                    $timeout(function () {
                                        $scope.ShowGroupResult();
                                    }, 100);

                                    //$timeout(function () {
                                    //    var headers = $('#groupstockanalyser thead tr th');
                                    //    $.each(headers, function (k, v) {
                                    //        var w = $(v).width().toString();
                                    //        if (w.endsWith('px')) {
                                    //            w = w.split('.')[0] + 'px';
                                    //        }
                                    //        $(v).children().first().width(w);
                                    //    })

                                    //}, 100)
                                }
                                else {
                                    $scope.ReportResult = result;
                                    $scope.GroupResult = {};
                                    $scope.SearchCondition.ShowReport = true;
                                    $scope.SearchCondition.ShowAnalyis = false;
                                    $scope.SearchCondition.ShowGroup = false;
                                    $timeout(function () {
                                        $scope.ShowReportResult();
                                    }, 100);
                                }
                            }).error(function (result) {
                            });

                        }
                    }
                }
            });
        }

        $scope.igrid = null;
        $scope.ShowGroupResult = function () {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }
            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, false, true, 0);

            var groupColumns = [];

            $.each($scope.GroupResult.Columns, function (k, v) {
                var column = igrid.AddCustomColumn('Col-' + v.Index, v.Header, 150, function (item, col) {
                    if (item.String[col.internal.Index] == null || item.String[col.internal.Index] == '')
                        return 'Un Assigned';
                    return item.String[col.internal.Index];
                });
                column.internal = v;
            });

            groupColumns.push(
                {
                    id: 'Filter',
                    name: (typeof ($scope.ReportCondition.GroupColumn) != 'undefined' &&
                        $scope.ReportCondition.GroupColumn != null &&
                        $scope.ReportCondition.GroupColumn.length > 0) ? $scope.ReportCondition.GroupColumn.replace('$', '') : 'Filter',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            $.each($scope.GroupResult.Groups, function (k, v) {

                igrid.columns = [];

                if ($scope.ReportCondition.ResultColumn.qty) {
                    var column = igrid.AddCustomColumn('Grp-Qty-' + v.Index, 'Qty', 100, function (item, col) {
                        //return $filter('IndianNumber')(item.GroupValues[col.internal.Index], 0);
                        var cellresult = item.GroupValues[col.internal.Index];
                        if (cellresult != null) return $filter('IndianNumber')(cellresult.qty, 2);
                        return '';
                    });
                    column.internal = v;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var cellresult = item.GroupValues[col.internal.Index];
                        if (cellresult != null) return cellresult.qty;
                        return 0;
                    }
                }

                if ($scope.ReportCondition.ResultColumn.value) {
                    var column = igrid.AddCustomColumn('Grp-Val-' + v.Index, 'Value', 100, function (item, col) {
                        //return $filter('IndianNumber')(item.GroupValues[col.internal.Index], 0);
                        var cellresult = item.GroupValues[col.internal.Index];
                        if (cellresult != null) return $filter('IndianNumber')(cellresult.value, 2);
                        return '';
                    });
                    column.internal = v;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var cellresult = item.GroupValues[col.internal.Index];
                        if (cellresult != null) return cellresult.value;
                        return 0;
                    }
                }

                if ($scope.analyser_stock_purchasesales && $scope.ReportCondition.ResultColumn.qty) {
                    var column = igrid.AddCustomColumn('Grp-PQTY-' + v.Index, 'P.Qty', 100, function (item, col) {
                        //return $filter('IndianNumber')(item.GroupValues[col.internal.Index], 0);
                        var cellresult = item.GroupValues[col.internal.Index];
                        if (cellresult != null) return $filter('IndianNumber')(cellresult.iqty, 2);
                        return '';
                    });
                    column.internal = v;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var cellresult = item.GroupValues[col.internal.Index];
                        if (cellresult != null) return cellresult.iqty;
                        return 0;
                    }

                }

                if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                    if ($scope.ReportCondition.ResultColumn.value) {
                        var column = igrid.AddCustomColumn('Grp-PValue-' + v.Index, 'P.Cost', 100, function (item, col) {
                            //return $filter('IndianNumber')(item.GroupValues[col.internal.Index], 0);
                            var cellresult = item.GroupValues[col.internal.Index];
                            if (cellresult != null) return $filter('IndianNumber')(cellresult.ivalue, 2);
                            return '';
                        });
                        column.internal = v;
                        column.cssClass = 'rightalign';
                        column.total = true;
                        column.getValue = function (item, col) {
                            var cellresult = item.GroupValues[col.internal.Index];
                            if (cellresult != null) return cellresult.ivalue;
                            return 0;
                        }
                    }
                }

                var header = v.Header == null ? 'Un Assigned' : v.Header;
                if (igrid.columns.length > 0) {
                    groupColumns.push(
                        {
                            id: header,
                            name: header,
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                }
            });

            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            igrid.grid.setOptions({ 'frozenColumn': $scope.GroupResult.Columns.length - 1 });
            $scope.igrid.UpdateData($scope.GroupResult.Items);
        }


        $scope.ShowReportResult = function () {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }
            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, false, true, 0);
            for (var column = 0; column < $scope.ReportResult.columns.length; column++) {
                var col = igrid.AddCustomColumn('col-' + column, $scope.ReportResult.columns[column], 150, function (item, col) {
                    return item.String[col.StringIndex];
                });
                col.StringIndex = column;
            }

            for (var column = 0; column < $scope.ReportResult.additionalcolumns.length; column++) {
                var col = igrid.AddCustomColumn('addl-' + column, $scope.ReportResult.additionalcolumns[column].headername, 150, function (item, col) {
                    return item.String[col.StringIndex];
                });
                col.StringIndex = $scope.ReportResult.additionalcolumns[column].index;
            }

            if ($scope.ReportCondition.ResultColumn.qty) {
                igrid.AddFormatedColumn('qty', 'Qty', 100, 'N2', 2);
            }

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                if ($scope.ReportCondition.ResultColumn.value) {
                    igrid.AddFormatedColumn('value', 'Cost Price', 100, 'N2', 2);
                    var column = igrid.AddCustomColumn('percentage', '%', 100, function (item, col) {
                        if (typeof (col.totalValue) != 'undefined') {
                            return (item.value / col.totalValue * 100).toFixed(2) + '%';
                        }
                    });
                    column.cssClass = 'rightalign';
                    igrid.AddFormatedColumn('svalue', 'Sale Price', 100, 'N2', 2);
                }
            }

            if ($scope.analyser_stock_purchasesales)
                $scope.irgrid.AddFormatedColumn('iqty', 'P.Qty', 100, 'N2', 2).total = true;

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                $scope.irgrid.AddFormatedColumn('ivalue', 'P.Cost', 100, 'N2', 2).total = true;
            }

            /*igrid.AddCustomColumn('spercentage', '%', 100, function (item, col) {
                if (typeof (col.totalValue) != 'undefined') {
                    return (item.svalue / col.totalValue * 100).toFixed(2) + '%';
                }
            }).cssClass = 'rightalign';*/

            igrid.Initialize();
            $scope.igrid = igrid;
            for (var i = 0; i < $scope.ReportResult.items.length; i++)
                $scope.ReportResult.items[i].id = i;


            var percentageValueColumn = $scope.igrid.grid.getColumns()[$scope.igrid.grid.getColumnIndex('percentage')];
            if (typeof (percentageValueColumn) != 'undefined')
                percentageValueColumn.totalValue = 0;

            /*var percentageSValueColumn = $scope.igrid.grid.getColumns()[$scope.igrid.grid.getColumnIndex('spercentage')];
            if (typeof (percentageSValueColumn) != 'undefined')
                percentageSValueColumn.totalValue = 0;*/

            $.each($scope.SearchResult, function (k, v) {
                if (typeof (percentageValueColumn) != 'undefined')
                    percentageValueColumn.totalValue += v.value;
                /*if (typeof (percentageSValueColumn) != 'undefined')
                    percentageSValueColumn.totalValue += v.svalue;*/
            });


            $scope.igrid.UpdateData($scope.ReportResult.items);
        }


        $rootScope.$on('OnWindowsSizeChange',
            function (message) {
                message = null;
            });

        $scope.ConfigureGrid = function () {
            if ($scope.irgrid != null) {
                $scope.irgrid.grid.destroy();
                $('#gridResultContainer').empty();
            }

            $scope.irgrid = new InfyGrid('gridResultContainer', '450px', '100%', false, true, true, 0);
            $scope.irgrid.options.showHeaderRow = true;
            $scope.irgrid.EnableTotal();
            var groupColumns = [];
            $scope.irgrid.AddSimpleColumn('name', '', 250);

            groupColumns.push(
                {
                    id: 'name',
                    name: 'Description',
                    headerCssClass: 'text-center',
                    columns: $scope.irgrid.columns
                });

            if ($scope.analyser_stock_purchasesales && $scope.SearchCondition.reporttype == '3') {

                $scope.irgrid.columns = [];
                $scope.irgrid.AddFormatedColumn('iqty', 'Qty', 100, 'N2', 2).total = true;
                if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                    $scope.irgrid.AddFormatedColumn('ivalue', 'Value', 100, 'N2', 2).total = true;
                }
                groupColumns.push(
                    {
                        id: 'Purchase',
                        name: 'Purchase',
                        headerCssClass: 'text-center',
                        columns: $scope.irgrid.columns
                    });


                $scope.irgrid.columns = [];
                var column = $scope.irgrid.AddCustomColumn('saleqty', 'Qty', 100, function (item, col) {
                    return (item.iqty - item.qty).toFixed(2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return (item.iqty - item.qty).toFixed(2);
                }

                if (typeof ($scope.Permission.p_SHOWSELLINGVALUE) != 'undefined' && !$scope.Permission.p_SHOWSELLINGVALUE.view) {
                    var column = $scope.irgrid.AddCustomColumn('salevalue', 'Value', 100, function (item, col) {
                        return (item.ivalue - item.value).toFixed(2);
                    });
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        return (item.ivalue - item.value).toFixed(2);
                    }
                }
                groupColumns.push(
                    {
                        id: 'Sales',
                        name: 'Sales',
                        headerCssClass: 'text-center',
                        columns: $scope.irgrid.columns
                    });
            }

            $scope.irgrid.columns = [];
            $scope.irgrid.AddFormatedColumn('qty', 'Qty', 100, 'N2', 2).total = true;

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                $scope.irgrid.AddFormatedColumn('value', 'Taxable', 100, 'N2', 2).total = true;
                $scope.irgrid.AddFormatedColumn('buyingcost', 'Cost Price', 100, 'N2', 2).total = true;
            }
            if (typeof ($scope.Permission.p_SHOWSELLINGVALUE) != 'undefined' && !$scope.Permission.p_SHOWSELLINGVALUE.view) {
                $scope.irgrid.AddFormatedColumn('sellingprice', 'Sale Price', 100, 'N2', 2).total = true;
            }

            var percentageValueColumn = $scope.irgrid.AddCustomColumn('percentage', '%', 100, function (item, col) {
                //var value = ((item.qty / item.iqty) * 100).toFixed(2);
                //if (isNaN(value))
                //    return '0%';
                //return value + '%';
                if (typeof (col.totalValue) != 'undefined') {
                    return (item.value / col.totalValue * 100).toFixed(2) + '%';
                }
                return '0%';
            });

            percentageValueColumn.sortable = true;
            percentageValueColumn.cssClass = 'rightalign';
            percentageValueColumn.getValue = function (item, col) {
                if (typeof (col.totalValue) != 'undefined') {
                    return (item.value / col.totalValue * 100);
                }
                return 0;
            }

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                var column = $scope.irgrid.AddCustomColumn('margin', 'Mark UP %', 100, function (item, col) {
                    var value = ((item.svalue - item.value) / item.value * 100).toFixed(2);
                    if (isNaN(value))
                        return '0%';
                    return value + '%';
                });
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.getValue = function (item, col) {
                    var value = ((item.svalue - item.value) / item.value * 100).toFixed(2);
                    if (isNaN(value))
                        return 0;
                    return value;
                }

                var column = $scope.irgrid.AddCustomColumn('margin', 'Mark DN %', 100, function (item, col) {
                    var value = (100 - ((item.value / item.svalue) * 100)).toFixed(2);
                    if (isNaN(value))
                        return '0%';
                    return value + '%';
                });
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.getValue = function (item, col) {
                    var value = ((item.svalue - item.value) / item.value * 100).toFixed(2);
                    if (isNaN(value))
                        return 0;
                    return value;
                }
            }

            groupColumns.push(
                {
                    id: 'Stock',
                    name: 'Stock',
                    headerCssClass: 'text-center',
                    columns: $scope.irgrid.columns
                });

            $scope.irgrid.columns = groupColumns;
            $scope.irgrid.Initialize();
            $scope.irgrid.grid.setOptions({ 'frozenColumn': 1 });
        }

        $scope.UpdatePercentage = function (grid) {
            var percentageValueColumn = grid.grid.getColumns()[grid.grid.getColumnIndex('percentage')];
            if (typeof (percentageValueColumn) != 'undefined') {
                percentageValueColumn.totalValue = 0;
                $.each($scope.SearchResult, function (k, v) {
                    percentageValueColumn.totalValue += v.value;
                });
            }
        }

        $scope.printDiv = function (div, title) {

            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
                var titlele = angular.element('#apptitle').context.title = title;

                var docHead = document.head.outerHTML;
                var printContents = document.getElementById(div).outerHTML;
                var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

                var newWin = window.open("", "_blank", winAttr);
                var writeDoc = newWin.document;
                writeDoc.open();
                writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
                writeDoc.close();
                newWin.focus();

                angular.element('#apptitle').context.title = otitle;
            }, 100);

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

        $timeout(function () {

            $('.gridHScroll').scroll(function () {
                $('.gridVScroll').width($('.gridHScroll').width() + $('.gridHScroll').scrollLeft());
            });

        }, 10);


    }]);