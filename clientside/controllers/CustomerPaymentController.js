
var app = angular.module('InfinityLab');

app.controller('CustomerPaymentController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'localStorageService', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, localStorageService, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.LastPaymentID = 0;
        $scope.SaveRequest = false;

        $scope.SwippingList = restService.Get('ReferencelistService/FULL-SWIPPINGMACINE', true).then(function (results) {
            $scope.SwippingList = results;
        })

        $scope.UPIProviderList = restService.Get('ReferencelistService/FULL-UPIPROVIDER', true).then(function (results) {
            $scope.UPIProviderList = results;
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
            $rootScope.BindCompany($scope, 'SearchPaymentCondition.companyid');
        });
        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });
        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });
        $scope.PaymentModes = [{ 'id': 0, 'name': 'Card' },
        { 'id': 2, 'name': 'Cash' },
        { 'id': 3, 'name': 'UPI' },
        { 'id': 4, 'name': 'Cheque/DD' },
        { 'id': 5, 'name': 'NEFT' },
        { 'id': 9, 'name': 'Discount' }];

        $scope.CollectedAt = [{ 'id': 0, 'name': 'Accounts Department' },
        { 'id': 1, 'name': 'Settlement Counter' },
        { 'id': 2, 'name': 'Collection Agent' }];

        $scope.system_loansettlement = false;
        $scope.system_swippingmachine = false;
        $scope.system_salecollectcarddata = false;
        $scope.HasLocationSelected = false;
        $scope.FinanceEnabled = false;
        $scope.ApplicationSettings = function () {
            if (!$scope.HasLocationSelected) {
                $rootScope.Triggers.SelectLocation('', function () {
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                        typeof ($rootScope.LocationInfo.location) != 'undefined');
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.location) != 'undefined')
                    $scope.ShowLocationInfo();
                    $scope.ApplicationSettings();
                });
                return;
            }

            $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.location) != 'undefined');
            var srequest = JSON.stringify({
                "Item": { 'locationid': $rootScope.LocationInfo.location.id, 'key': 'APPLICATIONSETTINGS' },
                "securitytoken": "String content"
            });
            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.ApplicationSettings = result[0];
                    $scope.system_loansettlement = $scope.ApplicationSettings.appsettings.system_loansettlement;
                    $scope.system_swippingmachine = $scope.ApplicationSettings.appsettings.system_swippingmachine;
                    $scope.system_salecollectcarddata = $scope.ApplicationSettings.appsettings.system_salecollectcarddata;
                    $scope.FinanceEnabled = $scope.ApplicationSettings.appsettings.int_finance;
                    if ($scope.FinanceEnabled && $scope.Ledgers == null) {
                        $scope.Ledgers = restService.Get('ReferencelistService/LEDGERS/6_11_10_5', true).then(function (results) {
                            $scope.Ledgers = results;
                        });
                    }
                }
                else {
                    $rootScope.Toaster.Error("Please save the application setting for the current location");
                }
            }).error(function (result) {
                $rootScope.Toaster.Error("Please save the application setting for the current location");
            });
        };
        $scope.ApplicationSettings();


        $scope.LoanProviderList = restService.Get('ReferencelistService/LOANPROVIDER', true).then(function (results) {
            $scope.LoanProviderList = results;
        });

        $scope.SwippingList = restService.Get('ReferencelistService/SWIPPINGMACINE', true).then(function (results) {
            $scope.SwippingList = results;
        });

        $scope.CardTypeList = restService.Get('ReferencelistService/CARDTYPES', true).then(function (results) {
            $scope.CardTypeList = results;
        });

        $scope.CustomerList = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.CustomerList = results;
        });

        $scope.OnCustomerSelection = function()
        {
            var customer = $scope.CustomerList.find(function (item) {
                return item.id == $scope.SearchPaymentCondition.customerid;
            })
            if (customer && customer != null) {
                $scope.SearchPaymentCondition.customerid = customer.id;
                $scope.SearchPaymentCondition.customername = customer.name;
                $scope.SearchCondition.customerid = customer.id;
                $scope.SearchCondition.customername = customer.name;
            }
            else {
                $scope.SearchPaymentCondition.customername = null;
                $scope.SearchCondition.customerid = null;
                $scope.SearchCondition.customername = null;
            }
        }

        $scope.EmployeeList = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-', 'code': '-' });
            }
            $scope.EmployeeList = results;
        });

        $scope.BankList = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-' });
            }
            else {
                var printerData = localStorageService.get('LocationData');
                if (printerData != null) {
                    var locationid = printerData.location.id;
                    var newlist = [];
                    $.each(results, function (k, v) {
                        if (v.locationid == 0 || v.locationid == locationid)
                            newlist.push(v);
                    });
                    results = newlist;
                }
            }
            $scope.BankList = results;
        });

        $scope.Entity = {
            "id": 0,
            "customerid": 0,
            "paidon": new Date(),
            "receivedby": 0,
            "amount": 0.0,
            "receiptno": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": null,
            "billpaid": 0,
            "billdiscount": 0,
            "paymentinfo": {},
            "Customerinvoice": []
        };
        $scope.CustomerPayment = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ShowLocationInfo = function () {
            $scope.CounterName = ($scope.HasLocationSelected) ? $scope.SearchRef($rootScope.LocationInfo.counter.id, $scope.Counters) : 'Select Counter';
            $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
        }

        $scope.Locations = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Locations = results;
            $rootScope.BindLocation($scope, 'SearchPaymentCondition.locationid');
            if (!$scope.HasLocationSelected) {
                $scope.LocationName = 'Select Location';
                return;
            }
            $scope.ShowLocationInfo();
        });


        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
            $scope.CounterName = ($scope.HasLocationSelected) ? $scope.SearchRef($rootScope.LocationInfo.counter.id, results) : 'Select Counter';
        });

        $scope.ShowInvoice = true;
        $scope.ShowPayment = false;
        $scope.ShowSearch = false;
        $scope.ShowPendingBills = true;
        $scope.showSearchResult = false;

        $scope.SearchResult = [];
        $scope.LoadPendinginvoice = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.CustomerPayment = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.BacktoLoadPayament = function () {
            $scope.ShowInvoice = true;
            $scope.ShowPayment = false;
        }

        $scope.New = function () {
            $scope.TotalPending = 0;
            $scope.AddedAmount = 0;
            $scope.TDSAmount = 0;
            $scope.DiscountedAmount = 0;
            $scope.RoundingAmount = 0;
            $scope.CustomerPayment = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchResult = [];
            $scope.LoadPendinginvoice = [];
            if ($scope.igrid && $scope.igrid!=null)
                $scope.igrid.UpdateData([]);
        }

        $scope.SearchPaymentCondition = {
            customerid: 0,
            showpending: true
        }

        $scope.AddedAmount = 0;
        $scope.TaxAmount = 0;
        $scope.TDSAmount = 0;
        $scope.DiscountedAmount = 0;
        $scope.TotalPending = 0;
        $scope.RoundingAmount = 0;
        $scope.ShowCustomerDialog = function () {


            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.PopupList = [];
                    $scope.Customer = null;
                    $scope.CloseIfOne = false;

                    $scope.Select = function () {
                        $scope.Customer = null;
                        $.each($scope.PopupList, function (k, v) {
                            if (v.selected)
                                $scope.Customer = v;
                        });
                        if ($scope.Customer != null)
                            $scope.closeThisDialog($scope.Customer);
                    };

                    $scope.Create = function () {
                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };

                    $scope.hasvalue = function (val) {
                        return typeof (val) != 'undefined' && val != null &&
                            val != '' && val.length > 0;
                    }

                    $scope.Search = function (closeIfOne) {

                        $scope.CloseIfOne = typeof (closeIfOne) != 'undefined' ? closeIfOne : false;
                        if (!$scope.hasvalue($scope.name) &&
                            !$scope.hasvalue($scope.area) &&
                            !$scope.hasvalue($scope.contactno) &&
                            !$scope.hasvalue($scope.gstno) &&
                            !$scope.hasvalue($scope.loyeltyno))
                            return;

                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Search",
                            data: srequest
                        }).success(function (result) {
                            if (result.length == 1 && $scope.CloseIfOne) {
                                $scope.closeThisDialog(result[0]);
                            }
                            $scope.PopupList = result;
                        });
                    };

                    $timeout(function () {
                        $scope.setFocuscustomer_name();
                    }, 10);

                    $scope.contactno = $scope.ngDialogData[0].SearchPaymentCondition.customername;
                    if (typeof ($scope.contactno) != 'undefined' && $scope.contactno != null) {
                        if ($scope.contactno.length != 10 || $scope.ngDialogData[0].GetNumber($scope.contactno) <= 0) {
                            $scope.name = $scope.contactno;
                            $scope.contactno = '';
                        }
                        $scope.Search(true);
                    }

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {
                        $scope.SearchPaymentCondition.customerid = data.value.id;
                        $scope.SearchPaymentCondition.customername = data.value.name;
                        $scope.SearchCondition.customerid = data.value.id;
                        $scope.SearchCondition.customername = data.value.name;
                        //$scope.SearchItems();
                    }
                }
            });
        }

        $scope.UPIChanged = function () {
            var provider = $scope.UPIProviderList.find(function (item) {
                return item.id == $scope.CustomerPayment.paymentinfo.upiproviderid;
            });
            if (provider && provider != null) {
                $scope.CustomerPayment.paymentinfo.bankid = provider.additionalinfo.parentid;
            }
        }

        $scope.MachineChanged = function () {
            var provider = $scope.SwippingList.find(function (item) {
                return item.id == $scope.CustomerPayment.paymentinfo.swippingid;
            });
            if (provider && provider != null) {
                $scope.CustomerPayment.paymentinfo.bankid = provider.additionalinfo.parentid;
            }
        }

        $scope.BillBased = false;
        $scope.SearchItems = function () {
            $scope.TotalPending = 0;
            $scope.CustomerPayment.Customerinvoice = [];
            $scope.CustomerPayment.LoadPendinginvoice = [];
            $scope.CustomerPayment.billdiscount = 0;
            $scope.CustomerPayment.billpaid = 0;
            $scope.SearchPaymentCondition.showpending = true;
            var isbillbased = true;
            if (typeof ($scope.SearchPaymentCondition.BillNo) == 'undefined' ||
                $scope.SearchPaymentCondition.BillNo == null ||
                $scope.SearchPaymentCondition.BillNo == '') {
                isbillbased = false;
                if ($scope.SearchPaymentCondition.customerid == 0) {
                    $rootScope.Toaster.Info("Please select customer for payment");
                    return;
                }
            }

            request = {
                "item": $scope.SearchPaymentCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length == 0) {
                    $rootScope.Toaster.Info("There is no pending payment for the given customer");
                    return;
                }
                else {
                    $scope.BillBased = isbillbased;
                    $scope.SearchCondition.showSearchResult = true;
                    $scope.LoadPendinginvoice = result;
                    $scope.TotalPending = 0;
                    $.each($scope.LoadPendinginvoice, function (k, v) {
                        $scope.TotalPending += (v.receivable - v.paid - v.discount);
                        v.paidcustomerinvoiceid = v.id;
                        v.id = 0;
                        if (isbillbased) {
                            v.fullvalue = true;
                            v.settlementamount = (v.receivable - v.paid - $scope.GetNumber(v.discountamount));
                        }
                    });
                    if (isbillbased) {
                        $scope.CustomerPayment.collectedat = 1;
                        $scope.CustomerPayment.paidon = new Date();
                        $scope.CustomerPayment.paymentinfo = {
                            'paidamount': $scope.TotalPending,
                            'paymentmode': 2
                        };
                        $scope.SearchPaymentCondition.customername = $scope.LoadPendinginvoice[0].customername;
                        $scope.SearchPaymentCondition.customerid = $scope.LoadPendinginvoice[0].customerid;
                        $scope.SearchPaymentCondition.companyid = $scope.LoadPendinginvoice[0].companyid;
                        $scope.SearchCondition.customerid = $scope.LoadPendinginvoice[0].customerid;
                        $scope.SearchCondition.customername = $scope.LoadPendinginvoice[0].customername;
                        $scope.AddedAmountChanged();
                    }
                    $scope.ShowPendingInvoice();
                }
            }).error(function (result) {
            });
        }

        $scope.ShowPendingInvoice = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('pendingContainer', '400px', '100%', true, false, true, 40);
            igrid.EnableSerial($scope.LoadPendinginvoice);

            igrid.AddSimpleColumn('billno', 'Bill No', 80);

            igrid.AddFormatedColumn('billdate', 'Bill Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receivable', 'Billed', 80, 'N2', 2);
            igrid.AddFormatedColumn('paid', 'Paid', 80, 'N2', 2);
            igrid.AddFormatedColumn('discount', 'Discnt', 80, 'N2', 2);
            igrid.AddCustomColumn('edit', '<input type="checkbox" style="z-index: auto;margin-left:5px">', 40, function (item, col) {
                if (item.fullvalue)
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px" checked>';
                else
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px">';
            });
            var column = igrid.AddCustomColumn('Balance', 'Balance', 80, function (item, col) {
                return $filter('IndianNumber')(item.receivable - (item.paid + item.discount), 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.receivable - (item.paid + item.discount);
            }
            column = igrid.AddColumn(
                {
                    id: "settlementamount", name: "Amount", field: "settlementamount",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, decimal: 2
                }
            );
            column.cssClass = 'rightalign';
            column.total = true;
            column = igrid.AddColumn(
                {
                    id: "discountamount", name: "Discount", field: "discountamount",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, decimal: 2
                }
            );
            column.cssClass = 'rightalign';
            column.total = true;

            var column = igrid.AddCustomColumn('newbalance', 'New Balance', 100, function (item, col) {
                return $filter('IndianNumber')(item.receivable - (item.paid + item.discount) - $scope.GetNumber(item.settlementamount) - $scope.GetNumber(item.discountamount), 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.decimal = 2;
            column.getValue = function (item, col) {
                return item.receivable - (item.paid + item.discount) - $scope.GetNumber(item.settlementamount) - $scope.GetNumber(item.discountamount);
            }

            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData().getItem(row);
                rowdata.style = rowdata.receivable - rowdata.paid < 0 ? 'igridbigrowswithmark' : '';
                return { 'cssClasses': '' };
            };
            $scope.igrid = igrid;

            igrid.grid.onHeaderClick.subscribe(function (e, args) {

                if (args.column.id == 'edit' && $(e.target).is(":checkbox")) {
                    if (igrid.grid.getEditorLock().isActive() && !igrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }

                    if ($(e.target).is(":checked")) {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.fullvalue = true;
                            item.settlementamount = (item.receivable - item.paid - $scope.GetNumber(item.discountamount));
                        }

                    } else {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.fullvalue = false;
                            item.settlementamount = 0;
                        }
                    }
                    igrid.UpdateAllTotals(igrid.grid);
                    igrid.grid.invalidate();
                    igrid.grid.render();
                    $timeout(function () {
                        $scope.AddedAmountChanged();
                    }, 10);
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                }

            });

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'edit') {
                    item = args.grid.getDataItem(args.row);
                    item.fullvalue = !item.fullvalue && $(e.target).val() == 'on';

                    if (item.fullvalue) {
                        item.settlementamount = (item.receivable - item.paid - $scope.GetNumber(item.discountamount));
                    }
                    else {
                        item.settlementamount = 0;
                    }

                    $timeout(function () {
                        $scope.AddedAmountChanged();
                        igrid.UpdateAllTotals(igrid.grid);
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);

                }
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'settlementamount' ||
                    args.grid.getColumns()[args.cell].id == 'discountamount') {
                    if (args.grid.getColumns()[args.cell].id == 'discountamount' &&
                        item.discountamount.toString().endsWith('%')) {
                        var dppercent = item.discountamount.substr(0, item.discountamount.length - 1);
                        var discount = Math.round(item.receivable * parseFloat(dppercent) / 100);
                        if (!isNaN(discount) && discount != 0) {
                            item.discountamount = discount;
                            if (parseFloat(item.settlementamount) > item.receivable - discount)
                                item.settlementamount = item.receivable - discount;
                        }
                    }
                    $timeout(function () {
                        $scope.AddedAmountChanged();
                        igrid.UpdateAllTotals(igrid.grid);
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);

                }
            });

            $scope.igrid.UpdateData($scope.LoadPendinginvoice);

        }

        $scope.tdsvaluechanged = function () {
            $scope.TDSAmount = $scope.GetNumber($scope.CustomerPayment.tdsamount);
        }

        $scope.tdspercentagechanged = function () {
            if ($scope.CustomerPayment.hastds) {
                var tdspercentage = parseFloat($scope.CustomerPayment.tdspercentage);
                if (isNaN(tdspercentage)) tdspercentage = 0;
                if (tdspercentage != 0) {
                    if ($scope.CustomerPayment.tdsexclusive) {
                        $scope.TDSAmount = Math.round(($scope.AddedAmount - $scope.TaxAmount) * tdspercentage / 100)
                    }
                    else {
                        $scope.TDSAmount = Math.round(($scope.AddedAmount - $scope.DiscountedAmount) * tdspercentage / 100);
                    }
                }
                else {
                    $scope.TDSAmount = 0;
                }
                $scope.CustomerPayment.tdsamount = $scope.TDSAmount;
            }
            else {
                $scope.TDSAmount = 0;
                $scope.CustomerPayment.tdsamount = $scope.TDSAmount;
            }
        }

        $scope.PrintPDF = function (id) {
            $rootScope.Download('CustomerpaymentService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.LastPaymentID : id), 'payment.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "payment.pdf";
                link.href = 'CustomerpaymentService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.LastPaymentID : id);
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.PrintSettilement = function (item) {

            request = {
                "Item": { 'id': item },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);


            $http({
                method: 'POST',
                url: "CustomerpaymentService/Print",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0 && result.printdocument != null) {
                    $scope.Sent2Printer(result.printdocument);
                }


            }).error(function (result) {
                $scope.ErrorMessage = result;
            });
        }

        $scope.Sent2Printer = function (printdocument, printdata) {
            if (printdocument == null)
                return;

            var printerData = typeof (printdata) == 'undefined' ? localStorageService.get('PrinterData') : printdata;
            if (printerData == null || typeof (printerData) == 'undefined') {
                $rootScope.Toaster.Error("Please select the printer");
                return;
            }

            $rootScope.Sent2Printer(printdocument, printerData);

            //var request = {
            //    'id': printerData.printerid,
            //    'content': printdocument,
            //    'encoded': 'BASE64'
            //};
            //var srequest = JSON.stringify(request);
            //$http({
            //    method: 'POST',
            //    url: printerData.service,
            //    data: srequest
            //}).success(function (result) {

            //});
        }


        $scope.Apply = function () {
            if ($scope.CustomerPayment.amount > 0) {
                var payamount = $scope.CustomerPayment.amount;

                $.each($scope.LoadPendinginvoice, function (k, v) {
                    var balanceamt = v.receivable - (v.paid - v.discount);
                    if (balanceamt > payamount) {
                        v.settlementamount = payamount;
                        return;
                    }
                    else {
                        v.settlementamount = balanceamt;
                        payamount -= balanceamt;
                        if (payamount <= 0)
                            return;
                    }
                });
            }
        }
        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };

        $scope.AddedAmountChanged = function () {
            $scope.AddedAmount = 0;
            $scope.TaxAmount = 0;
            $scope.DiscountedAmount = 0;
            $scope.TDSAmount = 0;
            $.each($scope.LoadPendinginvoice, function (k, v) {

                if (v.receivable - v.paid > 0) {
                    if ($scope.GetNumber(v.settlementamount) < 0) {
                        v.settlementamount = 0;
                    }

                    if ($scope.GetNumber(v.discountamount) < 0) {
                        v.discountamount = 0;
                    }

                    if (v.settlementamount > (v.receivable - v.paid - $scope.GetNumber(v.discountamount))) {
                        v.settlementamount = (v.receivable - v.paid - $scope.GetNumber(v.discountamount));
                    }
                }
                else if ($scope.GetNumber(v.settlementamount) > 0 || $scope.GetNumber(v.discountamount)) {
                    v.settlementamount = 0;
                    v.discountamount = 0;
                }
                $scope.TaxAmount += v.tax;
                $scope.AddedAmount += $scope.GetNumber(v.settlementamount);
                $scope.DiscountedAmount += $scope.GetNumber(v.discountamount);
            });
            $scope.tdspercentagechanged();
            $scope.RoundingAmount = ($scope.AddedAmount - $scope.TDSAmount) - Math.round($scope.AddedAmount - $scope.TDSAmount);
        }

        $scope.SavePayament = function () {
            var haserror = false;
            $scope.CustomerPayment.Customerinvoice = [];
            $.each($scope.LoadPendinginvoice, function (k, v) {
                if ($scope.GetNumber(v.settlementamount) != 0 ||
                    $scope.GetNumber(v.discountamount) != 0) {

                    var existing = $scope.CustomerPayment.Customerinvoice.find(function (item) {
                        return item.billid == v.billid && item.paidcustomerinvoiceid == v.id;
                    });

                    //if (typeof (existing) != 'undefined' && existing != null) {
                    //    $scope.CustomerPayment.Customerinvoice.splice($scope.CustomerPayment.Customerinvoice.indexOf(existing), 1);
                    //}

                    if ($scope.CustomerPayment.Customerinvoice.length == 0) {
                        $scope.CustomerPayment.companyid = v.companyid;
                        $scope.CustomerPayment.customerid = v.customerid;
                        $scope.CustomerPayment.locationid = $rootScope.LocationInfo.location.id;
                        $scope.CustomerPayment.counterid = $rootScope.LocationInfo.counter.id;
                    }

                    if ($scope.CustomerPayment.companyid != v.companyid) {
                        $rootScope.Toaster.Error(v.billno + " - Can't collect payment for different companies pending by single receipt!");
                        haserror = true;
                    }
                    else if ($scope.CustomerPayment.customerid != v.customerid) {
                        $rootScope.Toaster.Error(v.billno + " - Can't collect payment for different persons pending by single receipt!");
                        haserror = true;
                    }
                    else {
                        $scope.CustomerPayment.Customerinvoice.push({
                            'type': 4,
                            'paid': v.settlementamount,
                            'billid': v.billid,
                            'paidcustomerinvoiceid': v.paidcustomerinvoiceid,
                            'customerid': v.customerid,
                            'billno': v.billno,
                            'companyid': v.companyid,
                            'discount': v.discountamount,
                            'transactiondate': new Date()
                        });
                    }
                }
            });
            return haserror;
        }

        $scope.Save = function () {

            if ($scope.SavePayament()) return;

            if ($scope.CustomerPayment == null || $scope.CustomerPayment.length == 0) {
                $rootScope.Toaster.Info("Please Add Invoice details for Payment.");
                return;
            }

            $scope.CustomerPayment.billdiscount = 0;
            $scope.CustomerPayment.billpaid = 0;
            $scope.CustomerPayment.rounding = $scope.RoundingAmount;
            $.each($scope.CustomerPayment.Customerinvoice, function (k, v) {
                $scope.CustomerPayment.billdiscount += $scope.GetNumber(v.discount);
                $scope.CustomerPayment.billpaid += $scope.GetNumber(v.paid);
            })

            $scope.CustomerPayment.amount = $scope.GetNumber($scope.CustomerPayment.paymentinfo.paidamount);

            if ($scope.GetNumber($scope.CustomerPayment.amount) <= 0) {
                if ($scope.GetNumber($scope.CustomerPayment.amount) + $scope.GetNumber($scope.RoundingAmount) != $scope.GetNumber($scope.AddedAmount - $scope.TDSAmount)) {
                    $rootScope.Toaster.Info("Please enter the amount for Payment.");
                    return;
                }
            }

            if ($scope.GetNumber($scope.CustomerPayment.amount) + $scope.GetNumber($scope.RoundingAmount) != $scope.GetNumber($scope.AddedAmount - $scope.TDSAmount)) {
                $rootScope.Toaster.Info("Entered amount differes with allocated invoices.");
                return;
            }

            $scope.CustomerPayment.paymentinfo.payableamount = $scope.TotalPending;
            request = {
                "item": $scope.CustomerPayment,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;

            $http({
                method: 'POST',
                url: "CustomerpaymentService/Save",
                data: srequest
            }).success(function (result) {

                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {

                    $scope.LastPaymentID = result.id;
                    $scope.LastPaymentNo = result.receiptno;
                    $scope.PrintSettilement($scope.LastPaymentID);
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.New();
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
            });

        }
        $scope.SearchCondition = {
            customerid: 0,
            showpending: ""
        }

        $scope.SearchByReceiptNo = function () {
            if ($scope.CustomerPayment.receiptno && $scope.CustomerPayment.receiptno != null &&
                $scope.CustomerPayment.receiptno != '') {
                request = {
                    "item": { 'receiptno': $scope.CustomerPayment.receiptno },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "CustomerpaymentService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result && result.length  && result.length == 1) {
                        $scope.ManageSearchItem('Edit', result[0]);
                    }
                }).error(function (result) {
                });
            }
        }
        
        $scope.Search = function () {
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerpaymentService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.CustomerPaymentConfig();
            }).error(function (result) {
            });

        }

        $scope.CustomerPaymentConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddReferenceColumn('companyid', 'COMPANY NAME', 100, $scope.BuyingCompany);
            igrid.AddSimpleColumn('customername', 'CUSTOMER', 200);
            igrid.AddSimpleColumn('receiptno', 'RECEIPT NO', 80);
            igrid.AddFormatedColumn('paidon', 'RECEIPT DATE', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('paymentmode', 'PAYMENT MODE', 80);
            igrid.AddFormatedColumn('amount', 'AMOUNT', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tdsamount', 'TDS', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'DISCOUNT', 80, 'N2', 2).total = true;
            igrid.AddSimpleColumn('billnos', 'BILL NO', 120);
            igrid.AddSimpleColumn('receivedbyname', 'RECEIVED BY', 120);
            igrid.AddSimpleColumn('remarks', 'REMARKS', 150);

            if ($rootScope.UserInfo.p_CUSTOMERPAYMENT.modify || $rootScope.UserInfo.p_CUSTOMERPAYMENT.delete) {
                var commands = "";
                if ($rootScope.UserInfo.p_CUSTOMERPAYMENT.modify)
                    commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
                if ($rootScope.UserInfo.p_CUSTOMERPAYMENT.delete)
                    commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
                if ($rootScope.UserInfo.p_CUSTOMERPAYMENT.print) {
                    commands += "<input type='button' class='btn btn-secondary btn-xs' value='Print'/> &nbsp;"
                    commands += "<input type='button' class='btn btn-info btn-xs' value='Print PDF'/>"
                }

                igrid.AddButtonColumn('action', 'ACTION', 210, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Print')
                            $scope.PrintSettilement(args.item.id);
                        else if ($(e.target).val() == 'Print PDF')
                            $scope.PrintPDF(args.item.id);
                    });
                });
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.PaidChanged = function () {
            //$scope.CustomerPayment.amount = $scope.GetNumber($scope.CustomerPayment.paymentinfo.cash_paid) + $scope.GetNumber($scope.CustomerPayment.paymentinfo.card_paid);
            $scope.CustomerPayment.amount = $scope.GetNumber($scope.CustomerPayment.paymentinfo.paidamount);
        }

        $scope.ManageSearchItem = function (action, item) {

            if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.receiptno + "?",
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
                    buttons: {
                        formSubmit: {
                            text: 'Submit',
                            btnClass: 'btn-blue',
                            action: function () {
                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "CustomerPaymentService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.igrid.UpdateData($scope.SearchResult);
                                    }

                                }).error(function (result) {

                                });
                            }
                        },
                        cancel: function () {
                            //close
                        },
                    },

                    onContentReady: function () {
                        // bind to events
                        var jc = this;
                        jc.$$formSubmit.trigger('focus');
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });


            }
            else if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.Navigation.SetMode('EDIT');
                $scope.New();
                request = {
                    "Item": { 'id': item.id },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "CustomerPaymentService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.CustomerPayment = result[0];
                    $scope.LastPaymentID = $scope.CustomerPayment.id;
                    $scope.SearchPaymentCondition.customername = $scope.CustomerPayment.customername;
                    $scope.SearchPaymentCondition.customerid = $scope.CustomerPayment.customerid;
                    $scope.SearchCondition.customerid = $scope.CustomerPayment.customerid;
                    $scope.SearchCondition.customername = $scope.CustomerPayment.customername;
                    $scope.SearchPaymentCondition.companyid = $scope.CustomerPayment.companyid;
                    $scope.LoadPendinginvoice = [];
                    $.each($scope.CustomerPayment.Customerinvoice, function (k, v) {
                        v.settlementamount = v.paid;
                        v.discountamount = v.discount;
                        v.paid = 0;
                        v.fullvalue = true;
                        $scope.LoadPendinginvoice.push(v);
                    });
                    $scope.ShowPendingInvoice();
                }).error(function (result) {

                });

            }
        }


        $scope.CashChanged = function (cash) {

            var value = 0;
            if (typeof (cash) == 'string' && cash.length > 0) {
                try {
                    value = eval(cash);

                    if (typeof (value) == 'undefined' || value == null || typeof (value) != 'number' || isNaN(value)) {
                        vaule = 0;
                        $scope.CustomerPayment.paymentinfo.paidamount = '';
                        //$scope.CustomerPayment.paymentinfo.cash_dinamination = '';
                        //$scope.CustomerPayment.paymentinfo.cash_return = 0;
                    }
                } catch (exp) {
                }
            }


            $scope.CustomerPayment.paymentinfo.paidamount = value >= 0 ? value : '';
            $scope.PaidChanged();
        }



        $scope.checkbalanceandgo = function () {
            $scope.PaidChanged();

            if (nextfocus == 'cardpaid' && $scope.system_salecollectcarddata && $scope.Settlement.paymentinfo.card_paid > 0) {

                if (typeof ($scope.Settlement.paymentinfo.card_no) == 'undefined' || $scope.Settlement.paymentinfo.card_no == null || $scope.Settlement.paymentinfo.card_no.length != 4) {
                    nextfocus = 'cardno';
                    $scope['setFocus' + nextfocus]();
                    return;
                }
            }
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'CUSTOMER PAYMENT - INFORMATION'
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

        $scope.checkmachineandbank = function () {
            if ($scope.system_swippingmachine) {
                $scope['setFocuscardmachineid']();
            }
            else {
                $scope['setFocusbankid']();
            }
        }

    }]);

