
var app = angular.module('InfinityLab');

app.controller('ImageGalleryController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    'restService', 'ngDialog', '$state','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        

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
            TaxPercentage: 102,
            GLNNO: 103,
            Floor: 104,
            StockMarker: 106,
            BarcodeGroup: 117
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
        $scope.Options = [
            { 'id': Options.Product, 'name': 'PRODUCT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Brand, 'name': 'BRAND', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Size, 'name': 'SIZE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Pattern, 'name': 'PATTERN', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Material, 'name': 'MATERIAL', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Style, 'name': 'STYLE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Colour, 'name': 'COLOUR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Type, 'name': 'TYPE', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Section, 'name': 'SECTION', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.ProductGroup, 'name': 'PRODUCT GROUP', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.BarcodeGroup, 'name': 'BARCODE GROUP', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.ProductCode, 'name': 'PRODUCT CODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.Design, 'name': 'DESIGN', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.SaleRate, 'name': 'SALE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.PurchaseRate, 'name': 'PURCHASE RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.DiscountRate, 'name': 'DISCOUNT RATE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.All },
            { 'id': Options.SoldRate, 'name': 'SALE PRICE', 'style': $scope.style_unselected, 'type': CondtionType.Decmal, 'role': Roles.Sales },
            { 'id': Options.LREntryNo, 'name': 'LRENTRY NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.InvoiceNo, 'name': 'INVOICE NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.InvDate, 'name': 'INVOICE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All },
            { 'id': Options.PurchaseDate, 'name': 'PURCHASE DATE', 'style': $scope.style_unselected, 'type': CondtionType.Date, 'role': Roles.All },
            { 'id': Options.SaleYear, 'name': 'SALE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            { 'id': Options.PurchaseYear, 'name': 'PURCHASE YEAR', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.PurchaseMonth, 'name': 'PURCHASE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.SaleMonth, 'name': 'SALE MONTH', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.Sales },
            { 'id': Options.Supplier, 'name': 'SUPPLIER', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Agent, 'name': 'AGENT', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.Supplier_City, 'name': 'SUPPLIER CITY', 'style': $scope.style_unselected, 'type': CondtionType.Long, 'role': Roles.All },
            { 'id': Options.GLNNO, 'name': 'GLN NO', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
            { 'id': Options.HSNCode, 'name': 'HSN CODE', 'style': $scope.style_unselected, 'type': CondtionType.String, 'role': Roles.All },
        ];

        $scope.Options.sort(function (a, b) {
            var x = a.name.toLowerCase();
            var y = b.name.toLowerCase();
            return x < y ? -1 : x > y ? 1 : 0;
        });

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

        $scope.BarcodeList = []
        $scope.ConfigGrid = function () {

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridBarcodes').empty();
            }

            var igrid = new InfyGrid('gridBarcodes', '100px', '100%', false, true, true,360);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('barcode', 'Barcode', 120);
            igrid.Initialize();
            $scope.igrid = igrid;

            if ($scope.irgrid != null) {
                $scope.irgrid.grid.destroy();
                $('#gridResultContainer').empty();
            }

            var irgrid = new InfyGrid('gridResultContainer', '100px', '100%', false, true, true, 0);
            irgrid.options.showHeaderRow = true;
            irgrid.AddSimpleColumn('Image', 'Image', 120);
            irgrid.Initialize();
            $scope.irgrid = irgrid;
        }
       
        $scope.AddBarcode = function (barcode) {
            $scope.BarcodeList.push({ 'barcode': barcode });
            $scope.igrid.UpdateData($scope.BarcodeList);
            $scope.directbarcode = '';
        }
        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfilename = null;
            $.each(value.rows, function (k, v) {
                if (v.values.length > 0) {
                    $scope.BarcodeList.push({ 'barcode': v.values[0] });
                }
            });
            $scope.igrid.UpdateData($scope.BarcodeList);
        }

        $scope.Search = function () {
            var searchCondition = {
                "item": {
                    "reporttype": "1",
                    "PrimaryCondtion": 33,
                    "PrimaryRange": null,
                    "SecondaryCondtions": [
                        {
                            "option": 33, "type": 1,
                            "typeName": "BARCODE", "stringList": [],
                            'ShowColumn':true
                        }],
                    "ShowReport": false, "ShowAnalyis": true,
                    "ShowGroup": false, 
                    "Columns": ["ImageID"]
                }, "securitytoken": "String content"
            };
            $.each($scope.BarcodeList, function (k, v) {
                searchCondition.item.SecondaryCondtions[0].stringList.push(v.barcode);
            });
            $.each($scope.Options, function (k, v) {
                if (v.selected) {
                    searchCondition.item.SecondaryCondtions.push({
                        'option': v.id, 'type': v.type, 'typeName': v.name,'ShowColumn':true,'primary':v
                    });
                }
            });
            var srequest = JSON.stringify(searchCondition);
            $http({
                method: 'POST',
                url: "AnalyserService/StockAnalyser",
                data: srequest
            }).success(function (result) {
                var Report = {
                    'Title': '', 'Description': '', 'SecondaryCondtions': $scope.SecondaryCondtions,
                    'Column': {}, 'Mode': 'Save', 'id': 0,
                    'ResultColumn': {
                        'qty': UserDetails.Info.p_SHOWVALUE.view,
                        'value': UserDetails.Info.p_SHOWVALUE.view
                    }
                };
                Report.Mode = 'View';
                Report.SecondaryCondtions = searchCondition.item.SecondaryCondtions;
                Report.DetailPosition = $scope.DetailPosition;
                Report.RowCount = $scope.RowCount;
                Report.ColumnCount = $scope.ColumnCount;
                Report.datasource = result;
                $scope.Report = Report;
                $timeout(function () {
                    $scope.ShowImageResult(result, Report);
                }, 100);
            });
        }

        $scope.Report = null;

        $scope.DownloadPDF = function () {

            request = {
                "item": $scope.Report,
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
                'width': $('#gridResultContainer').width(), 'height': $('#gridResultContainer').height()
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

        $timeout(function () {
            $scope.ConfigGrid();
        }, 10);
    }]);
