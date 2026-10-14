
var app = angular.module('InfinityLab');

app.controller('FinanalyserController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$filter', '$timeout', 'cntxService', '$state',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $filter, $timeout, cntxService, $state) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.filteredtype = '';
        $scope.ReportModes = [{ 'id': 1, 'name': 'Company Summary' },
        { 'id': 5, 'name': 'Location Summary' },
        { 'id': 6, 'name': 'Voucher Summary' },
        { 'id': 66, 'name': 'Voucher Detail' },
        { 'id': 7, 'name': 'Tax Summary' },
        { 'id': 77, 'name': 'Tax Detail' },
        { 'id': 2, 'name': 'Compare' },
        { 'id': 3, 'name': 'Monthly' },
        { 'id': 4, 'name': 'Detail' }];

        $scope.Location = null;
        $scope.Company = null;
        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
            if ($scope.SearchRequest && $scope.SearchRequest.locations != null) {
                $.each($scope.SearchRequest.locations, function (k, v) {
                    var exist = $scope.Location.find(function (item) {
                        return item.id == v;
                    });
                    if (exist && exist != null)
                        exist.view = true;
                });
            }
        });

        $scope.TaxModes = [{ 'id': 0, 'name': 'Zero Tax' },
        { 'id': 1, 'name': 'DUTY' },
        { 'id': 2, 'name': 'CESS' },
        { 'id': 3, 'name': 'IGST' },
        { 'id': 4, 'name': 'SGST' },
        { 'id': 5, 'name': 'CGST' },
        { 'id': 6, 'name': 'TDS' },
        { 'id': 7, 'name': 'TCS' },
        { 'id': 8, 'name': 'SURCHARGE' }];
        $scope.PurchaseType = null;

        restService.Get('ReferencelistService/FULL-PURCHASETYPE', true).then(function (results) {
            $scope.PurchaseType = results;
            var vt = $scope.vouchertypes;
            $timeout(function () {
                $scope.vouchertypes = null;
                $.each(results, function (k, v) {
                    vt.push({ 'id': 100 + v.id, 'ptid': v.id, 'name': 'Purchase - ' + v.name });
                });
                $scope.vouchertypes = vt;
            }, 30);
        });

        $scope.vouchertypes = [
            { 'id': 1, 'name': 'Payment' },
            { 'id': 2, 'name': 'Receipt' },

            { 'id': 3, 'name': 'Purchase' },
            { 'id': 4, 'name': 'Sales' },
            { 'id': 5, 'name': 'Credit Note' },
            { 'id': 6, 'name': 'Debit Note' },

            { 'id': 7, 'name': 'Contra' },
            { 'id': 8, 'name': 'Journal Cr' },

            { 'id': 9, 'name': 'Cash Deposit' },
            { 'id': 10, 'name': 'Cash Widhrawal' },
            { 'id': 11, 'name': 'Main Cash to Petty Cash' },
            { 'id': 12, 'name': 'Petty Cash to Main Cash' },
            { 'id': 13, 'name': 'Cash Counter Expense' },

            { 'id': 14, 'name': 'Advance Payment' },

            { 'id': 15, 'name': 'Petty Cash Expense' },
            { 'id': 16, 'name': 'Journal Dr' },
            { 'id': 99, 'name': 'EOD Posting' }
        ];


        $scope.ItemTypes = [{ 'id': 1, 'name': 'Capital' },
        { 'id': 2, 'name': 'Fixed asset' },
        { 'id': 19, 'name': 'Current asset' },
        { 'id': 3, 'name': 'Liablities' },
        { 'id': 20, 'name': 'Current Liablities' },
        { 'id': 4, 'name': 'Direct Income' },
        { 'id': 5, 'name': 'Direct Expense' },
        { 'id': 6, 'name': 'Cash-In-Hand' },
        { 'id': 7, 'name': 'Bank Account' },
        { 'id': 8, 'name': 'Stock-In-Hand' },
        { 'id': 9, 'name': 'Indirect Incomes' },
        { 'id': 10, 'name': 'Indirect Expenses' },
        { 'id': 11, 'name': 'Sundry Debtors' },
        { 'id': 12, 'name': 'Sundry Creditors' },
        { 'id': 13, 'name': 'Petty Cash' },
        { 'id': 14, 'name': 'Sales' },
        { 'id': 15, 'name': 'Purchase' },
        { 'id': 16, 'name': 'Tax & Duties' },
        { 'id': 17, 'name': 'Petty Cash Direct Expense' },
        { 'id': 18, 'name': 'Petty Cash In-Direct Expense' },
        { 'id': 30, 'name': 'Rounding' }];

        $scope.Search = function () {

            $scope.SearchRequest.types = [];
            $.each($scope.ItemTypes, function (k, v) {
                if (v.view)
                    $scope.SearchRequest.types.push(v.id);
            })

            $scope.SearchRequest.locations = [];
            $.each($scope.Location, function (k, v) {
                if (v.view)
                    $scope.SearchRequest.locations.push(v.id);
            })

            $scope.SearchRequest.vtypes = [];
            $scope.SearchRequest.ptypes = [];
            $.each($scope.vouchertypes, function (k, v) {
                if (v.view) {
                    if (v.id > 100) {
                        $scope.SearchRequest.ptypes.push(v.id - 100);
                    }
                    else {
                        $scope.SearchRequest.vtypes.push(v.id);
                    }
                }
            })

            if ($scope.SearchRequest.ptypes.length > 0) {
                $scope.SearchRequest.vtypes.push(3);
            }

            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "VoucherService/Analyser",
                data: srequest
            }).success(function (result) {
                $scope.SupportGraph = false;
                $scope.LastResult = result;
                $scope.ShowResult(result);
            }).error(function (result) {
            });
        }

        $scope.ShowResult = function (result) {

            if ($scope.ShowGraph) {
                $scope.ShowGraph = false;
                $timeout(function () {
                    $scope.ShowResult(result);
                }, 25);
                return;
            }

            $scope.ShowTaxMode = false;
            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.destroy();
            if ($scope.SearchRequest.modeid == 1)
                $scope.ConfigCompanySummaryGrid(result);
            else if ($scope.SearchRequest.modeid == 2)
                $scope.ConfigCompareGrid(result);
            else if ($scope.SearchRequest.modeid == 3)
                $scope.ConfigMonthSummaryGrid(result);
            else if ($scope.SearchRequest.modeid == 4)
                $scope.ConfigDetailGrid(result);
            else if ($scope.SearchRequest.modeid == 6 || $scope.SearchRequest.modeid == 66)
                $scope.ConfigVoucherGrid(result);
            else if ($scope.SearchRequest.modeid == 5)
                $scope.ConfigLocationSummaryGrid(result);
            else if ($scope.SearchRequest.modeid == 7)
                $scope.ConfigTaxSummaryGrid(result,'');
            else if ($scope.SearchRequest.modeid == 77)
                $scope.ConfigTaxDetailGrid(result);
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'Finance Analyser'
            infygrid.ExportXLSX(infygrid.grid, title, subtitle);

            //var title = $rootScope.UserInfo.CompanyName;
            //var subtitle = 'Finance Analyser'
            //$scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            //if ($scope.exportHref != null) {
            //    $timeout(function () {
            //        var link = document.createElement('a');
            //        link.download = filename + ".xls";
            //        link.href = $scope.exportHref;
            //        document.body.appendChild(link);
            //        link.click();
            //        link.remove();
            //    }, 100);
            //}
        }

        $scope.ConfigVoucherGrid = function (result) {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddFormatedColumn('date', 'Voucher Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('no', 'Voucher No', 80);

            var column = igrid.AddCustomColumn('type', 'Voucher Type', 150, function (item, col) {
                if ($scope.PurchaseType &&
                    $scope.PurchaseType.length > 0 &&
                    item.type == 3 && item.ptype && item.ptype > 0)
                    return $scope.SearchRef(item.ptype, $scope.PurchaseType, 'Purchase Voucher');

                return $scope.SearchRef(item.type, $scope.vouchertypes, 'Petty Cash');
            });

            igrid.AddSimpleColumn('name', 'Ledger Name', 200);
            var exist = result.vsummary.find(function (item) {
                return item.aname && item.aname != null;
            })
            igrid.AddSimpleColumn('aname', 'Ledger Name-2', 200);

            if ($scope.SearchRequest.modeid == 66) {
                igrid.AddSimpleColumn('gstno', 'GST No', 100);
                igrid.AddSimpleColumn('invno', 'Invoice No', 100);
                igrid.AddFormatedColumn('invdate', 'Invoice Date', 80, 'dd-MM-yyyy');
            }

            var column = igrid.AddCustomColumn('dr', 'Debit', 100, function (item, col) {
                return $filter('IndianNumber')(item.dr, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.dr;
            }

            var column = igrid.AddCustomColumn('cr', 'Credit', 100, function (item, col) {
                return $filter('IndianNumber')(item.cr, 2);
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.cr;
            }

            var exist = result.vsummary.find(function (item) {
                return item.tp != 0;
            })

            if (exist && exist != null) {
                var column = igrid.AddCustomColumn('tp', 'GST %', 80, function (item, col) {
                    return $filter('IndianNumber')(item.tp, 2) + '%';
                });
                column.cssClass = 'rightalign';
                column.getValue = function (item, col) {
                    return item.tp;
                }
            }

            var addtaxable = false;
            var exist = result.vsummary.find(function (item) {
                return item.igst != 0;
            })

            if (exist && exist != null) {
                addtaxable = true;
                var column = igrid.AddCustomColumn('igst', 'IGST', 80, function (item, col) {
                    return $filter('IndianNumber')(item.igst, 2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return item.igst;
                }
            }

            var exist = result.vsummary.find(function (item) {
                return item.sgst != 0;
            })
            if (exist && exist != null) {
                addtaxable = true;
                var column = igrid.AddCustomColumn('sgst', 'SGST', 80, function (item, col) {
                    return $filter('IndianNumber')(item.sgst, 2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return item.sgst;
                }
            }


            var exist = result.vsummary.find(function (item) {
                return item.cgst != 0;
            })
            if (exist && exist != null) {
                addtaxable = true;
                var column = igrid.AddCustomColumn('cgst', 'CGST', 80, function (item, col) {
                    return $filter('IndianNumber')(item.cgst, 2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return item.cgst;
                }
            }

            if (addtaxable) {
                var column = igrid.AddCustomColumn('txble', 'Taxable', 80, function (item, col) {
                    return $filter('IndianNumber')(item.txble, 2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return item.txble;
                }
            }

            var exist = result.vsummary.find(function (item) {
                return item.tds != 0;
            })
            if (exist && exist != null) {
                var column = igrid.AddCustomColumn('tds', 'TDS', 80, function (item, col) {
                    return $filter('IndianNumber')(item.tds, 2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return item.tds;
                }
            }


            var exist = result.vsummary.find(function (item) {
                return item.tcs != 0;
            })
            if (exist && exist != null) {
                var column = igrid.AddCustomColumn('tcs', 'TCS', 80, function (item, col) {
                    return $filter('IndianNumber')(item.tcs, 2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return item.tcs;
                }
            }

            var exist = result.vsummary.find(function (item) {
                return item.cess != 0;
            })
            if (exist && exist != null) {
                var column = igrid.AddCustomColumn('cess', 'CESS', 80, function (item, col) {
                    return $filter('IndianNumber')(item.cess, 2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return item.cess;
                }
            }
            igrid.AddSimpleColumn('crbname', 'Created By', 100);
            igrid.AddSimpleColumn('narr', 'Narration', 200);

            $scope.selectedcount = 0;
            igrid.EnableTotal();
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.vsummary, '_id');
            $scope.SearchRequest.types
        }

        $scope.selectedcount = 0;
        $scope.EditVoucher = function () {
            $timeout(function () {
                var row = $scope.igrid.GetSelectedRows()[0];
                $state.go('voucher', { 'voucher': { 'id': row.vouchid } });
            }, 10);
        }

        $scope.GetCRDRValue = function (mode, vouchertype,sourcetype) {
            var amt = 0;
            $.each(this.items, function (k, v) {
                if (v.mode == mode && v.vouchertype == vouchertype && (sourcetype == '' || v.source == sourcetype)) {
                    amt += v.dr + v.cr;
                }
            });
            return amt;
        }

        $scope.ShowTaxMode = false;
        $scope.FilterTaxSummary = function (sourcetype) {
            var result = $scope.igrid.result;
            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.destroy();
            $scope.ConfigTaxSummaryGrid(result, sourcetype);
        }

        $scope.ConfigTaxSummaryGrid = function (result, sourcetype) {
            $scope.ShowTaxMode = true;
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            var column = igrid.AddCustomColumn('percentage', 'GST %', 60, function (item, col) {
                return $filter('IndianNumber')(item.percentage, 2);
            });
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                return item.percentage;
            }
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Detail',
                    name: 'Name',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];
            $.each(result.taxvouchertypes, function (k, v) {


                var column = igrid.AddCustomColumn('IGST' + v, 'IGST', 60, function (item, col) {

                    item.GetCRDRValue = $scope.GetCRDRValue;
                    var amt = item.GetCRDRValue(col.mode, col.vouchertype, col.sourcetype);
                    if (amt == undefined || amt == null || amt == 0 || isNaN(amt)) return '';
                    return $filter('IndianNumber')(amt, 2);

                    //var amt = item.items.find(function (item) {
                    //    return item.mode == col.mode && item.vouchertype == col.vouchertype;
                    //});
                    //if (amt && amt != null)
                    //  return $filter('IndianNumber')(amt.dr + amt.cr, 2);
                    //return '';
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {

                    item.GetCRDRValue = $scope.GetCRDRValue;
                    var amt = item.GetCRDRValue(col.mode, col.vouchertype, col.sourcetype);
                    if (amt == undefined || amt == null || amt == 0 || isNaN(amt)) return 0;
                    return amt;

                    //var amt = item.items.find(function (item) {
                    //    return item.mode == col.mode && item.vouchertype == col.vouchertype;
                    //});
                    //if (amt && amt != null)
                    //    return amt.dr + amt.cr;
                    //return 0;
                }
                column.sourcetype = sourcetype;
                column.vouchertype = v;
                column.mode = 3;

                var column = igrid.AddCustomColumn('SGST' + v, 'SGST', 60, function (item, col) {
                    item.GetCRDRValue = $scope.GetCRDRValue;
                    var amt = item.GetCRDRValue(col.mode, col.vouchertype, col.sourcetype);
                    if (amt == undefined || amt == null || amt == 0 || isNaN(amt)) return '';
                    return $filter('IndianNumber')(amt, 2);

                    //var amt = item.items.find(function (item) {
                    //    return item.mode == col.mode && item.vouchertype == col.vouchertype;
                    //});
                    //if (amt && amt != null)
                    //    return $filter('IndianNumber')(amt.dr + amt.cr, 2);
                    //return '';
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    item.GetCRDRValue = $scope.GetCRDRValue;
                    var amt = item.GetCRDRValue(col.mode, col.vouchertype, col.sourcetype);
                    if (amt == undefined || amt == null || amt == 0 || isNaN(amt)) return 0;
                    return amt;

                    //var amt = item.items.find(function (item) {
                    //    return item.mode == col.mode && item.vouchertype == col.vouchertype;
                    //});
                    //if (amt && amt != null)
                    //    return amt.dr + amt.cr;
                    //return 0;
                }
                column.vouchertype = v;
                column.mode = 4;
                column.sourcetype = sourcetype;

                var column = igrid.AddCustomColumn('CGST' + v, 'CGST', 60, function (item, col) {

                    item.GetCRDRValue = $scope.GetCRDRValue;
                    var amt = item.GetCRDRValue(col.mode, col.vouchertype, col.sourcetype);
                    if (amt == undefined || amt == null || amt == 0 || isNaN(amt)) return '';
                    return $filter('IndianNumber')(amt, 2);

                    //var amt = item.items.find(function (item) {
                    //    return item.mode == col.mode && item.vouchertype == col.vouchertype;
                    //});
                    //if (amt && amt != null)
                    //    return $filter('IndianNumber')(amt.dr + amt.cr, 2);
                    //return '';
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {

                    item.GetCRDRValue = $scope.GetCRDRValue;
                    var amt = item.GetCRDRValue(col.mode, col.vouchertype, col.sourcetype);
                    if (amt == undefined || amt == null || amt == 0 || isNaN(amt)) return 0;
                    return amt;

                    //    var amt = item.items.find(function (item) {
                    //        return item.mode == col.mode && item.vouchertype == col.vouchertype;
                    //    });
                    //    if (amt && amt != null)
                    //        return amt.dr + amt.cr;
                    //    return 0;
                }
                column.vouchertype = v;
                column.mode = 5;
                column.sourcetype = sourcetype;

                var header = $scope.SearchRef(v, $scope.vouchertypes, 'Unknown');
                groupColumns.push(
                    {
                        id: 'VoucherType' + v,
                        name: header,
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
                igrid.columns = [];
            })

            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.result = result;
            $scope.igrid.UpdateData(result.taxsummary, '_id');
        }

        $scope.ConfigTaxDetailGrid = function (result) {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddFormatedColumn('referencedate', 'Voucher Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('referenceno', 'Voucher No', 80);
            igrid.AddSimpleColumn('referencetype', 'Type', 80);
            igrid.AddSimpleColumn('name', 'Detail', 200);
            igrid.AddSimpleColumn('gstno', 'GST No', 100);

            var column = igrid.AddCustomColumn('taxpercentage', 'Tax %', 80, function (item, col) {
                return $filter('IndianNumber')(item.percentage, 2);
            });
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                return item.percentage;
            }
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Detail',
                    name: 'Name',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];


            var column = igrid.AddCustomColumn('iigst', 'IGST', 80, function (item, col) {
                if (item.mode == -3 || item.mode == 3)
                    return $filter('IndianNumber')(item.dr, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.mode == -3 || item.mode == 3)
                    return item.dr;
                return 0;
            }

            var column = igrid.AddCustomColumn('isgst', 'SGST', 80, function (item, col) {
                if (item.mode == -4)
                    return $filter('IndianNumber')((item.dr) / 2, 2);
                if (item.mode == 4)
                    return $filter('IndianNumber')((item.dr), 2);
                return '';
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.mode == -4)
                    return (item.dr) / 2;
                if (item.mode == 4)
                    return (item.dr);
                return 0;
            }


            var column = igrid.AddCustomColumn('icgst', 'CGST', 80, function (item, col) {
                if (item.mode == -4)
                    return $filter('IndianNumber')((item.dr) / 2, 2);
                if (item.mode == 4)
                    return $filter('IndianNumber')((item.dr), 2);
                return '';
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.mode == -4)
                    return (item.dr) / 2;
                if (item.mode == 4)
                    return (item.dr);
                return 0;
            }

            groupColumns.push(
                {
                    id: 'input',
                    name: 'Input',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];



            var column = igrid.AddCustomColumn('oigst', 'IGST', 80, function (item, col) {
                if (item.mode == -3 || item.mode == 3)
                    return $filter('IndianNumber')(item.dr + item.cr, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.mode == -3 || item.mode == 3)
                    return item.cr;
                return 0;
            }

            var column = igrid.AddCustomColumn('osgst', 'SGST', 80, function (item, col) {
                if (item.mode == -4)
                    return $filter('IndianNumber')((item.cr) / 2, 2);
                if (item.mode == 4)
                    return $filter('IndianNumber')((item.cr), 2);
                return '';
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.mode == -4)
                    return ( item.cr) / 2;
                if (item.mode == 4)
                    return (item.cr);
                return 0;
            }


            var column = igrid.AddCustomColumn('ocgst', 'CGST', 80, function (item, col) {
                if (item.mode == -4)
                    return $filter('IndianNumber')((item.cr) / 2, 2);
                if (item.mode == 4)
                    return $filter('IndianNumber')((item.cr), 2);
                return '';
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.mode == -4)
                    return (item.cr) / 2;
                if (item.mode == 4)
                    return (item.cr);
                return 0;
            }

            groupColumns.push(
                {
                    id: 'output',
                    name: 'Output',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.taxdetails, '_id');
        }

        $scope.ConfigDetailGrid = function (result) {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddFormatedColumn('date', 'Voucher Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('no', 'Voucher No', 80);
            igrid.AddReferenceColumn('type', 'Voucher Type', 100, $scope.vouchertypes);

            igrid.AddSimpleColumn('name', 'Ledger Name', 150);
            var column = igrid.AddCustomColumn('dr', 'Debit', 80, function (item, col) {
                return $filter('IndianNumber')(item.dr, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.dr;
            }
            var column = igrid.AddCustomColumn('cr', 'Credit', 80, function (item, col) {
                return $filter('IndianNumber')(item.cr, 2);
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.cr;
            }


            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.detail, '_id');
        }

        $scope.ConfigCompanySummaryGrid = function (result) {
            $scope.SupportPie = true;
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', '', 150);
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Name',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            $.each($scope.Company, function (k, v) {

                igrid.columns = [];
                var column = igrid.AddCustomColumn('dr' + v.id, 'Debit', 100, function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.lid == col.locationid;
                    })
                    if (val && val != null)
                        return $filter('IndianNumber')(val.dr, 2);

                    return '';
                });
                column.locationid = v.id;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.lid == col.locationid;
                    })
                    if (val && val != null)
                        return val.dr;
                    return 0;
                }
                var column = igrid.AddCustomColumn('cr' + v.id, 'Credit', 100, function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.lid == col.locationid;
                    })
                    if (val && val != null)
                        return $filter('IndianNumber')(val.cr, 2);

                    return '';
                });
                column.locationid = v.id;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.lid == col.locationid;
                    })
                    if (val && val != null)
                        return val.cr;
                    return 0;
                }


                var column = igrid.AddCustomColumn('balance' + v.id, 'Balance', 100, function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.lid == col.locationid;
                    })
                    if (val && val != null) {
                        if (val.dr > val.cr) {
                            return $filter('IndianNumber')(val.dr - val.cr, 2) + ' Dr';
                        } else if (val.dr < val.cr) {
                            return $filter('IndianNumber')(val.cr - val.dr, 2) + ' Cr';
                        }
                        return '';
                    }
                    return '';
                });
                column.locationid = v.id;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.lid == col.locationid;
                    })
                    if (val && val != null) {
                        if (val.dr > val.cr) {
                            return val.dr - val.cr;
                        } else if (val.dr < val.cr) {
                            return val.cr - val.dr;
                        }
                    }
                    return 0;
                }

                groupColumns.push(
                    {
                        id: 'loc_' + v.id,
                        name: v.name,
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            })
            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.summary, '_id');


        }

        $scope.ConfigMonthSummaryGrid = function (result) {
            $scope.SupportGraph = true;
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', '', 150);
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Name',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            $.each(result.mtitles, function (k, v) {

                igrid.columns = [];
                var column = igrid.AddCustomColumn('dr' + k, 'Debit', 100, function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.myear == col.myear;
                    })
                    if (val && val != null)
                        return $filter('IndianNumber')(val.dr, 2);

                    return '';
                });
                column.myear = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.myear == col.myear;
                    })
                    if (val && val != null)
                        return val.dr;
                    return 0;
                }

                var column = igrid.AddCustomColumn('cr' + k, 'Credit', 100, function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.myear == col.myear;
                    })
                    if (val && val != null)
                        return $filter('IndianNumber')(val.cr, 2);

                    return '';
                });
                column.myear = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.myear == col.myear;
                    })
                    if (val && val != null)
                        return val.cr;
                    return 0;
                }


                var column = igrid.AddCustomColumn('balance' + k, 'Balance', 100, function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.myear == col.myear;
                    })
                    if (val && val != null) {
                        if (val.dr > val.cr) {
                            return $filter('IndianNumber')(val.dr - val.cr, 2) + ' Dr';
                        } else if (val.dr < val.cr) {
                            return $filter('IndianNumber')(val.cr - val.dr, 2) + ' Cr';
                        }
                        return '';
                    }
                    return '';
                });
                column.myear = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var val = item.values.find(function (sitem) {
                        return sitem.myear == col.myear;
                    })
                    if (val && val != null) {
                        if (val.dr > val.cr) {
                            return val.dr - val.cr;
                        } else if (val.dr < val.cr) {
                            return val.cr - val.dr;
                        }
                    }
                    return 0;
                }

                groupColumns.push(
                    {
                        id: 'loc_' + k,
                        name: v,
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });

            })
            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.msummary, '_id');
        }

        $scope.ConfigLocationSummaryGrid = function (result) {
            $scope.SupportPie = true;
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', '', 150);
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Name',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            $.each($scope.Location, function (k, v) {
                if (v.view) {
                    igrid.columns = [];
                    var column = igrid.AddCustomColumn('dr' + v.id, 'Debit', 100, function (item, col) {
                        var val = item.values.find(function (sitem) {
                            return sitem.lid == col.locationid;
                        })
                        if (val && val != null)
                            return $filter('IndianNumber')(val.dr, 2);

                        return '';
                    });
                    column.locationid = v.id;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var val = item.values.find(function (sitem) {
                            return sitem.lid == col.locationid;
                        })
                        if (val && val != null)
                            return val.dr;
                        return 0;
                    }
                    var column = igrid.AddCustomColumn('cr' + v.id, 'Credit', 100, function (item, col) {
                        var val = item.values.find(function (sitem) {
                            return sitem.lid == col.locationid;
                        })
                        if (val && val != null)
                            return $filter('IndianNumber')(val.cr, 2);

                        return '';
                    });
                    column.locationid = v.id;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var val = item.values.find(function (sitem) {
                            return sitem.lid == col.locationid;
                        })
                        if (val && val != null)
                            return val.cr;
                        return 0;
                    }


                    var column = igrid.AddCustomColumn('balance' + v.id, 'Balance', 100, function (item, col) {
                        var val = item.values.find(function (sitem) {
                            return sitem.lid == col.locationid;
                        })
                        if (val && val != null) {
                            if (val.dr > val.cr) {
                                return $filter('IndianNumber')(val.dr - val.cr, 2) + ' Dr';
                            } else if (val.dr < val.cr) {
                                return $filter('IndianNumber')(val.cr - val.dr, 2) + ' Cr';
                            }
                            return '';
                        }
                        return '';
                    });
                    column.locationid = v.id;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var val = item.values.find(function (sitem) {
                            return sitem.lid == col.locationid;
                        })
                        if (val && val != null) {
                            if (val.dr > val.cr) {
                                return val.dr - val.cr;
                            } else if (val.dr < val.cr) {
                                return val.cr - val.dr;
                            }
                        }
                        return 0;
                    }

                    groupColumns.push(
                        {
                            id: 'loc_' + v.id,
                            name: v.name,
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                }
            })
            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.summary, '_id');
        }

        $scope.ConfigCompareGrid = function (result) {
            $scope.SupportGraph = true;
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', '', 150);
            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Name',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = [];
            var column = igrid.AddCustomColumn('dr1', 'Debit', 60, function (item, col) {
                return $filter('IndianNumber')(item.values[0].dr, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.values[0].dr;
            }
            var column = igrid.AddCustomColumn('cr1', 'Credit', 60, function (item, col) {
                return $filter('IndianNumber')(item.values[0].cr, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.values[0].cr;
            }

            var column = igrid.AddCustomColumn('balance1', 'Closing', 60, function (item, col) {
                if (item.values[0].dr > item.values[0].cr) {
                    return $filter('IndianNumber')(item.values[0].dr - item.values[0].cr, 2) + ' Dr';
                } else if (item.values[0].dr < item.values[0].cr) {
                    return $filter('IndianNumber')(item.values[0].cr - item.values[0].dr, 2) + ' Cr';
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.values[0].dr > item.values[0].cr) {
                    return item.values[0].dr - item.values[0].cr;
                } else if (item.values[0].dr < item.values[0].cr) {
                    return item.values[0].cr - item.values[0].dr;
                }
                return 0;
            }
            groupColumns.push(
                {
                    id: 'compare1',
                    name: result.title1,
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = [];

            var column = igrid.AddCustomColumn('dr2', 'Debit', 60, function (item, col) {
                return $filter('IndianNumber')(item.values[1].dr, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.values[1].dr;
            }

            var column = igrid.AddCustomColumn('cr2', 'Credit', 60, function (item, col) {
                return $filter('IndianNumber')(item.values[1].cr, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.values[1].cr;
            }



            var column = igrid.AddCustomColumn('balance3', 'Closing', 60, function (item, col) {
                if (item.values[1].dr > item.values[1].cr) {
                    return $filter('IndianNumber')(item.values[1].dr - item.values[1].cr, 2) + ' Dr';
                } else if (item.values[1].dr < item.values[1].cr) {
                    return $filter('IndianNumber')(item.values[1].cr - item.values[1].dr, 2) + ' Cr';
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.values[1].dr > item.values[1].cr) {
                    return item.values[1].dr - item.values[1].cr;
                } else if (item.values[1].dr < item.values[1].cr) {
                    return item.values[1].cr - item.values[1].dr;
                }
                return 0;
            }
            groupColumns.push(
                {
                    id: 'compare2',
                    name: result.title2,
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.summary, '_id');
        }

        $scope.SelectAll = function (list, value) {
            $.each(list, function (k, v) {
                v.view = value;
            })
        }

        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings &&
                $scope.PurchaseType && $scope.PurchaseType != null) {
                $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
                $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;
                $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
                $scope.SearchRequest = $scope.LocalContext.Get('$scope.SearchRequest', false);
                $scope.LastResult = $scope.LocalContext.Get('$scope.LastResult', false);
                if ($scope.LastResult && $scope.LastResult != null) {
                    if ($scope.SearchRequest.types &&
                        $scope.SearchRequest.types != null) {

                        $.each($scope.SearchRequest.types, function (k, v) {
                            var exist = $scope.ItemTypes.find(function (item) {
                                return item.id == v;
                            });
                            if (exist && exist != null)
                                exist.view = true;
                        });
                    }

                    if ($scope.SearchRequest.vtypes &&
                        $scope.SearchRequest.vtypes != null) {
                        $.each($scope.SearchRequest.vtypes, function (k, v) {
                            var exist = $scope.vouchertypes.find(function (item) {
                                return item.id == v;
                            });
                            if (exist && exist != null)
                                exist.view = true;
                        });
                    }

                    if ($scope.SearchRequest.locations &&
                        $scope.SearchRequest.locations != null) {
                        if ($scope.Location && $scope.Location != null) {
                            $.each($scope.SearchRequest.locations, function (k, v) {
                                var exist = $scope.Location.find(function (item) {
                                    return item.id == v;
                                });
                                if (exist && exist != null)
                                    exist.view = true;
                            });
                        }
                    }

                    $scope.ShowResult($scope.LastResult);
                }
            } else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 10);
            }
        }

        $scope.ShowGraph = false;
        $scope.SupportGraph = false;
        $scope.HideGraph = false;
        $scope.SupportPie = false;
        $scope.RemoveGraph = function () {
            if ($scope.myBar && $scope.myBar != null) {
                $scope.myBar.destroy();
            }
            $scope.HideGraph = false;
            $scope.ShowGraph = false;
        }

        $scope.DrawPie = function () {
            if ($scope.ShowGraph == false) {
                $scope.ShowGraph = true;
                $scope.HideGraph = true;
                $timeout(function () {
                    $scope.DrawPie();
                }, 25);
                return;
            }
            else {
                $scope.ShowGraph = true;
                $scope.HideGraph = true;
            }

            $scope.LastResult.mtitles = [];
            if ($scope.SearchRequest.modeid == 5) {
                $.each($scope.LastResult.chart, function (k, v) {
                    var l = $scope.Location.find(function (item) {
                        return item.id == v.lid;
                    });
                    if (l && l != null) {
                        v.name = l.name;
                    }
                });
            }
            else if ($scope.SearchRequest.modeid == 1) {
                $.each($scope.LastResult.chart, function (k, v) {
                    var l = $scope.Company.find(function (item) {
                        return item.id == v.lid;
                    });
                    if (l && l != null) {
                        v.name = l.name;
                    }
                });
            }



            var colorArray = ['#FF6633', '#FFB399', '#FF33FF', '#FFFF99', '#00B3E6',
                '#E6B333', '#3366E6', '#999966', '#99FF99', '#B34D4D',
                '#80B300', '#809900', '#E6B3B3', '#6680B3', '#66991A',
                '#FF99E6', '#CCFF1A', '#FF1A66', '#E6331A', '#33FFCC',
                '#66994D', '#B366CC', '#4D8000', '#B33300', '#CC80CC',
                '#66664D', '#991AFF', '#E666FF', '#4DB3FF', '#1AB399',
                '#E666B3', '#33991A', '#CC9999', '#B3B31A', '#00E680',
                '#4D8066', '#809980', '#E6FF80', '#1AFF33', '#999933',
                '#FF3380', '#CCCC00', '#66E64D', '#4D80CC', '#9900B3',
                '#E64D66', '#4DB380', '#FF4D4D', '#99E6E6', '#6666FF'];

            var lables = [];
            var salesvalues = [];
            var colours = [];
            $.each($scope.LastResult.chart, function (k, v) {
                lables.push(v.name);
                salesvalues.push(v.cr > v.dr ? v.cr : v.dr);
                colours.push(colorArray[k]);
            });


            var barChartData = {
                labels: lables,
                datasets: [
                    {
                        data: salesvalues,
                        backgroundColor: colours,
                    }]
            };

            var ctx = document.getElementById("canvas").getContext("2d");

            if ($scope.myBar && $scope.myBar != null) {
                $scope.myBar.destroy();
            }

            $scope.myBar = new Chart(ctx, {
                type: 'pie',
                data: barChartData,
                legend: {
                    display: true
                },
                options: {
                    responsive: true,
                    plugins: {
                        labels: {
                            render: function (args) {
                                return args.label + '(' + args.percentage + '%' + ')';
                            },
                            fontColor: '#000',
                            position: 'outside'
                        }
                    }
                }
            });
        }

        $scope.DrawGraph = function (topcount) {
            if ($scope.ShowGraph == false) {
                $scope.ShowGraph = true;
                $scope.HideGraph = true;
                $timeout(function () {
                    $scope.DrawGraph(topcount);
                }, 25);
                return;
            }
            else {
                $scope.ShowGraph = true;
                $scope.HideGraph = true;
            }
            if ($scope.SearchRequest.modeid == 1)
                $scope.CompanyChart(topcount);
            else if ($scope.SearchRequest.modeid == 3)
                $scope.MonthlyChart(topcount);
            else if ($scope.SearchRequest.modeid == 2)
                $scope.CompareChart(topcount);
        }

        $scope.CompareChart = function (topcount) {

            topcount = topcount == undefined ? $scope.LastResult.summary.length : topcount;
            $scope.LastResult.mtitles = [];
            $scope.LastResult.mtitles.push({ 'id': 0, 'name': $scope.LastResult.title1 });
            $scope.LastResult.mtitles.push({ 'id': 1, 'name': $scope.LastResult.title2 });
            var lables = [];
            var salesarray = [];
            var salesvalues = [];
            $.each($scope.LastResult.mtitles, function (k, v) {
                var value = [];
                salesvalues.push(value);
                salesarray[v.name] = value;
            });

            $.each($scope.LastResult.summary, function (k, v) {
                if (topcount != lables.length) {
                    lables.push(v.name);
                    $.each($scope.LastResult.mtitles, function (ik, iv) {
                        if (v.values[iv.id].cr > v.values[iv.id].dr)
                            salesarray[iv.name].push(v.values[iv.id].cr);
                        else
                            salesarray[iv.name].push(v.values[iv.id].dr);
                    })
                }
            });

            var colorArray = ['#FF6633', '#FFB399', '#FF33FF', '#FFFF99', '#00B3E6',
                '#E6B333', '#3366E6', '#999966', '#99FF99', '#B34D4D',
                '#80B300', '#809900', '#E6B3B3', '#6680B3', '#66991A',
                '#FF99E6', '#CCFF1A', '#FF1A66', '#E6331A', '#33FFCC',
                '#66994D', '#B366CC', '#4D8000', '#B33300', '#CC80CC',
                '#66664D', '#991AFF', '#E666FF', '#4DB3FF', '#1AB399',
                '#E666B3', '#33991A', '#CC9999', '#B3B31A', '#00E680',
                '#4D8066', '#809980', '#E6FF80', '#1AFF33', '#999933',
                '#FF3380', '#CCCC00', '#66E64D', '#4D80CC', '#9900B3',
                '#E64D66', '#4DB380', '#FF4D4D', '#99E6E6', '#6666FF'];

            var datasets = [];
            var idx = 1;
            $.each(salesvalues, function (k, v) {
                datasets.push({
                    label: $scope.LastResult.mtitles[datasets.length].name,
                    stack: 'Stack',
                    type: 'bar',
                    fill: true,
                    borderColor: colorArray[datasets.length],
                    backgroundColor: colorArray[datasets.length],
                    yAxisID: "y-axis-1",
                    data: v
                })
            });

            var barChartData = {
                labels: lables,
                datasets: datasets
            };

            var ctx = document.getElementById("canvas").getContext("2d");

            if ($scope.myBar && $scope.myBar != null) {
                $scope.myBar.destroy();
            }

            $scope.myBar = new Chart(ctx, {
                type: 'bar',
                data: barChartData,
                options: {

                    tooltips: {
                        mode: 'index',
                        intersect: false
                    },
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Finance Anayser'
                        },
                    },
                    responsive: true,
                    scales: {
                        xAxes: [
                            {
                                ticks: {
                                    mirror: true,
                                    autoSkip: false,
                                    //maxRotation: 90,
                                    //minRotation: 90,
                                    //    padding: -110
                                }
                            }
                        ],
                        yAxes: [{
                            type: "linear", // only linear but allow scale type registration. This allows extensions to exist solely for log scale for instance
                            display: true,
                            position: "left",
                            id: "y-axis-1",
                            ticks: {
                                // Include a dollar sign in the ticks
                                callback: function (value, index, values) {
                                    var divider = 10000000;
                                    var dividerLabel = 'Cr';
                                    var fixed = 2;
                                    if (value > 10000000) {
                                        divider = 10000000;
                                        dividerLabel = 'Cr';
                                    } else if (value > 100000) {
                                        divider = 100000;
                                        dividerLabel = 'Lac';
                                    } else if (value > 1000) {
                                        divider = 1000;
                                        dividerLabel = 'K';
                                    }
                                    else {
                                        divider = 1;
                                        dividerLabel = '';
                                        fixed = 0;
                                    }

                                    return (parseFloat(value) / divider).toFixed(fixed) + dividerLabel;
                                }
                            },
                            stacked: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Value'
                            }
                        }]
                    }
                }
            });
        }

        $scope.CompanyChart = function (topcount) {

            $scope.LastResult.mtitles = [];
            $.each($scope.Company, function (k, v) {
                $scope.LastResult.mtitles.push({ 'id': v.id, 'name': v.name });
            })
            var lables = [];
            var salesarray = [];
            var salesvalues = [];
            $.each($scope.LastResult.mtitles, function (k, v) {
                var value = {
                    'cr': [], 'dr': []
                };
                salesvalues.push(value);
                salesarray[v.name] = value;
            });

            $scope.LastResult.summary.sort(function (a, b) {
                var aval = 0;
                var bval = 0;
                $.each(a.values, function (kk, vv) {
                    aval += vv.cr + vv.dr;
                });

                $.each(b.values, function (kk, vv) {
                    bval += vv.cr + vv.dr;
                });
                return bval - aval;
            });

            var summary = $scope.LastResult.summary;
            if (topcount && topcount > 0) {
                summary = [];
                for (var i = 0; i < topcount &&
                    i < $scope.LastResult.summary.length; i++) {
                    summary.push($scope.LastResult.summary[i]);
                }
            }

            $.each(summary, function (k, v) {
                lables.push(v.name);
                $.each($scope.LastResult.mtitles, function (ik, iv) {
                    var exist = v.values.find(function (item) {
                        return item.lid == iv.id;
                    });
                    if (exist) {
                        salesarray[iv.name].cr.push(exist.cr);
                        salesarray[iv.name].dr.push(exist.dr);
                    }
                    else {
                        salesarray[iv.name].cr.push(0);
                        salesarray[iv.name].dr.push(0);
                    }
                })
            });

            var colorArray = ['#FF6633', '#FFB399', '#FF33FF', '#FFFF99', '#00B3E6',
                '#E6B333', '#3366E6', '#999966', '#99FF99', '#B34D4D',
                '#80B300', '#809900', '#E6B3B3', '#6680B3', '#66991A',
                '#FF99E6', '#CCFF1A', '#FF1A66', '#E6331A', '#33FFCC',
                '#66994D', '#B366CC', '#4D8000', '#B33300', '#CC80CC',
                '#66664D', '#991AFF', '#E666FF', '#4DB3FF', '#1AB399',
                '#E666B3', '#33991A', '#CC9999', '#B3B31A', '#00E680',
                '#4D8066', '#809980', '#E6FF80', '#1AFF33', '#999933',
                '#FF3380', '#CCCC00', '#66E64D', '#4D80CC', '#9900B3',
                '#E64D66', '#4DB380', '#FF4D4D', '#99E6E6', '#6666FF'];

            var datasets = [];
            var idx = 1;
            var titleidx = 0;
            $.each(salesvalues, function (k, v) {
                datasets.push({
                    label: $scope.LastResult.mtitles[titleidx].name,
                    stack: 'Credit',
                    type: 'bar',
                    fill: true,
                    borderColor: colorArray[titleidx],
                    backgroundColor: colorArray[titleidx],
                    yAxisID: "y-axis-1",
                    data: v.cr
                })
                datasets.push({
                    label: $scope.LastResult.mtitles[titleidx].name,
                    stack: 'Debit',
                    type: 'bar',
                    fill: true,
                    borderColor: colorArray[titleidx],
                    backgroundColor: colorArray[titleidx],
                    yAxisID: "y-axis-2",
                    data: v.dr
                });
                titleidx++;
            });


            var barChartData = {
                labels: lables,
                datasets: datasets
            };

            var ctx = document.getElementById("canvas").getContext("2d");

            if ($scope.myBar && $scope.myBar != null) {
                $scope.myBar.destroy();
            }

            $scope.myBar = new Chart(ctx, {
                type: 'bar',
                data: barChartData,
                options: {

                    tooltips: {
                        mode: 'index',
                        intersect: false
                    },
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Finance Anayser'
                        },
                    },
                    responsive: true,
                    scales: {
                        yAxes: [{
                            type: "linear", // only linear but allow scale type registration. This allows extensions to exist solely for log scale for instance
                            display: true,
                            position: "left",
                            id: "y-axis-1",
                            ticks: {
                                // Include a dollar sign in the ticks
                                callback: function (value, index, values) {
                                    var divider = 10000000;
                                    var dividerLabel = 'Cr';
                                    var fixed = 2;
                                    if (value > 10000000) {
                                        divider = 10000000;
                                        dividerLabel = 'Cr';
                                    } else if (value > 100000) {
                                        divider = 100000;
                                        dividerLabel = 'Lac';
                                    } else if (value > 1000) {
                                        divider = 1000;
                                        dividerLabel = 'K';
                                    }
                                    else {
                                        divider = 1;
                                        dividerLabel = '';
                                        fixed = 0;
                                    }

                                    return (parseFloat(value) / divider).toFixed(fixed) + dividerLabel;
                                }
                            },
                            stacked: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Value'
                            }
                        }]
                    }
                }
            });
        }

        $scope.MonthlyChart = function (toplist) {
            var lables = [];
            var salesarray = [];
            var salesvalues = [];
            if (toplist == undefined)
                toplist = $scope.LastResult.msummary.length;

            $.each($scope.LastResult.mtitles, function (k, v) {
                var value = [];
                salesvalues.push(value);
                salesarray[v] = value;
            });
            $.each($scope.LastResult.msummary, function (k, v) {
                if (lables.length != toplist) {
                    lables.push(v.name);
                    $.each($scope.LastResult.mtitles, function (ik, iv) {
                        var exist = v.values.find(function (item) {
                            return item.myear == iv;
                        });
                        if (exist)
                            salesarray[iv].push(exist.cr < exist.dr ? exist.dr : exist.cr);
                        else
                            salesarray[iv].push(0);
                    })
                }
            });
            var colorArray = ['#FF6633', '#FFB399', '#FF33FF', '#FFFF99', '#00B3E6',
                '#E6B333', '#3366E6', '#999966', '#99FF99', '#B34D4D',
                '#80B300', '#809900', '#E6B3B3', '#6680B3', '#66991A',
                '#FF99E6', '#CCFF1A', '#FF1A66', '#E6331A', '#33FFCC',
                '#66994D', '#B366CC', '#4D8000', '#B33300', '#CC80CC',
                '#66664D', '#991AFF', '#E666FF', '#4DB3FF', '#1AB399',
                '#E666B3', '#33991A', '#CC9999', '#B3B31A', '#00E680',
                '#4D8066', '#809980', '#E6FF80', '#1AFF33', '#999933',
                '#FF3380', '#CCCC00', '#66E64D', '#4D80CC', '#9900B3',
                '#E64D66', '#4DB380', '#FF4D4D', '#99E6E6', '#6666FF'];

            var datasets = [];
            var idx = 1;
            $.each(salesvalues, function (k, v) {
                datasets.push({
                    label: $scope.LastResult.mtitles[datasets.length],
                    stack: 'Stack',
                    type: 'bar',
                    fill: true,
                    borderColor: colorArray[datasets.length],
                    backgroundColor: colorArray[datasets.length],
                    yAxisID: "y-axis-1",
                    data: v
                })
            });


            var barChartData = {
                labels: lables,
                datasets: datasets
            };

            var ctx = document.getElementById("canvas").getContext("2d");

            if ($scope.myBar && $scope.myBar != null) {
                $scope.myBar.destroy();
            }

            $scope.myBar = new Chart(ctx, {
                type: 'bar',
                data: barChartData,
                options: {
                    tooltips: {
                        mode: 'index',
                        intersect: false
                    },
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Finance Anayser'
                        },
                    },
                    responsive: true,
                    scales: {
                        xAxes: [
                            {
                                ticks: {
                                    mirror: true,
                                    autoSkip: false,
                                    //maxRotation: 90,
                                    //minRotation: 90,
                                    //    padding: -110
                                }
                            }
                        ],
                        yAxes: [{
                            type: "linear", // only linear but allow scale type registration. This allows extensions to exist solely for log scale for instance
                            display: true,
                            position: "left",
                            id: "y-axis-1",
                            ticks: {
                                // Include a dollar sign in the ticks
                                callback: function (value, index, values) {
                                    var divider = 10000000;
                                    var dividerLabel = 'Cr';
                                    var fixed = 2;
                                    if (value > 10000000) {
                                        divider = 10000000;
                                        dividerLabel = 'Cr';
                                    } else if (value > 100000) {
                                        divider = 100000;
                                        dividerLabel = 'Lac';
                                    } else if (value > 1000) {
                                        divider = 1000;
                                        dividerLabel = 'K';
                                    }
                                    else {
                                        divider = 1;
                                        dividerLabel = '';
                                        fixed = 0;
                                    }

                                    return (parseFloat(value) / divider).toFixed(fixed) + dividerLabel;
                                }
                            },
                            stacked: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'Value'
                            }
                        }]
                    }
                }
            });
        }
        $scope.LocalContext = cntxService.CreatePageContext('finanalyser');
        $scope.$on(
            "$destroy",
            function (event) {
                $scope.LocalContext.Post('$scope.SearchRequest', $scope.SearchRequest, false);
                $scope.LocalContext.Post('$scope.LastResult', $scope.LastResult, false);
            }
        );

        $timeout(function () {
            $scope.LoadAppSetting();

        }, 10);
    }]);
