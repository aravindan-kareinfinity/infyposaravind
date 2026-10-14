var app = angular.module('InfinityLab');

app.controller('ChequeBoxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter', 'ngDialog', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter, ngDialog, localStorageService) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.ReportList = restService.Get('ConfigurationService/ReportList?id=CHEQUEPRINT', true).then(function (results) {
            $scope.ReportList = results;
        });

        $scope.StatusList = [{ 'id': 0, 'name': 'Progress' },
        { 'id': 1, 'name': 'Bounced' },
        { 'id': 2, 'name': 'Completed' },
        { 'id': 3, 'name': 'Cancelled' },
        { 'id': 4, 'name': 'Rejected' }];

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });

        $scope.SourceTypes = [{ 'id': 'SPMT', 'name': 'Supplier Payment' },
        { 'id': 'VPMT', 'name': 'Voucher Payment' },
        { 'id': 'TPMT', 'name': 'Transport Payment' },
        { 'id': 'APMT', 'name': 'Agent Payment' }];

        $scope.Entity = {
            id: 0,
            companyid: 0,
            locationid: 0,
            customerid: 0,
            sourcetype: "",
            employeeid: "",
            sourceid: "",
            bankid: "",
            chequedate: "",
            chequeno: "",
            amount: "",
            accountname: "",
            createdon: "",
            modifiedon: "",
            status: "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1
        }
        $scope.ChequeBox = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ChequeBox.sourcetype = "Direct";

        $scope.SearchResult = [];
        $scope.totalamount = 0;
        $scope.paidamt = 0;

        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";
        $scope.ShowSearch = false;

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.ChequeBox = (JSON.parse(JSON.stringify($scope.Entity)));

        }

        $scope.SearchCondition = {
            accountname: "",
            fromdate: "",
            todate: "",
            status: ""
        }



        $scope.Save = function () {
            var ismodify = $scope.ChequeBox.id && $scope.ChequeBox.id > 0;
            request = {
                "Item": $scope.ChequeBox,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BankchequetransactionService/Save",
                data: srequest
            }).success(function (result) {

                if (ismodify) {
                    $scope.igrid.UpdateData($scope.SearchResult);
                }
                else {
                    $scope.SearchResult.push(result);
                    $scope.igrid.UpdateData($scope.SearchResult);
                }
                $rootScope.Toaster.Info("Cheque Details Saved Successfully.!");
                $scope.ChequeBox = (JSON.parse(JSON.stringify($scope.Entity)));

            }).error(function (result) {
            });

        }

        $scope.SearchChequeList = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BankchequetransactionService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ChequeListConfig();
            }).error(function (result) {
            });
        }

        $scope.ManageList = function (item, status) {
            item.status = status;
            request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BankchequetransactionService/Update",
                data: srequest
            }).success(function (result) {
                $scope.ChequeListConfig();
            }).error(function (result) {
            });
            //$location.url('/ChequeBox?id=' + item.id);
            //$scope.ShowSearch = true;
            //$scope.ChequeBox = item;
        };

        $scope.Print = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                retList.push(v.id);
            });
            if (retList.length > 0) {
                $rootScope.Handlers.SelectPrinter('CHEQUEPRINT', function (printdata) {
                    if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                        return;


                    request = {
                        "Item": { 'ids': retList, 'reportid': printdata.report.id },
                        "securitytoken": "String content"
                    }

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "BankchequetransactionService/Print",
                        data: srequest
                    }).success(function (printdocument) {
                        $rootScope.Sent2Printer(printdocument, printdata, 'PDF');
                    }).error(function (result) {
                    });
                });
            }
        }

        $scope.Download = function () {
            var seletedRows = this.igrid.GetSelectedRows();

            var data = [];
            var company = $scope.Company.find(function (item) {
                return item.id == seletedRows[0].companyid;
            });

            var bank = $scope.Banks.find(function (item) {
                return item.id == seletedRows[0].bankid;
            });
            var meregelist = [];
            meregelist.push({ s: { r: 0, c: 0 }, e: { r: 0, c: 6 } });
            meregelist.push({ s: { r: 1, c: 0 }, e: { r: 1, c: 6 } });

            data.push([company.name]);
            if (bank.additionalinfo &&
                bank.additionalinfo.bankinfo &&
                bank.additionalinfo.bankinfo != null) {
                data.push(['ACCOUNT NAME', bank.additionalinfo.bankinfo.accountname, '', '', 'ACCOUNT NO', bank.additionalinfo.bankinfo.accountno]);
            }
            else {
                data.push([bank.name]);
            }
            data.push(['CHEQUE NO', seletedRows[0].chequeno, '', '', 'CHEQUE DATE',
                $filter('date')(seletedRows[0].chequedate, 'dd-MM-yyyy')]);

            data.push([
                'LEDGER',
                'ACCOUNT NAME',
                'ACCOUNT NO',
                'ACCOUNT IFSC',
                'BRANCH',
                'AMOUNT'
            ]);

            var total = 0;
            $.each(seletedRows, function (k, v) {
                data.push([
                    v.ledgername,
                    v.accountname,
                    v.bankaccountno,
                    v.bankifsc,
                    v.bankbranch,
                    v.amount
                ]);
                total += v.amount;
            });

            data.push([
                '',
                '',
                '',
                '',
                '',
                total
            ]);

            if (data.length > 0) {
                var ws = XLSX.utils.aoa_to_sheet(data);
                var wb = XLSX.utils.book_new();

                ws['!merges'] = meregelist;

                ws['!cols'] = [{ width: 50 }, { width: 40 }, { width: 30 }, { width: 50 }, { width: 50 }, { width: 30 }];
                var title = {									// set the style for target cell
                    font: {
                        name: 'verdana',
                        sz: 14,
                        bold: true,
                        color: { rgb: "33220000" }
                    },
                    alignment: {
                        vertical: "center",
                        horizontal: "center"
                    }
                };

                var Stitle = {									// set the style for target cell
                    font: {
                        name: 'verdana',
                        sz: 12,
                        bold: true,
                        color: { rgb: "33220000" }
                    },
                    alignment: {
                        vertical: "center",
                        horizontal: "left"
                    }
                };

                var Ctitle = {									// set the style for target cell
                    font: {
                        name: 'verdana',
                        sz: 12,
                        bold: true,
                        color: { rgb: "33220000" }
                    },
                    alignment: {
                        vertical: "center",
                        horizontal: "center"
                    }
                };

                ws["A1"].s = title;
                ws["A2"].s = title;

                ws["A3"].s = Stitle;
                ws["B3"].s = Stitle;
                ws["C3"].s = Stitle;
                ws["D3"].s = Stitle;
                ws["E3"].s = Stitle;
                ws["F3"].s = Stitle;

                ws["A4"].s = Ctitle;
                ws["B4"].s = Ctitle;
                ws["C4"].s = Ctitle;
                ws["D4"].s = Ctitle;
                ws["E4"].s = Ctitle;
                ws["F4"].s = Ctitle;

                //ws["A1:F2"].s = {									// set the style for target cell
                //    border: {
                //        bottom: {
                //            style: 'thin', color: { auto: 1 }
                //        }
                //    },
                //};

                XLSX.utils.book_append_sheet(wb, ws, "sheet1");
                /* generate file and download */
                const wbout = XLSX.write(wb, { type: "array", bookType: "xlsx" });
                saveAs(new Blob([wbout], { type: "application/octet-stream" }), "download.xlsx");
            }
        }

        $scope.ChequeSummary = function () {
            var idlist = [];
            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                idlist.push(v.id);
            });
            $rootScope.PostDownload('BankchequetransactionService/ChequeSummaryPDF', { 'item': { 'ids': idlist } },
                'summary.pdf', 'application/pdf');
        }

        $scope.ChequePDFDownload = function (issummary) {
            var idlist = [];
            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                idlist.push(v.id);
            });
            if (idlist.length > 0) {
                if (issummary) {
                    $rootScope.PostDownload('BankchequetransactionService/ChequeSummaryPDF', { 'item': { 'ids': idlist } },
                        'summary.pdf', 'application/pdf');
                }
                else {


                    $rootScope.Handlers.SelectPrinter('CHEQUEPRINT', function (printdata) {
                        if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                            return;

                        $rootScope.PostDownload('BankchequetransactionService/ChequePDF', { 'Item': { 'ids': idlist, 'reportid': printdata.report.id } },
                            'summary.pdf', 'application/pdf');

                    });
                }
            }
        }



        $scope.ChangeStatus = function (status) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                retList.push(v.id);
            });

            var request = {
                "Item": { 'ids': retList, 'status': status },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BankchequetransactionService/ChangeStatus",
                data: srequest
            }).success(function (result) {

                var seletedRows = this.igrid.GetSelectedRows();
                $.each(seletedRows, function (k, v) {
                    v.status = status;
                });

                $scope.ChequeListConfig();
            }).error(function (result) {
            });
        }

        $scope.ChequeListConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 5);
            igrid.options.showHeaderRow = true;

            if ($scope.SearchCondition.companyid == undefined || $scope.SearchCondition.companyid == null)
                igrid.AddReferenceColumn('companyid', 'COMPANY', 150, $scope.Company);
            if ($scope.SearchCondition.locationid == undefined || $scope.SearchCondition.locationid == null)
                igrid.AddReferenceColumn('locationid', 'LOCATION', 150, $scope.Location);
            if ($scope.SearchCondition.bankid == undefined || $scope.SearchCondition.bankid == null)
                igrid.AddReferenceColumn('bankid', 'BANK NAME', 150, $scope.Banks);
            igrid.AddSimpleColumn('ledgername', 'LEDGER', 150);
            igrid.AddSimpleColumn('sourcetype', 'TYPE', 100);
            igrid.AddSimpleColumn('referenceno', 'REFERENCE', 100);

            igrid.AddFormatedColumn('chequedate', 'CHEQUE DATE', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('chequeno', 'CHEQUE NO', 100);

            igrid.AddSimpleColumn('accountname', 'ACCOUNT NAME', 150);
            igrid.AddSimpleColumn('bankaccountno', 'ACCOUNT NO', 100);
            igrid.AddSimpleColumn('bankifsc', 'ACCOUNT IFSC', 100);
            igrid.AddSimpleColumn('bankbranch', 'ACCOUNT BRANCH', 100);

            igrid.AddFormatedColumn('amount', 'PAYABLE', 100, 'N2', 2).total = true;
            igrid.AddReferenceColumn('status', 'STATUS', 150, $scope.StatusList);

            var commands = "<input type='button' class='btn btn-secondary btn-xs' value='Edit'/>"
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Rejected'/>"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Cancelled'/>"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Bounced'/>"
            igrid.AddButtonColumn('action', 'ACTION', 250, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit') {
                        $scope.ChequeBox = args.item;
                        $scope.ShowSearch = true;
                    }
                    if ($(e.target).val() == 'Cancelled')
                        $scope.ManageList(args.item, 3);
                    else if ($(e.target).val() == 'Rejected')
                        $scope.ManageList(args.item, 4);
                    else if ($(e.target).val() == 'Bounced')
                        $scope.ManageList(args.item, 1);
                });
            });



            //igrid.AddMultiButtonColumn('action', 'ACTION', 80, ['Rejected', 'Cancelled'], function (e, args) {
            //    $scope.$apply(function () {
            //        if ($(e.target).val() == 'Cancelled')
            //            $scope.ManageList(args.item, 1);
            //        else if ($(e.target).val() == 'Rejected') 
            //            $scope.ManageList(args.item, 2);
            //    });
            //});

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.EnableTotal();
            igrid.Initialize();
            //igrid.grid.onClick.subscribe(function (e) {
            //    var cell = igrid.grid.getCellFromEvent(e);
            //    var item = igrid.grid.getDataItem(cell.row);
            //    var command = igrid.grid.getColumns()[cell.cell].id;
            //    if (command == 'Edit') {
            //        $scope.ChequeBox = item;
            //        $scope.ShowSearch = false;
            //        $scope.$digest();
            //    }
            //    return true;
            //});
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }
        $scope.selectedcount = 0;
        //$timeout(function () {
        //    $scope.setFocusfirst();
        //}, 10);


    }]);

