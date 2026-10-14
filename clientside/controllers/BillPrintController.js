
var app = angular.module('InfinityLab');

app.controller('BillPrintController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter', 'localStorageService', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter, localStorageService, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.selectedcount = 0;
        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
            $scope.CustomerCategory = results;
        });
        $scope.Counter = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counter = results;
        });

        $scope.RetailCustomers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.RetailCustomers = results;
        });

        $scope.SearchResult = [];
        $scope.SearchCondition = {
            billno: "",
            billdate: "",
            customername: "",
            mobileno: ""
        }
        $scope.UpdateBill = {
            customerid: 0,
            mobileno: "",
            customername: "",
            gstno: "",
            billdate: ""
        }

        $scope.Search = function (id) {
            if (!($scope.SearchCondition.billno || $scope.SearchCondition.fromdate || $scope.SearchCondition.cardno || $scope.SearchCondition.mobileno || $scope.SearchCondition.customername)) {
                $rootScope.Toaster.Info("Please enter one minimum condition");
                return;
            }
            if (!($scope.SearchCondition.billno != null || $scope.SearchCondition.fromdate != null || $scope.SearchCondition.cardno != null || $scope.SearchCondition.mobileno != null || $scope.SearchCondition.customername != null)) {
                $rootScope.Toaster.Info("Please enter one minimum condition");
                return;
            }

            if (!($scope.SearchCondition.billno != '' || new Date($scope.SearchCondition.fromdate).getFullYear() > 2000
                || $scope.SearchCondition.cardno != '' || $scope.SearchCondition.mobileno != '' || $scope.SearchCondition.customername != '')) {
                $rootScope.Toaster.Info("Please enter one minimum condition");
                return;
            }


            $scope.SearchResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/SearchCustomerBills",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.GridConfig();
            }).error(function (result) {
            });
        }

        $scope.Sent2Printer = function (printdocument) {

            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }

            $rootScope.Sent2Printer(printdocument, printerData);
        }

        $scope.PrintThermalBill = function (item) {

            var request = {
                "Item": {
                    'id': typeof (item.billid) != 'undefined' ? item.billid : item.id,
                    'enableprintdocument': true,
                    'billtype': item.billtype,
                    'isbulkreturn': item.billtype == 10
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {

                if (result != null && result.length == 1) {
                    $scope.Sent2Printer(result[0].printdocument);
                }
            });
        };

        $scope.PrintBill = function (item) {
            $rootScope.Download('BillService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id + "/false", 'Invoice No-' + item.billno + '.pdf', 'application/pdf');
            return;
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "bill.pdf";
                link.href = 'BillService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id + "/false";
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }
        $scope.PrintCustomBill = function (item) {
            $rootScope.Download('BillService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id + "/false/true", 'Invoice No-' + item.billno + '.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "bill.pdf";
                link.href = 'BillService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id + "/false/true";
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'SALES LIST');
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.GridConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.Location);
            igrid.AddReferenceColumn('counterid', 'Counter', 80, $scope.Counter);
            igrid.AddSimpleColumn('customername', 'Customer Name', 100);
            igrid.AddSimpleColumn('mobileno', 'Mobile no', 80);
            igrid.AddSimpleColumn('gstno', 'GST NO', 80);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 60, function (item, col) {
                return item.isinterstatesale == true ? "YES" : "NO";
            });
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 85, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('billvalue', 'Bill Value', 80, 'N2', 2).total = true;

            var commands = "";
            if ($rootScope.UserInfo.p_BILLPRINT.view)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Print Pdf'/> &nbsp;"
            if ($rootScope.UserInfo.p_BILLPRINT.view)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Print Thermal'/>  &nbsp;"
            if ($rootScope.UserInfo.p_BILLPRINT.view)
                commands += "<input type='button' class='btn btn-info btn-xs' value='Custom Pdf'/>  &nbsp;"

            if ($rootScope.UserInfo.p_BILLPRINT.view) {
                igrid.AddButtonColumn('action', 'ACTION', 280, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Print Pdf')
                            $scope.PrintBill(args.item);
                        else if ($(e.target).val() == 'Custom Pdf')
                            $scope.PrintCustomBill(args.item);
                        else if ($(e.target).val() == 'Print Thermal')
                            $scope.PrintThermalBill(args.item);
                    });
                });

            }
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                    $scope.$apply();
                }, 10);
            };

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.ShowUpdateDialog = function () {

            var dialog = ngDialog.open({
                template: 'UpdateBill',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.UpdateCustomer = function () {
                        $scope.closeThisDialog($scope.parent);
                    };
                    $scope.UpdateBillDate = function () {
                        $scope.parent.UpdateBillDate();
                        $scope.closeThisDialog(null);
                    };
                    $scope.SearchCustomer = function () {
                        $scope.parent.SearchCustomer();
                    };
                    $scope.UpdateIGST = function () {
                        $scope.parent.UpdateIGST();
                        $scope.closeThisDialog(null);
                    };
                    $scope.UpdateHSN = function () {
                        $scope.parent.UpdateHSN();
                        $scope.closeThisDialog(null);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.UpdateCustomer();
                    }
                }
            });
        }
        $scope.UpdateCustomer = function () {
            if ($scope.UpdateBill.customerid == 'undefined' || $scope.UpdateBill.customerid == null
                || $scope.UpdateBill.customerid == 0) {
                $rootScope.Toaster.Error("Please select customer.");
                return;
            }

            var seletedRows = this.igrid.GetSelectedRows();
            var ids = [];
            $.each(seletedRows, function (k, v) {
                ids.push(v.id);
            });
            var webrequest = {
                "item": {
                    'customerid': $scope.UpdateBill.customerid, 'ids': ids
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "BillService/UpdateCustomer",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Customer Change done successfully.");
            }).error(function (result) {
            });
        }

        $scope.UpdateBillDate = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            var ids = [];
            $.each(seletedRows, function (k, v) {
                ids.push(v.id);
            });
            var webrequest = {
                "item": {
                    'billdate': $scope.UpdateBill.billdate, 'ids': ids
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "BillService/UpdateBillDate",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Bill Date Change done successfully.");
            }).error(function (result) {
            });
        }
        $scope.UpdateIGST = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            var ids = [];
            $.each(seletedRows, function (k, v) {
                ids.push(v.id);
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Error("Please select bill.");
                return;
            }
            var webrequest = {
                "item": {
                    'billdate': $scope.UpdateBill.billdate, 'ids': ids
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "BillService/UpdateIGST",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Bill updation done successfully.");
            }).error(function (result) {
            });
        }
        $scope.UpdateHSN = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            var ids = [];
            $.each(seletedRows, function (k, v) {
                ids.push(v.id);
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Error("Please select bill.");
                return;
            }
            var webrequest = {
                "item": {
                    'billdate': $scope.UpdateBill.billdate, 'ids': ids
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "BillService/UpdateBillHSN",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Bill HSN updation completed successfully.");
            }).error(function (result) {
            });
        }

        $scope.SearchCustomer = function () {
            if ($scope.UpdateBill.mobileno == null || $scope.UpdateBill.mobileno == "") {
                return;
            }
            restService.Get('CustomerService/PhoneNumber?id=' + $scope.UpdateBill.mobileno, true).then(function (results) {
                $scope.UpdateBill.customername = results.name;
                $scope.UpdateBill.customerid = results.id;
                $scope.UpdateBill.gstno = results.gstno;
                if (results.id == 'undefined' || results.id == null || results.id == "") {
                    $rootScope.Toaster.Error("Can't find Customer, please refine your search.");
                    return;
                }
            });
        }

        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);