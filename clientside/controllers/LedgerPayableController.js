var app = angular.module('InfinityLab');
app.controller('LedgerPayableController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter', '$state','cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter, $state, cntxService) {
        $rootScope.AccordionName = $location.path();



        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = false;
        $scope.AllowEnter = false;
        $scope.SearchResult = [];

        $scope.Ledgers = restService.Get('ReferencelistService/LEDGERHEADERS', true).then(function (results) {
            $scope.Ledgers = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'SearchCondition.companyid');
        });

        restService.Get('ReferencelistService/SUPPLIERLEDGER', true).then(function (results) {
            $scope.Supplier = results;
        });

        restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transport = results;
        });

        $scope.ErrorStyle = "success";

        $scope.ErrorMessage = "";


        $scope.SearchDaybook = function () {

            if ($scope.SearchCondition.reporttypeid &&
                $scope.SearchCondition.reporttypeid == 4 &&
                $scope.SearchCondition.ledgerid &&
                $scope.SearchCondition.ledgerid > 0 &&
                $scope.SearchCondition.sourceid &&
                $scope.SearchCondition.sourceid == 2) {
                $scope.SearchTransportLedger();
                return;
            }

            if ($scope.SearchCondition.reporttypeid &&
                $scope.SearchCondition.reporttypeid == 4 &&
                $scope.SearchCondition.ledgerid &&
                $scope.SearchCondition.ledgerid > 0 &&
                $scope.SearchCondition.sourceid &&
                $scope.SearchCondition.sourceid == 1) {
                $scope.SearchSupplierLedger();
                return;
            }

            if ($scope.SearchCondition.reporttypeid &&
                $scope.SearchCondition.reporttypeid == 4 &&
                $scope.SearchCondition.ledgerid &&
                $scope.SearchCondition.ledgerid > 0 &&
                $scope.SearchCondition.sourceid &&
                $scope.SearchCondition.sourceid == 0) {
                $scope.SearchVoucherLedger();
                return;
            }

            if ($scope.SearchCondition.reporttypeid &&
                $scope.SearchCondition.reporttypeid == 5 &&
                $scope.SearchCondition.ledgerid &&
                $scope.SearchCondition.ledgerid > 0 &&
                $scope.SearchCondition.sourceid &&
                $scope.SearchCondition.sourceid == 0) {
                $scope.SearchVoucherSummary();
                return;
            }

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "VoucherService/SearchPayable",
                data: srequest
            }).success(function (result) {
                $scope.BuildGrid(result);
            }).error(function (result) {
            });
        }

        $scope.Sources = [
            { 'id': 0, 'name': 'Ledger' },
            { 'id': 1, 'name': 'Supplier' },
            { 'id': 2, 'name': 'Transport' },
            { 'id': 5, 'name': 'All' },
        ];

        $scope.ReportType = [
            { 'id': 0, 'name': 'Detail' },
            { 'id': 1, 'name': 'Summary' },
            { 'id': 2, 'name': 'Agewise Detail' },
            { 'id': 3, 'name': 'Agewise Summary' },
            { 'id': 4, 'name': 'Finance Ledger' },
            { 'id': 5, 'name': 'Voucher Summary' },
        ];

        $scope.refvouchertypelist = [
            { 'id': -1, 'name': '' },
            { 'id': 0, 'name': 'Petty Cash' },
            { 'id': 1, 'name': 'Payment' },
            { 'id': 2, 'name': 'Receipt' },
            { 'id': 3, 'name': 'Purchase' },
            { 'id': 4, 'name': 'Sales' },
            { 'id': 5, 'name': 'Credit Note' },
            { 'id': 6, 'name': 'Debit Note' },
            { 'id': 7, 'name': 'Contra' },
            { 'id': 8, 'name': 'Journal' },
            { 'id': 9, 'name': 'Cash Deposit' },
            { 'id': 10, 'name': 'Cash Widhrawal' },
            { 'id': 11, 'name': 'Main Cash to Petty Cash' },
            { 'id': 12, 'name': 'Petty Cash to Main Cash' }
        ];

        $scope.exporttoExcel = function (infygrid, filename) {
            var title = 'Accounts Payable';
            if ($scope.SearchCondition.ledgerid &&
                $scope.SearchCondition.ledgerid > 0) {
                if ($scope.SearchCondition.sourceid == 0) {
                    var ledger = $scope.Ledgers.find(function (item) {
                        return item.id == $scope.SearchCondition.ledgerid;
                    });
                    if (ledger && ledger != null)
                        title = ledger.displayname;
                }
                else if ($scope.SearchCondition.sourceid == 1) {
                    title = $scope.SearchRef($scope.SearchCondition.ledgerid, $scope.Supplier, 'Accounts Payable');
                }
                else {
                    title = $scope.SearchRef($scope.SearchCondition.ledgerid, $scope.Transport, 'Accounts Payable');
                }
            }

            var fromdate = $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy')
            var todate = $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy')
            subtitle = fromdate + ' - ' + todate;
            var additionalrows = [];
            if ($scope.ShowClosingBalance) {
                additionalrows.push(['','', '', 'Closing Balance',
                    $scope.ClosingBalance.debit,
                    $scope.ClosingBalance.credit]);
            }

            if ($scope.FinGridData && $scope.FinGridData != null &&
                $scope.FinGridData.transactions &&
                $scope.FinGridData.transactions.length > 5000) {
                infygrid.ExportCSV(infygrid.grid, title, subtitle, true, additionalrows);
            }
            else {
                infygrid.ExportXLSX(infygrid.grid, title, subtitle, true, additionalrows);
            }
            

            //$scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle, true, additionalrows);
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

        $scope.DownloadURL = null;
        $scope.SearchTransportLedger = function () {
            $scope.ShowClosingBalance = false;

            if ($scope.SearchCondition.locationid == undefined ||
                $scope.SearchCondition.locationid == null) {
                $scope.SearchCondition.locationid = 0;
            }
            if ($scope.SearchCondition.companyid == undefined ||
                $scope.SearchCondition.companyid == null) {
                $scope.SearchCondition.companyid = 0;
            }

            var url = 'TransportpaymentService/TransportLedger/'
                + $scope.SearchCondition.companyid + '/' + $scope.SearchCondition.ledgerid + '/'
                + $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy') + '/'
                + $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy');

            restService.Get(url, true).then(function (results) {

                $scope.DownloadURL = 'TransportpaymentService/PrintSummaryPDF/' + $scope.UserInfo.OrganizationID + '/'
                    + $scope.SearchCondition.companyid + '/' + $scope.SearchCondition.ledgerid + '/'
                    + $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy') + '/'
                    + $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy');
                $scope.BuildFinGrid(results);
            });
        }

        $scope.SearchSupplierLedger = function () {

            if ($scope.SearchCondition.locationid == undefined ||
                $scope.SearchCondition.locationid == null) {
                $scope.SearchCondition.locationid = 0;
            }
            if ($scope.SearchCondition.companyid == undefined ||
                $scope.SearchCondition.companyid == null) {
                $scope.SearchCondition.companyid = 0;
            }

            var url = 'SupplierpaymentService/SupplierLedger/'
                + $scope.SearchCondition.companyid + '/' + $scope.SearchCondition.locationid + '/' + $scope.SearchCondition.ledgerid + '/'
                + $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy') + '/'
                + $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy');

            restService.Get(url, true).then(function (results) {
                $scope.DownloadURL = 'SupplierpaymentService/PrintSummaryPDF/' + $scope.UserInfo.OrganizationID + '/'
                    + $scope.SearchCondition.companyid + '/' + $scope.SearchCondition.locationid + '/' + $scope.SearchCondition.ledgerid + '/'
                    + $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy') + '/'
                    + $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy');
                $scope.BuildFinGrid(results);
            });
        }

        $scope.SearchVoucherLedger = function () {
            $scope.ShowClosingBalance = false;

            if ($scope.SearchCondition.locationid == undefined ||
                $scope.SearchCondition.locationid == null) {
                $scope.SearchCondition.locationid = 0;
            }
            if ($scope.SearchCondition.companyid == undefined ||
                $scope.SearchCondition.companyid == null) {
                $scope.SearchCondition.companyid = 0;
            }

            var ShowLedgerConfig = btoa(JSON.stringify({
                'c': $scope.SearchCondition.companyid,
                'f': $scope.SearchCondition.fromdate,
                't': $scope.SearchCondition.todate,
                'ln': $scope.SearchCondition.locationid,
                'ld': $scope.SearchCondition.ledgerid
            }));
            var url = 'LedgerService/LedgerSummary/' + ShowLedgerConfig;
            restService.Get(url, true).then(function (results) {
                $scope.DownloadURL = 'LedgerService/PrintLedgerPDF/' + $scope.UserInfo.OrganizationID + '/' + ShowLedgerConfig;
                $scope.BuildFinGrid(results);
            });
        }

        $scope.SearchVoucherSummary = function () {
            $scope.ShowClosingBalance = false;

            if ($scope.SearchCondition.locationid == undefined ||
                $scope.SearchCondition.locationid == null) {
                $scope.SearchCondition.locationid = 0;
            }
            if ($scope.SearchCondition.companyid == undefined ||
                $scope.SearchCondition.companyid == null) {
                $scope.SearchCondition.companyid = 0;
            }

            var ShowLedgerConfig = btoa(JSON.stringify({
                'c': $scope.SearchCondition.companyid,
                'f': $scope.SearchCondition.fromdate,
                't': $scope.SearchCondition.todate,
                'ln': $scope.SearchCondition.locationid,
                'ld': $scope.SearchCondition.ledgerid
            }));
            var url = 'LedgerService/VoucherSummary/' + ShowLedgerConfig;
            restService.Get(url, true).then(function (results) {
                $scope.DownloadURL = null;
                $scope.BuildFinSummaryGrid(results);
            });
        }

        $scope.BuildFinSummaryGrid = function (result) {
            $scope.FinGridData = result;
            $scope.GridData = null;

            var opening = result.transactions.find(function (item) {
                return item.type == 'Opening';
            });

            if (opening == undefined || opening == null) {
                result.transactions.splice(0, 0, {
                    'type': 'Opening', 'description': '',
                    'debit': result.obdebit,
                    'credit': result.obcredit
                });
            }

            $scope.ShowClosingBalance = true;
            $scope.ClosingBalance = {
                'type': 'Closing', 'description': '',
                'cbcredit': result.cbcredit,
                'cbdebit': result.cbdebit
            };

            $scope.ClosingBalance.credit = $scope.ClosingBalance.cbcredit > 0 ? $filter('IndianNumber')($scope.ClosingBalance.cbcredit, 2) + 'Cr' : '';
            $scope.ClosingBalance.debit = $scope.ClosingBalance.cbdebit > 0 ? $filter('IndianNumber')($scope.ClosingBalance.cbdebit, 2) + 'Dr' : '';

            //result.transactions.push({
            //    'type': 'Closing', 'description': '',
            //    'credit': result.cbcredit,
            //    'debit': result.cbdebit
            //});

            $scope.FinSummary = result;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            //igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            var column = igrid.AddCustomColumn('date', 'Date', 100, function (item, col) {
                return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>"
            });

            var column = igrid.AddCustomColumn('date', 'Date', 100, function (item, col) {
                if (item.date)
                    return $filter('date')(item.date, 'dd-MM-yyyy');
                return '';
            });

            igrid.AddSimpleColumn('type', 'Source', 100);
            igrid.AddSimpleColumn('description', 'Detail', 350);

            var column = igrid.AddCustomColumn('debit', 'Debit', 100, function (item, col) {
                if (item.debit && item.debit != null)
                    return $filter('IndianNumber')(item.debit, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.debit && item.debit != null)
                    return item.debit;
                return 0;
            }

            var column = igrid.AddCustomColumn('credit', 'Credit', 100, function (item, col) {
                if (item.credit && item.credit != null)
                    return $filter('IndianNumber')(item.credit, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.credit && item.credit != null)
                    return item.credit;
                return 0;
            }

            var column = igrid.AddCustomColumn('gst', 'GST', 100, function (item, col) {
                if (item.gst && item.gst>0)
                    return $filter('IndianNumber')(item.gst, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.gst;
            }

            var column = igrid.AddCustomColumn('tds', 'TDS', 100, function (item, col) {
                if (item.tds && item.tds > 0)
                    return $filter('IndianNumber')(item.tds, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.tds;
            }

            var column = igrid.AddCustomColumn('narration', 'Narration', 150, function (item, col) {
                if (item.detail && item.detail != null)
                    return item.detail.narration;
                return ''
            });

            if (result.bankid && result.bankid > 0) {
                var column = igrid.AddCustomColumn('bankreference', 'Chq No', 100, function (item, col) {
                    if (item.detail && item.detail != null)
                        return item.detail.bankreference;
                    return ''
                });
                var column = igrid.AddCustomColumn('bankdate', 'Chq Date', 100, function (item, col) {
                    if (item.detail && item.detail != null) {
                        var chequedate = $filter('date')(item.detail.bankdate, 'dd-MM-yyyy');
                        if (chequedate == '01-01-0001' || chequedate == '01-01-1901') return '';
                        return chequedate;
                    }
                    return ''
                });
            }
            else {
                var column = igrid.AddCustomColumn('voucherno', 'Vch. No', 100, function (item, col) {
                    if (item.detail && item.detail != null)
                        return item.detail.no;
                    return ''
                });
                var column = igrid.AddCustomColumn('adjno', 'Adj. No', 100, function (item, col) {
                    if (item.detail && item.detail != null) {
                        if (item.detail.adjno && item.detail.adjno != null && item.detail.adjno!='')
                            return item.detail.adjno;
                    }
                    return ''
                });
                var column = igrid.AddCustomColumn('invoiceno', 'Inv. No', 100, function (item, col) {
                    if (item.detail && item.detail != null)
                        return item.detail.invoiceno;
                    return ''
                });
                var column = igrid.AddCustomColumn('invoicedate', 'Inv. Date', 100, function (item, col) {
                    if (item.detail && item.detail != null) {
                        var invoicedate = $filter('date')(item.detail.invoicedate, 'dd-MM-yyyy');
                        if (invoicedate == '01-01-0001' || invoicedate == '01-01-1901') return '';
                        return invoicedate;
                    }
                    return ''
                });
            }

            var column = igrid.AddCustomColumn('createdby', 'Created By', 100, function (item, col) {
                if (item.cuser && item.cuser != null)
                    return item.cuser;

                if (item.detail && item.detail != null)
                    return item.detail.cuser;
                return ''
            });

            var column = igrid.AddCustomColumn('modifiedby', 'Modified By', 100, function (item, col) {
                if (item.muser && item.muser != null)
                    return item.muser;

                if (item.detail && item.detail != null)
                    return item.detail.muser;
                return ''
            });

            $scope.igrid = igrid;
            $scope.selectedcount = 0;
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Edit') {
                    if (item.detail.isvoucher) {
                        $timeout(function () {
                            $state.go('voucher', { 'voucher': item.detail });
                        }, 10);
                    }
                }
            });
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.transactions, '_id');
        }

        $scope.selectedcount = 0;
        $scope.EditVoucher = function () {
            $timeout(function () {
                var row = $scope.igrid.GetSelectedRows()[0];
                if (row.voucherid && row.voucherid > 0)
                    $state.go('voucher', { 'voucher': { 'id': row.voucherid } });
            }, 10);
        }

        $scope.BuildFinGrid = function (result) {
            $scope.FinGridData = result;
            $scope.GridData = null;

            var opening = result.transactions.find(function (item) {
                return item.type == 'Opening';
            });

            if (opening == undefined || opening == null) {
                result.transactions.splice(0, 0, {
                    'type': 'Opening', 'description': '',
                    'debit': result.obdebit,
                    'credit': result.obcredit
                });
            }

            $scope.ShowClosingBalance = true;
            $scope.ClosingBalance = {
                'type': 'Closing', 'description': '',
                'cbcredit': result.cbcredit,
                'cbdebit': result.cbdebit
            };
            
            $scope.ClosingBalance.credit = $scope.ClosingBalance.cbcredit > 0 ? $filter('IndianNumber')($scope.ClosingBalance.cbcredit, 2)+'Cr' : '';
            $scope.ClosingBalance.debit = $scope.ClosingBalance.cbdebit > 0 ? $filter('IndianNumber')($scope.ClosingBalance.cbdebit, 2)+'Dr' : '';

            //result.transactions.push({
            //    'type': 'Closing', 'description': '',
            //    'credit': result.cbcredit,
            //    'debit': result.cbdebit
            //});

            $scope.FinSummary = result;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            var column = igrid.AddCustomColumn('date', 'Date', 100, function (item, col) {
                if (item.date)
                    return $filter('date')(item.date, 'dd-MM-yyyy');
                return '';
            });

            igrid.AddSimpleColumn('type', 'Source', 100);
            igrid.AddSimpleColumn('description', 'Detail', 350);

            var column = igrid.AddCustomColumn('debit', 'Debit', 100, function (item, col) {
                if (item.debit && item.debit != null)
                    return $filter('IndianNumber')(item.debit, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.debit && item.debit != null)
                    return item.debit;
                return 0;
            }

            var column = igrid.AddCustomColumn('credit', 'Credit', 100, function (item, col) {
                if (item.credit && item.credit != null)
                    return $filter('IndianNumber')(item.credit, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.credit && item.credit != null)
                    return item.credit;
                return 0;
            }

            var column = igrid.AddCustomColumn('narration', 'Narration', 150, function (item, col) {
                if (item.detail && item.detail != null)
                    return item.detail.narration;
                return ''
            });

            if (result.bankid && result.bankid > 0) {
                var column = igrid.AddCustomColumn('voucherno', 'Vch. No', 100, function (item, col) {
                    if (item.detail && item.detail != null)
                        return item.detail.no;
                    return ''
                });
                var column = igrid.AddCustomColumn('bankreference', 'Chq No', 100, function (item, col) {
                    if (item.detail && item.detail != null)
                        return item.detail.bankreference;
                    return ''
                });
                var column = igrid.AddCustomColumn('bankdate', 'Chq Date', 100, function (item, col) {
                    if (item.detail && item.detail != null) {
                        var chequedate = $filter('date')(item.detail.bankdate, 'dd-MM-yyyy');
                        if (chequedate == '01-01-0001' || chequedate == '01-01-1901') return '';
                        return chequedate;
                    }
                    return ''
                });
                var column = igrid.AddCustomColumn('adjno', 'Adj. No', 100, function (item, col) {
                    if (item.detail && item.detail != null)
                        return item.detail.adjno;
                    return ''
                });
            }
            else {
                var column = igrid.AddCustomColumn('voucherno', 'Vch. No', 100, function (item, col) {
                    if (item.detail && item.detail != null)
                        return item.detail.no;
                    return ''
                });
                var column = igrid.AddCustomColumn('adjno', 'Adj. No', 100, function (item, col) {
                    if (item.detail && item.detail != null)
                        return item.detail.adjno;
                    return ''
                });
                var column = igrid.AddCustomColumn('invoiceno', 'Inv. No', 100, function (item, col) {
                    if (item.detail && item.detail != null)
                        return item.detail.invoiceno;
                    return ''
                });
                var column = igrid.AddCustomColumn('invoicedate', 'Inv. Date', 100, function (item, col) {
                    if (item.detail && item.detail != null) {
                        var invoicedate = $filter('date')(item.detail.invoicedate, 'dd-MM-yyyy');
                        if (invoicedate == '01-01-0001' || invoicedate == '01-01-1901') return '';
                        return invoicedate;
                    }
                    return ''
                });
            }

            
            var column = igrid.AddCustomColumn('createdby', 'Created By', 100, function (item, col) {
                if (item.cuser && item.cuser != null)
                    return item.cuser;

                if (item.detail && item.detail != null)
                    return item.detail.cuser;
                return ''
            });

            var column = igrid.AddCustomColumn('modifiedby', 'Modified By', 100, function (item, col) {
                if (item.muser && item.muser != null)
                    return item.muser;

                if (item.detail && item.detail != null)
                    return item.detail.muser;
                return ''
            });

            $scope.igrid = igrid;
            $scope.selectedcount = 0;
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Edit') {
                    $timeout(function () {
                        $state.go('voucher', { 'voucher': item.detail });
                    }, 10);
                }
            });
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.transactions, '_id');
        }

        $scope.igrid = null;
        $scope.BuildGrid = function (result) {

            $scope.FinGridData = null;
            $scope.GridData = result;


            $scope.DownloadURL = null;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);

            if ($scope.SearchCondition.reporttypeid == 0 ||
                $scope.SearchCondition.reporttypeid == 2) {
                var column = igrid.AddCustomColumn('transaction', 'Date', 100, function (item, col) {
                    return $filter('date')(item.transactionon, 'dd-MM-yyyy');
                });
                var column = igrid.AddCustomColumn('referenceno', 'Voucher No', 100, function (item, col) {
                    return item.referenceno;
                });
                igrid.AddSimpleColumn('referencetype', 'Source', 100);
                igrid.AddReferenceColumn('locationid', 'Branch', 200, $scope.Locations);
                igrid.AddSimpleColumn('createdby', 'Created By', 100);
            }

            igrid.AddSimpleColumn('displayname', 'Detail', 250);
            igrid.AddSimpleColumn('city', 'City', 100);

            if ($scope.SearchCondition.reporttypeid == 2) {
                igrid.AddSimpleColumn('age', 'Age', 80);
                igrid.AddSimpleColumn('referenceinvoiceno', 'Inv.No', 80); 
                var column = igrid.AddCustomColumn('referenceinvoiceno', 'Inv.Date', 100, function (item, col) {
                    var invdate = $filter('date')(item.referenceinvoicedate, 'dd-MM-yyyy');
                    if (invdate == '01-01-0001')
                        return '';
                    return invdate;
                });
            }

            if ($scope.SearchCondition.reporttypeid == 1 ||
                $scope.SearchCondition.reporttypeid == 3) {
                igrid.AddSimpleColumn('bills', 'Bills', 80).cssClass = 'rightalign';
            }

            if ($scope.SearchCondition.reporttypeid == 3) {
                var newresult = [];
                var agelist = [];
                $.each(result, function (k, item) {
                    var exist = newresult.find(function (kv) {
                        return kv.displayname == item.displayname &&
                            kv.city == item.city;
                    });
                    if (exist == undefined || exist.undefined) {
                        exist = {
                            'displayname': item.displayname,
                            'city': item.city,
                            'bills': 0,
                            'amount': 0,
                            'adjusted': 0,
                            'agelist': []
                        };
                        newresult.push(exist);
                    }
                    if (agelist.indexOf(item.age) < 0)
                        agelist.push(item.age);
                    exist.agelist.push(item);
                    exist.amount += item.amount;
                    exist.bills += item.bills;
                    exist.adjusted += item.adjusted;
                });

                result = newresult;
                $.each(agelist, function (k, v) {
                    var column = igrid.AddCustomColumn('age_' + k, v, 100, function (item, col) {
                        var ageval = item.agelist.find(function (kv) {
                            return kv.age == col.age;
                        });
                        if (ageval && ageval != null)
                            return $filter('IndianNumber')(ageval.amount - ageval.adjusted, 2);
                        return '';
                    });
                    column.age = v;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var ageval = item.agelist.find(function (kv) {
                            return kv.age == col.age;
                        });
                        if (ageval && ageval != null)
                            return ageval.amount - ageval.adjusted;
                        return 0;
                    }
                });
            }


            var column = igrid.AddCustomColumn('amount', 'Amount', 100, function (item, col) {
                return $filter('IndianNumber')(item.amount, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.amount;
            }
            var column = igrid.AddCustomColumn('adjusted', 'Paid', 100, function (item, col) {
                return $filter('IndianNumber')(item.adjusted, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.adjusted;
            }
            var column = igrid.AddCustomColumn('balance', 'Payable', 100, function (item, col) {
                return $filter('IndianNumber')(item.amount - item.adjusted, 2);
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.amount - item.adjusted;
            }

            $scope.igrid = igrid;
            $scope.selectedcount = 0;
            igrid.EnableTotal();
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result);
        }

        $scope.LocalContext = cntxService.CreatePageContext('legerpayable');
        $scope.$on(
            "$destroy",
            function (event) {
                $scope.LocalContext.Post('$scope.SearchCondition', $scope.SearchCondition, false);
                $scope.LocalContext.Post('$scope.FinGridData', $scope.FinGridData, false);
                $scope.LocalContext.Post('$scope.GridData', $scope.GridData, false);
                $scope.LocalContext.Post('$scope.DownloadURL', $scope.DownloadURL, false);
                
            }
        );

        $timeout(function () {

            $scope.SearchCondition = $scope.LocalContext.Get('$scope.SearchCondition', false);
            $scope.FinGridData = $scope.LocalContext.Get('$scope.FinGridData', false);
            $scope.GridData = $scope.LocalContext.Get('$scope.GridData', false);
            $scope.DownloadURL = $scope.LocalContext.Get('$scope.DownloadURL', false);

            if ($scope.FinGridData && $scope.FinGridData != null) {
                $scope.BuildFinGrid($scope.FinGridData);
            }
            else if ($scope.GridData && $scope.GridData != null) {
                $scope.BuildGrid($scope.GridData);
            }

        }, 100);

    }]);


