
var app = angular.module('InfinityLab');

app.controller('SalesAnalyserController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.Permission = UserDetails.Info;
        $scope.SearchRef = restService.SearchReference;
        $scope.PrimarySelected = null;
        $scope.SecondarySelected = [];
        $scope.SecondaryName = [];
        $scope.SecondaryCondtions = [];
        $scope.Range = null;
        $scope.ShowImage = false;
        $scope.ReportList = [];
        $scope.secondarystyle_selected = {
            "color": "white",
            "background-color": "coral",
        };

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

        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.ReportList = restService.Get('ReferencelistService/DIRECT-SALESANALYSER', true).then(function (results) {
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
        { 'id': 2, 'name': 'SALES' }];

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
            SaleMargin_Range: 38,
            RetailMargin_Range: 39,
            Agent: 40,
            Counter: 48,
            Company: 49,
            ProductGroup: 50,
            ReturnReason: 51,
            SaleReason: 52,
            Area: 53,
            Season: 54,
            PurchaseDate: 84,
            Customer_Religion: 93,
            SourceSupplier: 94,
            ProductCode: 98,
            Fit: 99,
            Sleeve: 100,
            TaxPercentage: 102,
            GLNNO: 103,
            Floor: 104,
            StockMarker: 106,
            JobworkSupplier: 110,
            SellingMode: 111,
            BillType: 113,
            PriceTag: 114,
            BarcodeGroup: 117,
            Incharge: 126,
            Custom_ProductGroups: 200,
            Custom_Fields: 201
        }

        $scope.ReportType = [{ 'id': 0, 'name': 'WAREHOUSE STOCK' },
        { 'id': 1, 'name': 'RETAIL STOCK' },
        { 'id': 2, 'name': 'SALES' }];

        $scope.Options = [
            { 'id': Options.Company, 'name': 'COMPANY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Product, 'name': 'PRODUCT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Brand, 'name': 'BRAND', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Size, 'name': 'SIZE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Pattern, 'name': 'PATTERN', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Material, 'name': 'MATERIAL', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Style, 'name': 'STYLE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Colour, 'name': 'COLOUR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Type, 'name': 'TYPE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Item, 'name': 'ITEM', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Section, 'name': 'SECTION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            //{ 'id': Options.StockMarker, 'name': 'STOCK MARKER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.ProductGroup, 'name': 'PRODUCT GROUP', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.BarcodeGroup, 'name': 'BARCODE GROUP', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.ProductCode, 'name': 'PRODUCT CODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.Fit, 'name': 'FIT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Sleeve, 'name': 'SLEEVE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.PriceTag, 'name': 'PRICE TAG', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },

            { 'id': Options.SaleLocation, 'name': 'SALE LOCATION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            { 'id': Options.StockLocation, 'name': 'STOCK LOCATION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Design, 'name': 'DESIGN', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.SaleRate, 'name': 'SALE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.PurchaseRate, 'name': 'PURCHASE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.DiscountRate, 'name': 'DISCOUNT RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.SoldRate, 'name': 'SALE PRICE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.Sales },
            { 'id': Options.SaleRate_Range, 'name': 'SALE RATE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            { 'id': Options.StockAge_Range, 'name': 'STOCK AGE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.BothStock },
            { 'id': Options.PurchaseRate_Range, 'name': 'PURCHASE RATE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            { 'id': Options.DiscountRate_Range, 'name': 'DISCOUNT RATE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            { 'id': Options.SoldRate_Range, 'name': 'SOLD RATE RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales },
            { 'id': Options.Margin_Range, 'name': 'MARGIN RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.Sales },
            { 'id': Options.RetailMargin_Range, 'name': 'RETAIL MARGIN RANGE', 'style': $scope.style_unselected, 'type': CondtionType.Range, 'role': Roles.All },
            { 'id': Options.LREntryNo, 'name': 'LRENTRY NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.InvoiceNo, 'name': 'INVOICE NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.InvDate, 'name': 'INVOICE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All },
            { 'id': Options.PurchaseDate, 'name': 'PURCHASE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All },
            { 'id': Options.SaleYear, 'name': 'SALE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            { 'id': Options.PurchaseYear, 'name': 'PURCHASE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.PurchaseMonth, 'name': 'PURCHASE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SaleMonth, 'name': 'SALE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            { 'id': Options.BillNo, 'name': 'BILL NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.Sale },
            { 'id': Options.BillDate, 'name': 'BILL DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.Sale },
            { 'id': Options.Supplier, 'name': 'SUPPLIER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Agent, 'name': 'AGENT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Salesman, 'name': 'SALES MAN', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sale },
            { 'id': Options.Supplier_City, 'name': 'SUPPLIER CITY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Barcode, 'name': 'BARCODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.GLNNO, 'name': 'GLN NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.HSNCode, 'name': 'HSN CODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.Tax, 'name': 'SALES TAX', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.TaxPercentage, 'name': 'TAX PERCENTAGE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.SaleWeekDay, 'name': 'SALE WEEK DAY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sale },
            { 'id': Options.SaleMonthDay, 'name': 'SALE MONTH DAY', 'style': $scope.style_unselected, 'type': CondtionType.DayRange, 'role': Roles.Sale },
            { 'id': Options.ReturnReason, 'name': 'RETURN REASON', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SaleReason, 'name': 'SALE REASON', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Area, 'name': 'SALE AREA', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Counter, 'name': 'COUNTER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Floor, 'name': 'FLOOR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Season, 'name': 'SEASON', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SourceSupplier, 'name': 'SOURCE SUPPLIER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.JobworkSupplier, 'name': 'JOB WORKER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SellingMode, 'name': 'SELLING MODE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.BillType, 'name': 'BILL TYPE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
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
                    $scope.ConfigureGrid();
                    $scope.SearchResult = result;
                    $scope.irgrid.grid.setSelectedRows(selectedRows);
                    $scope.UpdatePercentage();
                    $scope.irgrid.UpdateData($scope.SearchResult);
                });
            }

            $scope.UpdatePercentage();
            $scope.irgrid.UpdateData($scope.SearchResult);
        };

        $scope.selected = function (item) {
            var addrule = false;
            $scope.SecondarySelected = [];
            $scope.SecondaryName = [];

            $.each(this.irgrid.GetSelectedRows(), function (k, v) {
                if (v.id == "null") {
                    if ($scope.PrimarySelected.type == 2 || $scope.PrimarySelected.type == 4)
                        $scope.SecondarySelected.push('0');
                    else
                        $scope.SecondarySelected.push('null');
                }
                else {
                    $scope.SecondarySelected.push(v.id);
                }
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
                        ranges = value.split(',');
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
            var selectedall = $scope.SearchResult && $scope.SearchResult != null &&
                $scope.SecondarySelected && $scope.SecondarySelected != null &&
                $scope.SearchResult.length == $scope.SecondarySelected.length;

            if (addrule) {
                var rule = {
                    'option': $scope.PrimarySelected.id,
                    'type': $scope.PrimarySelected.type,
                    'typeName': $scope.PrimarySelected.name,
                    'primary': $scope.PrimarySelected,
                    'range': $scope.Range,
                    'nameList': selectedall ? [] : $scope.SecondaryName,
                    'selectedall': selectedall,
                    'customoption': $scope.PrimarySelected.coption
                };
                if (!selectedall) {
                    if ($scope.PrimarySelected.type == CondtionType.Date)
                        rule.dateList = $scope.SecondarySelected;
                    else if ($scope.PrimarySelected.type == CondtionType.Long)
                        rule.longList = $scope.SecondarySelected;
                    else if ($scope.PrimarySelected.type == CondtionType.Decmal)
                        rule.decimalList = $scope.SecondarySelected;
                    else
                        rule.stringList = $scope.SecondarySelected;
                }
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

            $scope.SearchCondition.reporttype = $scope.SearchCondition.reporttype == "0" ? 0 : 1;
            $scope.SearchCondition.PrimaryCondtion = $scope.PrimarySelected.id;    // SEELCT SIZE
            $scope.SearchCondition.CustomOption = $scope.PrimarySelected.coption;    // SEELCT SIZE
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
                url: "AnalyserService/SalesAnalyser",
                data: srequest
            }).success(function (result) {
                $scope.ShowImage = $scope.PrimarySelected.name == "BARCODE";
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
                    $scope.irgrid.grid.setSelectedRows([]);
                    $scope.UpdatePercentage();
                    $scope.irgrid.UpdateData($scope.SearchResult);
                }
            }).error(function (result) {
            });
        }

        $scope.ShowCurrentReport = function () {
            var report = $scope.GetCurrentReport();
            $scope.SearchCondition.reporttype = "2";

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

        $scope.ShowImageDialog = function () {
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
                var rule = {
                    'option': $scope.PrimarySelected.id, 'type': $scope.PrimarySelected.type, 'typeName': $scope.PrimarySelected.name,
                    'primary': $scope.PrimarySelected, 'range': $scope.Range, 'nameList': $scope.SecondaryName, 'customoption': $scope.PrimarySelected.coption
                };
                if ($scope.PrimarySelected.type == CondtionType.Date)
                    rule.dateList = $scope.SecondarySelected;
                else if ($scope.PrimarySelected.type == CondtionType.Long)
                    rule.longList = $scope.SecondarySelected;
                else
                    rule.stringList = $scope.SecondarySelected;
                scondition.push(rule);
            }

            var dialog = ngDialog.open({
                template: 'ImageReport',
                data: {
                    'SecondaryCondtions': scondition,
                    'Parent': $scope,
                    'Report': (JSON.parse(JSON.stringify(report)))
                },
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SHOWVALUE = !$scope.ngDialogData.Parent.Permission.p_SHOWVALUE.view;
                    $scope.GroupColumns = [{ 'id': '', 'name': 'None' }, { 'id': 'Product', 'name': 'Product' }, { 'id': 'Brand', 'name': 'Brand' }, { 'id': 'Style', 'name': 'Style' },
                    { 'id': 'Pattern', 'name': 'Pattern' }, { 'id': 'Size', 'name': 'Size' },
                    { 'id': 'Colour', 'name': 'Colour' },
                    { 'id': 'Type', 'name': 'Type' },
                    { 'id': 'Fit', 'name': 'Fit' },
                    { 'id': 'Sleeve', 'name': 'Sleeve' },
                    { 'id': 'Design', 'name': 'Design' }];

                    $.each($scope.ngDialogData.SecondaryCondtions, function (k, v) {
                        if (v.typeName != 'PRODUCT' && v.typeName != 'BRAND' && v.typeName != 'SIZE'
                            && v.typeName != 'PATTERN' && v.typeName != 'MATERIAL' && v.typeName != 'STYLE'
                            && v.typeName != 'COLOUR' && v.typeName != 'TYPE' && v.typeName != 'DESIGN'
                            && v.typeName != 'FIT' && v.typeName != 'SLEEVE')
                            $scope.GroupColumns.push({ 'id': '$' + v.typeName, 'name': v.typeName });
                    });

                    $scope.SecondaryCondtions = $scope.ngDialogData.SecondaryCondtions;
                    $scope.Parent = $scope.ngDialogData.Parent;
                    $scope.Report = {
                        'Title': '', 'Description': '', 'SecondaryCondtions': $scope.SecondaryCondtions,
                        'Column': {}, 'Mode': 'Save', 'id': 0,
                        'ResultColumn': { 'qty': $scope.SHOWVALUE, 'value': $scope.SHOWVALUE }
                    };
                    $scope.RowCount = '3';
                    $scope.ColumnCount = '2';

                    if ($scope.ngDialogData.Report != null)
                        $scope.Report = $scope.ngDialogData.Report;

                    $scope.Report.Changed = false;


                    $scope.ShowReport = function () {
                        $scope.Report.Mode = 'View';
                        $scope.Report.SecondaryCondtions = $scope.SecondaryCondtions;
                        $scope.Report.DetailPosition = $scope.DetailPosition;
                        $scope.Report.RowCount = $scope.RowCount;
                        $scope.Report.ColumnCount = $scope.ColumnCount;
                        $scope.closeThisDialog($scope.Report);
                    }

                    $scope.DownloadReport = function () {
                        $scope.Report.Mode = 'Download';
                        $scope.Report.SecondaryCondtions = $scope.SecondaryCondtions;
                        $scope.Report.DetailPosition = $scope.DetailPosition;
                        $scope.Report.RowCount = $scope.RowCount;
                        $scope.Report.ColumnCount = $scope.ColumnCount;
                        $scope.closeThisDialog($scope.Report);
                    }

                    $scope.SelectAll = function (all) {
                        $scope.Report.Column.Product = $scope.Report.Column.Brand = $scope.Report.Column.Style = $scope.Report.Column.Pattern = all;
                        $scope.Report.Column.Size = $scope.Report.Column.Colour = $scope.Report.Column.Type = $scope.Report.Column.Design = all;
                        $scope.Report.Column.Fit = $scope.Report.Column.Sleeve = all;
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
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$escape' && data.value != '$document') {


                        $scope.ReportCondition = (JSON.parse(JSON.stringify($scope.SearchCondition)));
                        $scope.ReportCondition.Columns = [];
                        $scope.ReportCondition.ColumnIndex = [];
                        $scope.ReportCondition.GroupColumn = data.value.GroupCoumn;
                        $scope.ReportCondition.ResultColumn = data.value.ResultColumn;
                        $scope.ReportCondition.Columns.push('ImageID');

                        var barcodecondition = data.value.SecondaryCondtions.find(function (item) {
                            return item.typeName == 'BARCODE';
                        });

                        if (typeof (barcodecondition) == 'undefined' || barcodecondition == null)
                            $scope.ReportCondition.Columns.push('Barcode');

                        if (typeof (data.value.Column) != 'undefined') {
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
                            if (data.value.Column.Fit)
                                $scope.ReportCondition.Columns.push('Fit');
                            if (data.value.Column.Sleeve)
                                $scope.ReportCondition.Columns.push('Sleeve');
                            if (data.value.Column.Type)
                                $scope.ReportCondition.Columns.push('Type');
                            if (data.value.Column.Design)
                                $scope.ReportCondition.Columns.push('Design');
                        }


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
                            url: "AnalyserService/SalesAnalyser",
                            data: srequest
                        }).success(function (result) {

                            if (data.value.Mode == 'Download') {
                                $scope.DownloadImageResult(result, data.value);
                            }
                            else {
                                $scope.ImageResult = result;
                                $timeout(function () {
                                    $scope.ShowImageResult(result, data.value);
                                }, 100);
                            }
                        }).error(function (result) {
                        });

                    }
                }
            });
        }

        $scope.DownloadImageResult = function (result, report) {

            report.datasource = result;

            request = {
                "item": report,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/ImagePDFGeneration",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length == 16) {
                    $rootScope.Download("AnalyserService/ImagePDFGeneration/" + result, 'images.pdf', 'application/pdf');
                    return;

                    $timeout(function () {
                        var link = document.createElement('a');
                        link.download = "images.pdf";
                        link.href = "AnalyserService/ImagePDFGeneration/" + result;
                        document.body.appendChild(link);
                        link.click();
                        link.remove();
                    }, 100);
                }
                else {
                    $rootScope.Toaster.Info(result);
                }
            }).error(function (result) {
            });
        }

        $scope.ShowImageResult = function (result, report) {

            var size = {
                'width': $('#gridResultContainer').width(), 'height': $('#report').height()
            }

            if ($scope.irgrid != null) {
                $scope.irgrid.grid.destroy();
                $('#gridResultContainer').empty();
            }

            var row = report.RowCount ? parseInt(report.RowCount) : 4;
            var col = report.ColumnCount ? parseInt(report.ColumnCount) : 2;
            var rowheight = (size.height - 50) / row;
            var imageheight = rowheight / 4 * 3;
            var descriptionwidth = report.DetailPosition == '3' || report.DetailPosition == '4' ? 40 + 150 : 40;
            var columnwidth = (size.width - 50 - (descriptionwidth * col)) / col;
            var groupresults = [];
            var imageindex = 0;
            var urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';
            for (var i = 0; i < result.items.length; i = i + col) {
                var row = {};
                var index = 0;
                for (var j = i; j < result.items.length && index < col; j++) {
                    row['col_' + index] = result.items[j];
                    row['col_' + index].Selected = false;
                    row['col_' + index].id = result.items[j].String[1];
                    index++;
                }
                groupresults.push(row);
            }

            $scope.irgrid = new InfyGrid('gridResultContainer', '450px', '100%', false, false, true, 0);
            $scope.irgrid.options.rowHeight = rowheight;
            $scope.irgrid.options.showHeaderRow = true;

            if (report.DetailPosition != '0') {
                $scope.irgrid.displaytemplate = '<span>';
                $.each(result.columns, function (k, v) {
                    if (v != 'ImageID')
                        $scope.irgrid.displaytemplate += v + ':${String[' + k + ']}<br>';
                });
                $.each(result.additionalcolumns, function (k, v) {
                    $scope.irgrid.displaytemplate += v.headername + ':${String[' + v.index + ']}<br>';
                });

                $scope.irgrid.displaytemplate += 'Stock:${qty}<br>';

                if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                    $scope.irgrid.displaytemplate += 'Taxable:${value}<br>';
                    $scope.irgrid.displaytemplate += 'Cost:${buyingcost}<br>';
                }
                if (typeof ($scope.Permission.p_SHOWSELLINGVALUE) != 'undefined' && !$scope.Permission.p_SHOWSELLINGVALUE.view) {
                    $scope.irgrid.displaytemplate += 'Price:${sellingprice}';
                }
                $scope.irgrid.displaytemplate += '</span>';

                $scope.irgrid.imgtemplate = '<div style="height: ' + imageheight + 'px;display: flex;align-items: center;justify-content: center;"><img style="width:100%;height:100%" src="' + urlprefix + '${String[0]}"/></div>';
                if (report.DetailPosition == '1') {
                    $scope.irgrid.imgtemplate = $scope.irgrid.imgtemplate + $scope.irgrid.displaytemplate;
                } else if (report.DetailPosition == '2') {
                    $scope.irgrid.imgtemplate = $scope.irgrid.displaytemplate + $scope.irgrid.imgtemplate;
                }
            }




            for (var i = 0; i < col; i++) {
                var column = $scope.irgrid.AddCustomColumn('select_' + i, '', 40, function (item, col) {
                    if (item['col_' + col.Index]) {
                        if (item['col_' + col.Index].Selected)
                            return '<input type="checkbox" style="z-index: auto;margin-left:10px" checked>';
                        else
                            return '<input type="checkbox" style="z-index: auto;margin-left:10px">';
                    }
                    return '';
                });
                column.Index = i;

                if (report.DetailPosition == '3') {
                    column = $scope.irgrid.AddCustomColumn('description_' + i, '', 150, function (item, col) {
                        if (item['col_' + col.Index])
                            return $("<div />").append($.tmpl(col.template, item['col_' + col.Index])).html();
                        return '';
                    });
                    column.Index = i;
                    column.template = $scope.irgrid.displaytemplate;
                    column.enablefilter = true;
                    column.getValue = function (item, col) {
                        if (item['col_' + col.Index])
                            return $("<div />").append($.tmpl(col.template, item['col_' + col.Index])).text();
                        return '';
                    }
                }

                if (report.DetailPosition == '3' || report.DetailPosition == '4') {
                    column = $scope.irgrid.AddCustomColumn('img_' + i, '', columnwidth, function (item, col) {
                        if (item['col_' + col.Index]) {
                            if (item['col_' + col.Index].String[0] != '')
                                return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img style="width:100%;height:100%" src="' + col.urlprefix + item['col_' + col.Index].String[0] + '"/></div>';
                            return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img src="clientside/dist/img/no-image.png"/></div>';
                        }
                        return '';
                    });
                    column.Index = i;
                    column.urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';
                }
                else {
                    column = $scope.irgrid.AddCustomColumn('img_' + i, '', 150, function (item, col) {
                        if (item['col_' + col.Index]) {
                            if (item['col_' + col.Index].String[0] != '')
                                return $("<div />").append($.tmpl(col.imgtemplate, item['col_' + col.Index])).html();
                            else
                                return $("<div />").append($.tmpl(col.template, item['col_' + col.Index])).html();
                        }
                        return '';
                    });
                    column.Index = i;
                    column.template = $scope.irgrid.displaytemplate;
                    column.imgtemplate = $scope.irgrid.imgtemplate;
                    column.enablefilter = true;
                    column.urlprefix = urlprefix;
                    column.getValue = function (item, col) {
                        if (item['col_' + col.Index])
                            return $("<div />").append($.tmpl(col.template, item['col_' + col.Index])).text();
                        return '';
                    }
                }

                if (report.DetailPosition == '4') {
                    column = $scope.irgrid.AddCustomColumn('description_' + i, '', 150, function (item, col) {
                        if (item['col_' + col.Index])
                            return $("<div />").append($.tmpl(col.template, item['col_' + col.Index])).html();
                        return '';
                    });
                    column.Index = i;
                    column.template = $scope.irgrid.displaytemplate;
                    column.enablefilter = true;
                    column.getValue = function (item, col) {
                        if (item['col_' + col.Index])
                            return $("<div />").append($.tmpl(col.template, item['col_' + col.Index])).text();
                        return '';
                    }
                }
            }

            $scope.irgrid.Initialize();
            $scope.irgrid.grid.onClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id.startsWith('select_')) {
                    item = args.grid.getDataItem(args.row);
                    var colindex = parseInt(args.grid.getColumns()[args.cell].id.split('_')[1]);
                    item['col_' + colindex].Selected = !item['col_' + colindex].Selected && $(e.target).val() == 'on';
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            $scope.irgrid.grid.onDblClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id.startsWith('img_')) {
                    item = args.grid.getDataItem(args.row);
                    var clmn = args.grid.getColumns()[args.cell];
                    var colindex = parseInt(clmn.id.split('_')[1]);
                    var link = document.createElement('a');
                    link.href = clmn.urlprefix + item['col_' + colindex].String[0];
                    link.target = '_blank';
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }
            });

            $scope.irgrid.datasource = result.items;
            $scope.irgrid.GetSelectedRows = function () {
                var selectedData = [];
                for (var index = 0; index < this.datasource.length; index++) {
                    if (this.datasource[index].Selected)
                        selectedData.push(this.datasource[index]);
                }
                return selectedData;
            }

            $scope.irgrid.UpdateData(groupresults);
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
                var rule = {
                    'option': $scope.PrimarySelected.id, 'type': $scope.PrimarySelected.type, 'typeName': $scope.PrimarySelected.name,
                    'primary': $scope.PrimarySelected, 'range': $scope.Range, 'nameList': $scope.SecondaryName, 'customoption': $scope.PrimarySelected.coption
                };
                if ($scope.PrimarySelected.type == CondtionType.Date)
                    rule.dateList = $scope.SecondarySelected;
                else if ($scope.PrimarySelected.type == CondtionType.Long)
                    rule.longList = $scope.SecondarySelected;
                else
                    rule.stringList = $scope.SecondarySelected;
                scondition.push(rule);
            }
            var dialog = ngDialog.open({
                template: 'SaveReport',
                data: {
                    'SecondaryCondtions': scondition,
                    'Parent': $scope,
                    'Report': (JSON.parse(JSON.stringify(report)))
                },
                width: '650px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SHOWVALUE = !$scope.ngDialogData.Parent.Permission.p_SHOWVALUE.view;
                    $scope.GroupColumns = [{ 'id': '', 'name': 'None' }, { 'id': 'Product', 'name': 'Product' }, { 'id': 'Brand', 'name': 'Brand' }, { 'id': 'Style', 'name': 'Style' },
                    { 'id': 'Pattern', 'name': 'Pattern' }, { 'id': 'Size', 'name': 'Size' },
                    { 'id': 'Colour', 'name': 'Colour' },
                    { 'id': 'Type', 'name': 'Type' },
                    { 'id': 'Fit', 'name': 'Fit' },
                    { 'id': 'Sleeve', 'name': 'Sleeve' },
                    { 'id': 'Design', 'name': 'Design' }];

                    $.each($scope.ngDialogData.SecondaryCondtions, function (k, v) {
                        if (v.typeName != 'PRODUCT' && v.typeName != 'BRAND' && v.typeName != 'SIZE'
                            && v.typeName != 'PATTERN' && v.typeName != 'MATERIAL' && v.typeName != 'STYLE'
                            && v.typeName != 'COLOUR' && v.typeName != 'TYPE' && v.typeName != 'DESIGN'
                            && v.typeName != 'FIT' && v.typeName != 'SLEEVE')
                            $scope.GroupColumns.push({ 'id': '$' + v.typeName, 'name': v.typeName });
                    });

                    $scope.SecondaryCondtions = $scope.ngDialogData.SecondaryCondtions;
                    $scope.Parent = $scope.ngDialogData.Parent;
                    $scope.Report = {
                        'Title': '', 'Description': '', 'SecondaryCondtions': $scope.SecondaryCondtions,
                        'Column': {}, 'Mode': 'Save', 'id': 0,
                        'ResultColumn': { 'qty': $scope.SHOWVALUE, 'value': $scope.SHOWVALUE }
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
                        $scope.Report.Column.Fit = $scope.Report.Column.Sleeve = all;
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
                                "Item": { 'type': 'SALESANALYSER', 'name': data.value.Title, 'code': 'SALESANALYSER', 'data': JSON.stringify(data.value), 'id': data.value.id },
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

                            if (data.value.Column.Fit)
                                $scope.ReportCondition.Columns.push('Fit');
                            if (data.value.Column.Sleeve)
                                $scope.ReportCondition.Columns.push('Sleeve');

                            if (data.value.Column.Type)
                                $scope.ReportCondition.Columns.push('Type');
                            if (data.value.Column.Design)
                                $scope.ReportCondition.Columns.push('Design');

                            $.each($scope.ReportCondition.Columns, function (item) {
                                $scope.ReportCondition.ColumnIndex.push($scope.ReportCondition.ColumnIndex.length);
                            });


                            $scope.ReportCondition.PrimaryCondtion = $scope.PrimarySelected.id;    // SEELCT SIZE
                            $scope.ReportCondition.PrimaryRange = $scope.Range;
                            $scope.ReportCondition.SecondaryCondtions = data.value.SecondaryCondtions;
                            $scope.ReportCondition.isreport = true;
                            
                            request = {
                                "item": $scope.ReportCondition,
                                "securitytoken": "String content"
                            }

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "AnalyserService/SalesAnalyser",
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
            var igrid = new InfyGrid('gridContainer', '500px', '100%', true, true, true, 25);

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
                    column.sortable = true;
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
                    column.sortable = true;
                    column.getValue = function (item, col) {
                        var cellresult = item.GroupValues[col.internal.Index];
                        if (cellresult != null) return cellresult.value;
                        return 0;
                    }
                }

                if ($scope.ReportCondition.ResultColumn.margin) {
                    if (typeof ($scope.Permission.p_SHOWSELLINGVALUE) != 'undefined' && !$scope.Permission.p_SHOWSELLINGVALUE.view) {
                        var column = igrid.AddCustomColumn('margin-' + v.Index, 'Margin', 100, function (item, col) {
                            var cellresult = item.GroupValues[col.internal.Index];
                            if (cellresult != null) return $filter('IndianNumber')(cellresult.iqty - cellresult.ivalue, 2);
                            return '';
                        });
                        column.sortable = true;
                        column.cssClass = 'rightalign';
                        column.internal = v;
                        column.total = true;
                        column.filtertype = 'number';
                        column.getValue = function (item, col) {
                            var cellresult = item.GroupValues[col.internal.Index];
                            if (cellresult != null)
                                return (cellresult.iqty - cellresult.ivalue);
                            return 0;
                        }

                        column = igrid.AddCustomColumn('marginup-' + v.Index, 'Mark UP %', 100, function (item, col) {
                            var cellresult = item.GroupValues[col.internal.Index];
                            if (cellresult != null)
                                return ((cellresult.iqty - cellresult.ivalue) / cellresult.ivalue * 100).toFixed(2) + '%';
                            return '';;
                        });
                        column.markup = { 'svalue': '({0}.iqty - {0}.ivalue)', 'value': 'ivalue' };
                        column.sortable = true;
                        column.cssClass = 'rightalign';
                        column.internal = v;
                        column.getValue = function (item, col) {
                            var cellresult = item.GroupValues[col.internal.Index];
                            if (cellresult != null)
                                return ((cellresult.iqty - cellresult.ivalue) / cellresult.ivalue * 100);
                            return 0;
                        }

                        column = igrid.AddCustomColumn('margindown-' + v.Index, 'Mark DN %', 100, function (item, col) {
                            var cellresult = item.GroupValues[col.internal.Index];
                            if (cellresult != null)
                                return (100 - ((cellresult.ivalue / cellresult.iqty) * 100)).toFixed(2) + '%';
                            return '';
                        });
                        column.markdown = { 'svalue': '({0}.iqty - {0}.ivalue)', 'value': 'ivalue' };
                        column.sortable = true;
                        column.cssClass = 'rightalign';
                        column.internal = v;
                        column.getValue = function (item, col) {
                            if (cellresult != null)
                                return (100 - ((item.ivalue / item.iqty) * 100));
                            return 0;
                        }
                    }
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
            //igrid.grid.setOptions({ 'frozenColumn': $scope.GroupResult.Columns.length - 1 });
            $scope.igrid.UpdateData($scope.GroupResult.Items);
        }


        $scope.ShowReportResult = function () {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            if (typeof ($scope.ReportResult.columns) == 'undefined')
                $scope.ReportResult.columns = [];
            if (typeof ($scope.ReportResult.additionalcolumns) == 'undefined')
                $scope.ReportResult.additionalcolumns = [];

            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.EnableTotal();

            for (var column = 0; column < $scope.ReportResult.columns.length; column++) {
                var col = igrid.AddCustomColumn('col-' + column, $scope.ReportResult.columns[column], 150, function (item, col) {
                    return item.String[col.StringIndex];
                });
                col.sortable = true;
                col.enablefilter = true;
                col.StringIndex = column;
                col.getValue = function (item, col) {
                    if (item.String[col.StringIndex] == null) return '';
                    return item.String[col.StringIndex];
                }
            }

            for (var column = 0; column < $scope.ReportResult.additionalcolumns.length; column++) {
                var col = igrid.AddCustomColumn('addl-' + column, $scope.ReportResult.additionalcolumns[column].headername, 150, function (item, col) {
                    return item.String[col.StringIndex];
                });
                col.StringIndex = $scope.ReportResult.additionalcolumns[column].index;
                col.sortable = true;
                col.enablefilter = true;
                col.getValue = function (item, col) {
                    if (item.String[col.StringIndex] == null) return '';
                    return item.String[col.StringIndex];
                }
            }

            if ($scope.ReportCondition.ResultColumn.qty) {
                igrid.AddFormatedColumn('qty', 'Qty', 100, 'N2', 2);
            }

            if ($scope.ReportCondition.ResultColumn.value) {
                igrid.AddFormatedColumn('value', 'Value', 100, 'N2', 2);
                igrid.AddFormatedColumn('svalue', 'Discount', 100, 'N2', 2).total = true;
                igrid.AddCustomColumn('percentage', '%', 100, function (item, col) {
                    if (typeof (col.totalValue) != 'undefined') {
                        return (item.value / col.totalValue * 100).toFixed(2) + '%';
                    }
                }).cssClass = 'rightalign';

                if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                    igrid.AddFormatedColumn('iqty', 'Taxable', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('svalue', 'Discount', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('ivalue', 'Stock Taxable', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('cvalue', 'Stock Cost', 100, 'N2', 2).total = true;

                }

                if (typeof ($scope.Permission.p_SHOWSELLINGVALUE) != 'undefined' && !$scope.Permission.p_SHOWSELLINGVALUE.view) {
                    var column = igrid.AddCustomColumn('marginup', 'Margin', 100, function (item, col) {
                        return $filter('IndianNumber')(item.iqty - item.ivalue, 2);
                    });
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.enablefilter = true;
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        return (item.iqty - item.ivalue);
                    }

                    column = igrid.AddCustomColumn('marginup', 'Mark UP %', 100, function (item, col) {
                        return ((item.iqty - item.ivalue) / item.ivalue * 100).toFixed(2) + '%';
                    });
                    column.sortable = true;
                    column.markup = { 'svalue': '({0}.iqty - {0}.ivalue)', 'value': 'ivalue' };
                    column.cssClass = 'rightalign';
                    column.enablefilter = true;
                    column.getValue = function (item, col) {
                        return ((item.iqty - item.ivalue) / item.ivalue * 100);
                    }

                    column = igrid.AddCustomColumn('margindown', 'Mark DN %', 100, function (item, col) {
                        return (100 - ((item.ivalue / item.iqty) * 100)).toFixed(2) + '%';
                    });
                    column.sortable = true;
                    column.markdown = { 'svalue': '({0}.iqty - {0}.ivalue)', 'value': 'ivalue' };
                    column.cssClass = 'rightalign';
                    column.enablefilter = true;
                    column.getValue = function (item, col) {
                        return (100 - ((item.ivalue / item.iqty) * 100));
                    }
                }
            }

            igrid.Initialize();
            $scope.igrid = igrid;
            for (var i = 0; i < $scope.ReportResult.items.length; i++)
                $scope.ReportResult.items[i].id = i;
            var percentageValueColumn = $scope.igrid.grid.getColumns()[$scope.igrid.grid.getColumnIndex('percentage')];
            if (typeof (percentageValueColumn) != 'undefined') {
                percentageValueColumn.totalValue = 0;
                $.each($scope.SearchResult, function (k, v) {
                    percentageValueColumn.totalValue += v.value;
                });
            }
            $scope.igrid.UpdateData($scope.ReportResult.items);
        }

        $scope.irgrid = null;
        $scope.ConfigureGrid = function () {
            if ($scope.irgrid != null) {
                $scope.irgrid.grid.destroy();
                $('#gridResultContainer').empty();
            }
            $scope.irgrid = new InfyGrid('gridResultContainer', '450px', '100%', false, true, true, 0);
            $scope.irgrid.options.showHeaderRow = true;
            $scope.irgrid.EnableTotal();
            $scope.irgrid.AddSimpleColumn('name', 'Description', 250);
            $scope.irgrid.AddFormatedColumn('qty', 'Qty', 100, 'N2', 2).total = true;

            if (typeof ($scope.Permission.p_SHOWSELLINGVALUE) != 'undefined' && !$scope.Permission.p_SHOWSELLINGVALUE.view) {
                $scope.irgrid.AddFormatedColumn('iqty', 'Taxable', 100, 'N2', 2).total = true;
                $scope.irgrid.AddFormatedColumn('value', 'Sale Value', 100, 'N2', 2).total = true;
                $scope.irgrid.AddCustomColumn('percentage', '%', 60, function (item, col) {
                    if (typeof (col.totalValue) != 'undefined') {
                        return (item.value / col.totalValue * 100).toFixed(2) + '%';
                    }
                }).cssClass = 'rightalign';
                $scope.irgrid.AddFormatedColumn('svalue', 'Discount', 100, 'N2', 2).total = true;
            }

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view) {
                $scope.irgrid.AddFormatedColumn('ivalue', 'Stock Taxable', 100, 'N2', 2).total = true;
                $scope.irgrid.AddFormatedColumn('cvalue', 'Stock Cost', 100, 'N2', 2).total = true;                
            }
            if (typeof ($scope.Permission.p_SHOWSELLINGVALUE) != 'undefined' && !$scope.Permission.p_SHOWSELLINGVALUE.view) {
                var column = $scope.irgrid.AddCustomColumn('margin', 'Margin', 100, function (item, col) {
                    return $filter('IndianNumber')(item.iqty - item.ivalue, 2);
                });
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    return (item.iqty - item.ivalue);
                }

                column = $scope.irgrid.AddCustomColumn('marginup', 'Mark UP %', 100, function (item, col) {
                    return ((item.iqty - item.ivalue) / item.ivalue * 100).toFixed(2) + '%';
                });
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.markup = { 'svalue': '({0}.iqty - {0}.ivalue)', 'value': 'ivalue' };
                column.getValue = function (item, col) {
                    return ((item.iqty - item.ivalue) / item.ivalue * 100);
                }

                column = $scope.irgrid.AddCustomColumn('margindown', 'Mark DN %', 100, function (item, col) {
                    return (100 - ((item.ivalue / item.iqty) * 100)).toFixed(2) + '%';
                });
                column.sortable = true;
                column.markdown = { 'svalue': '({0}.iqty - {0}.ivalue)', 'value': 'ivalue' };
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return (100 - ((item.ivalue / item.iqty) * 100));
                }
            }

            $scope.irgrid.Initialize();
            $scope.irgrid.grid.setOptions({ 'frozenColumn': 1 });
        }
        $scope.ConfigureGrid();
        $scope.UpdatePercentage = function () {
            var percentageValueColumn = $scope.irgrid.grid.getColumns()[$scope.irgrid.grid.getColumnIndex('percentage')];
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
            //$scope.exportHref = infygrid.Export(infygrid.grid, filename);
            //return;
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


        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                $rootScope.UserInfo.appsettings != null) {

                if ($rootScope.UserInfo.appsettings.prd_hierarchy &&
                    $rootScope.UserInfo.appsettings.prd_hierarchy != null &&
                    $rootScope.UserInfo.appsettings.prd_hierarchy != '') {
                    var list = $rootScope.UserInfo.appsettings.prd_hierarchy.split(',');
                    for (var i = 0; i < list.length; i++) {
                        $scope.Options.push({
                            'id': Options.Custom_ProductGroups,
                            'name': 'PRD GRP - ' + list[i],
                            'coption': i + 1,
                            'style': $scope.style_unselected,
                            'type': CondtionType.Long,
                            'role': Roles.All
                        })
                    }
                }

                if ($rootScope.UserInfo.appsettings.system_customfields) {

                    restService.Get('ReferencelistService/FULL-CUSTOMFIELDS', true).then(function (results) {
                        $.each(results, function (k, v) {
                            if (v.additionalinfo.customfield &&
                                v.additionalinfo.customfield != null &&
                                v.additionalinfo.customfield.cfbuilt) {
                                var coption = v.additionalinfo.customfield.cfname + ',' +
                                    v.referenceid + ',' +
                                    v.additionalinfo.customfield.cftype + ',' +
                                    v.additionalinfo.customfield.cfref;
                                var type = CondtionType.Long;
                                if (v.referenceid == 1)
                                    type = CondtionType.String;
                                else if (v.referenceid == 2)
                                    type = CondtionType.Decmal;
                                else if (v.referenceid == 3)
                                    type = CondtionType.Date;

                                $scope.Options.push({
                                    'id': Options.Custom_Fields,
                                    'name': 'CFIELDS - ' + v.name,
                                    'coption': coption,
                                    'style': $scope.style_unselected,
                                    'type': type,
                                    'role': Roles.All
                                })
                            }
                        })
                    });

                }

                if ($rootScope.UserInfo.appsettings.analyser_incharge) {
                    $scope.Options.splice(0, 0, { 'id': Options.Incharge, 'name': 'INCHARGE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All });
                    $scope.Options.sort(function (a, b) {
                        var x = a.name.toLowerCase();
                        var y = b.name.toLowerCase();
                        return x < y ? -1 : x > y ? 1 : 0;
                    });
                }

                $('.gridHScroll').scroll(function () {
                    $('.gridVScroll').width($('.gridHScroll').width() + $('.gridHScroll').scrollLeft());
                });

            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }
        $scope.LoadApplicationSetting();


    }]);