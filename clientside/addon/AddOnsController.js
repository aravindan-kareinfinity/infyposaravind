
var app = angular.module('InfinityLab');

app.controller('AddOnsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, $stateParams) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.Appsetting_Company = false;
        $scope.CompanyList = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        });
        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.SalesCounter = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.SalesCounter = results;
        });
        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        
        $scope.SectionChanged = function () {
            if ($scope.margintargetgrid && $scope.margintargetgrid != null)
                $scope.margintargetgrid.destroy();
            var productlist = [];
            $.each($scope.ProductList, function (k, v) {
                if (v.salescounterid == $scope.SearchCondition.sectionid) {
                    productlist.push(v);
                    v.sno = productlist.length;
                    v.margin = '';
                    v.mstock = '';
                }
            })
            
            var igrid = new InfyGrid('gridProducts', '425px', '100%', true, true, true, 120);
            igrid.options.showHeaderRow = true;
            var groupColumns = [];
            igrid.AddSimpleColumn('sno', 'S.No', 80);
            igrid.AddSimpleColumn('name', 'Produt', 80);
            var column = igrid.AddColumn({
                id: "margin",
                name: "Margin",
                field: "margin",
                width: 80,
                cssClass: "cell-title",
                editor: Slick.Editors.Text,
                'sortable': true
            });
            var column = igrid.AddColumn({
                id: "mstock",
                name: "Stock",
                field: "mstock",
                width: 80,
                cssClass: "cell-title",
                editor: Slick.Editors.Text,
                'sortable': true
            });
            igrid.options.autoEdit = true;
            igrid.options.showHeaderRow = true;
            igrid.EnableTotal();
            igrid.Initialize();
            restService.Get('ReferencelistService/ADDONDATA/TARGETWITHMARGIN/' + $scope.SearchCondition.sectionid, true).then(function (results) {
                if (results && results != null && results.length > 0) {
                    $scope.margintargetconfig = results[0];
                    $.each(results[0].customArraydata, function (k, v) {
                        var exist = productlist.find(function (item) {
                            return item.id == v.productid;
                        });
                        if (exist && exist != null) {
                            exist.margin = v.margin;
                            exist.mstock = v.mstock;
                        }
                    })
                }
                else {
                    $scope.margintargetconfig = null;   
                }
                $scope.margintargetgrid = igrid;
                $scope.margintargetdata = productlist;
                igrid.UpdateData(productlist, '_id');
            });
        }

        $scope.margintargetconfig = null;
        $scope.SaveTargetMarginConfig = function () {
            var data = [];
            
            $.each($scope.margintargetdata, function (k, v) {
                data.push({ 'productid': v.id, 'margin': v.margin, 'mstock': v.mstock });
            })

            if ($scope.margintargetconfig == null)
                $scope.margintargetconfig = { 'customtype': 'TARGETWITHMARGIN', 'referenceid': $scope.SearchCondition.sectionid, 'isactive': true };

            $scope.margintargetconfig.customArraydata = data;
            request = {
                "Item": $scope.margintargetconfig ,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AddondataService/Save",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Configuration Saved Successfully.!");
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            });
        }

        $scope.ReportTargetMargin = function () {
            var data = [];
            $.each($scope.margintargetdata, function (k, v) {
                data.push({ 'productid': v.id, 'margin': v.margin, 'mstock': v.mstock });
            })

            if ($scope.margintargetconfig == null)
                $scope.margintargetconfig = { 'customtype': 'TARGETWITHMARGIN', 'referenceid': $scope.SearchCondition.sectionid, 'isactive': true };

            var products = [];
            var rows = $scope.margintargetgrid.GetSelectedRows();
            if (rows.length > 0) {
                $.each(rows, function (k, v) {
                    products.push(v.id);
                })
            }

            $scope.margintargetconfig.productids = products;
            $scope.margintargetconfig.customArraydata = data;
            $scope.margintargetconfig.companyid = $scope.SearchCondition.companyid;
            $scope.margintargetconfig.locationid = $scope.SearchCondition.locationid;
            $scope.margintargetconfig.sectionid = $scope.SearchCondition.sectionid;
            $scope.margintargetconfig.targetfrom = $scope.SearchCondition.targetfrom;
            $scope.margintargetconfig.targettodate = $scope.SearchCondition.targettodate;
            $scope.margintargetconfig.salesfromdate = $scope.SearchCondition.salesfromdate;
            $scope.margintargetconfig.salestodate = $scope.SearchCondition.salestodate;

            request = {
                "Item": $scope.margintargetconfig,
                "securitytoken": "String content"
            }

            $http({
                method: 'POST',
                url: "AddondataService/ReportTargetMargin",
                data: request
            }).success(function (results) {

                var stockmarkerlist = [];
                $.each(results, function (k, v) {
                    v.sno = k + 1;

                    var config = $scope.margintargetconfig.customArraydata.find(function (item) {
                        return item.productid == v.productid;
                    });

                    if (config == undefined || config == null)
                        config = { 'margin': 0, 'mstock': 0 };

                    v.configmargin = parseFloat(config.margin);
                    v.configstock = parseFloat(config.mstock);

                    if (v.markers && v.markers != null) {
                        $.each(v.markers, function (kk, vv) {
                            var exist = stockmarkerlist.find(function (item) {
                                return item.id == vv.id;
                            })
                            if (exist == undefined || exist == null) {
                                stockmarkerlist.push({ 'id': vv.id, 'name': vv.name });
                            }
                        })
                    }
                })

                if ($scope.sgrid && $scope.sgrid != null)
                    $scope.sgrid.destroy();

                var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 50);
                igrid.options.showHeaderRow = true;
                var groupColumns = [];

                igrid.AddSimpleColumn('sno', 'S.No', 80);
                igrid.AddSimpleColumn('productname', 'Product', 80);
                igrid.AddFormatedColumn('configstock', 'F.Stock', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('stockqty', 'C.Stock', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('targetqty', 'Target', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('salesqty', 'Sales', 80, 'N2', 0).total = true;

                $.each(stockmarkerlist, function (k, v) {
                    var column = igrid.AddCustomColumn('smst_' + k, 'Stock - '+v.name, 40,
                        function (item, col) {
                            if (item.markers && item.markers != null) {
                                var markervalue = item.markers.find(function (item) {
                                    return item.id == col.MarkerID;
                                })
                                if (markervalue && markervalue != null) {
                                    return markervalue.stockqty;
                                }
                            }
                            return '';
                        });
                    column.MarkerID = v.id;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.markers && item.markers != null) {
                            var markervalue = item.markers.find(function (item) {
                                return item.id == col.MarkerID;
                            })
                            if (markervalue && markervalue != null) {
                                return markervalue.stockqty;
                            }
                        }
                        return 0;
                    }
                    var column = igrid.AddCustomColumn('smsl_' + k, 'Sales - ' + v.name, 40,
                        function (item, col) {
                            if (item.markers && item.markers != null) {
                                var markervalue = item.markers.find(function (item) {
                                    return item.id == col.MarkerID;
                                })
                                if (markervalue && markervalue != null) {
                                    return markervalue.salesqty;
                                }
                            }
                            return '';
                        });
                    column.MarkerID = v.id;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.markers && item.markers != null) {
                            var markervalue = item.markers.find(function (item) {
                                return item.id == col.MarkerID;
                            })
                            if (markervalue && markervalue != null) {
                                return markervalue.salesqty;
                            }
                        }
                        return 0;
                    }
                });
                igrid.AddFormatedColumn('targetivalue', 'Target Value', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('saleivalue', 'Sales Value', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('salesicost', 'Sold Cost', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('configmargin', 'Benchmark', 80, 'N2', 0).total = true;
                var column = igrid.AddCustomColumn('markdown', 'Markdown', 100, function (item) {
                    return (100 - ((item.salesicost / item.saleivalue) * 100)).toFixed(2);
                });
                var column = igrid.AddCustomColumn('final', '%', 100, function (item) {
                    return ((item.saleivalue / item.targetivalue * 100)-100).toFixed(2);
                });
                igrid.EnableTotal();
                igrid.Initialize();
                $scope.sgrid = igrid;
                $scope.sgrid.UpdateData(results, '_id');
                $scope.TargetQty = 0;
                $scope.SaleQty = 0;
                $scope.TargetValue = 0;
                $scope.SaleValue = 0;
                $.each(results, function (k, v) {
                    $scope.TargetQty += v.targetqty;
                    $scope.SaleQty += v.salesqty
                    $scope.TargetValue += v.targetivalue;
                    $scope.SaleValue += v.saleivalue;
                })
            });
        }

        $scope.Export = function () {
            $scope.sgrid.ExportExcel($scope.sgrid.grid, '', '', true, [], function (rows) {
                var colcount = $scope.sgrid.grid.getColumns().length;
                var singlerow = ["<td colspan='2'></td>", '<td>Target Qty</td>', '<td>Sales Qty</td>', '<td>Qty %</td>', '<td>Target Value</td>', '<td>Sales Value</td>', '<td>Value %</td>'];

                rows.push('<tr>');
                rows.push("<th colspan='" + colcount+"'></th>");
                rows.push('</tr>');


                rows.push('<tr>');
                rows.push(singlerow.join(''));
                rows.push('</tr>');

                singlerow = ["<th colspan='2'></th>"];
                singlerow.push('<th mso-number-format:"0.' + '0'.repeat(2) + '">');
                singlerow.push($scope.sgrid.GetNumber($scope.TargetQty).round(2));
                singlerow.push('</th>');
                singlerow.push('<th mso-number-format:"0.' + '0'.repeat(2) + '">');
                singlerow.push($scope.sgrid.GetNumber($scope.SaleQty).round(2));
                singlerow.push('</th>');
                singlerow.push('<th mso-number-format:"0.' + '0'.repeat(2) + '">');
                singlerow.push($scope.sgrid.GetNumber($scope.SaleQty / $scope.TargetQty * 100 - 100).round(2));
                singlerow.push('</th>');
                singlerow.push('<th mso-number-format:"0.' + '0'.repeat(2) + '">');
                singlerow.push($scope.sgrid.GetNumber($scope.TargetValue).round(2));
                singlerow.push('</th>');
                singlerow.push('<th mso-number-format:"0.' + '0'.repeat(2) + '">');
                singlerow.push($scope.sgrid.GetNumber($scope.SaleValue).round(2));
                singlerow.push('</th>');
                singlerow.push('<th mso-number-format:"0.' + '0'.repeat(2) + '">');
                singlerow.push($scope.sgrid.GetNumber($scope.SaleValue / $scope.TargetValue * 100 - 100).round(2));
                singlerow.push('</th>');

                rows.push('<tr>');
                rows.push(singlerow.join(''));
                rows.push('</tr>');

            });
        //    var meregelist = [];
        //    data = [];
        //    data.push([
        //        'LEDGER',
        //        'ACCOUNT NAME',
        //        'ACCOUNT NO',
        //        'ACCOUNT IFSC',
        //        'BRANCH',
        //        'AMOUNT'
        //    ]);

        //    var total = 0;
        //    $.each(seletedRows, function (k, v) {
        //        data.push([
        //            v.ledgername,
        //            v.accountname,
        //            v.bankaccountno,
        //            v.bankifsc,
        //            v.bankbranch,
        //            v.amount
        //        ]);
        //        total += v.amount;
        //    });

        //    data.push([
        //        '',
        //        '',
        //        '',
        //        '',
        //        '',
        //        total
        //    ]);

        //    if (data.length > 0) {
        //        var ws = XLSX.utils.aoa_to_sheet(data);
        //        var wb = XLSX.utils.book_new();

        //        ws['!merges'] = meregelist;

        //        ws['!cols'] = [{ width: 50 }, { width: 40 }, { width: 30 }, { width: 50 }, { width: 50 }, { width: 30 }];
        //        var title = {									// set the style for target cell
        //            font: {
        //                name: 'verdana',
        //                sz: 14,
        //                bold: true,
        //                color: { rgb: "33220000" }
        //            },
        //            alignment: {
        //                vertical: "center",
        //                horizontal: "center"
        //            }
        //        };

        //        var Stitle = {									// set the style for target cell
        //            font: {
        //                name: 'verdana',
        //                sz: 12,
        //                bold: true,
        //                color: { rgb: "33220000" }
        //            },
        //            alignment: {
        //                vertical: "center",
        //                horizontal: "left"
        //            }
        //        };

        //        var Ctitle = {									// set the style for target cell
        //            font: {
        //                name: 'verdana',
        //                sz: 12,
        //                bold: true,
        //                color: { rgb: "33220000" }
        //            },
        //            alignment: {
        //                vertical: "center",
        //                horizontal: "center"
        //            }
        //        };

        //        ws["A1"].s = title;
        //        ws["A2"].s = title;

        //        ws["A3"].s = Stitle;
        //        ws["B3"].s = Stitle;
        //        ws["C3"].s = Stitle;
        //        ws["D3"].s = Stitle;
        //        ws["E3"].s = Stitle;
        //        ws["F3"].s = Stitle;

        //        ws["A4"].s = Ctitle;
        //        ws["B4"].s = Ctitle;
        //        ws["C4"].s = Ctitle;
        //        ws["D4"].s = Ctitle;
        //        ws["E4"].s = Ctitle;
        //        ws["F4"].s = Ctitle;

        //        //ws["A1:F2"].s = {									// set the style for target cell
        //        //    border: {
        //        //        bottom: {
        //        //            style: 'thin', color: { auto: 1 }
        //        //        }
        //        //    },
        //        //};

        //        XLSX.utils.book_append_sheet(wb, ws, "sheet1");
        //        /* generate file and download */
        //        const wbout = XLSX.write(wb, { type: "array", bookType: "xlsx" });
        //        saveAs(new Blob([wbout], { type: "application/octet-stream" }), "download.xlsx");
        }

        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined') {
                $scope.Appsetting_Company = true;
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }
        $scope.LoadApplicationSetting();

    }]);