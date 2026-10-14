
var app = angular.module('InfinityLab');

app.controller('ClosingcashController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;
        $scope.IsPrint = false;
        $scope.system_cashclosewithtaken = false;
        $scope.system_canpostdayclose = false;
        $scope.Entity = {
            "id": 0,
            "paidbyemployeeid": "",
            "receivedbyemployeid": "",
            "counterid": "",
            "cashierid": 0,
            "paidon": "",
            "returnedon": new Date(),
            "counterid": "",
            "locationid": "",
            "openingamount": 0,
            "closingamount": 0,
            "expenses": 0,
            "amount": "",
            "closingdinomination": [{ dinomination: 2000, qty: 0, amount: 0 }, { dinomination: 500, qty: 0, amount: 0 }, { dinomination: 200, qty: 0, amount: 0 }, { dinomination: 100, qty: 0, amount: 0 }, { dinomination: 50, qty: 0, amount: 0 },
            { dinomination: 20, qty: 0, amount: 0 }, { dinomination: 10, qty: 0, amount: 0 }, { dinomination: 5, qty: 0, amount: 0 }, { dinomination: 2, qty: 0, amount: 0 }, { dinomination: 1, qty: 0, amount: 0 }],
            "takendinomination": [{ dinomination: 2000, qty: 0, amount: 0 }, { dinomination: 500, qty: 0, amount: 0 }, { dinomination: 200, qty: 0, amount: 0 }, { dinomination: 100, qty: 0, amount: 0 }, { dinomination: 50, qty: 0, amount: 0 },
            { dinomination: 20, qty: 0, amount: 0 }, { dinomination: 10, qty: 0, amount: 0 }, { dinomination: 5, qty: 0, amount: 0 }, { dinomination: 2, qty: 0, amount: 0 }, { dinomination: 1, qty: 0, amount: 0 }],
            "balancedinomination": [{ dinomination: 2000, qty: 0, amount: 0 }, { dinomination: 500, qty: 0, amount: 0 }, { dinomination: 200, qty: 0, amount: 0 }, { dinomination: 100, qty: 0, amount: 0 }, { dinomination: 50, qty: 0, amount: 0 },
            { dinomination: 20, qty: 0, amount: 0 }, { dinomination: 10, qty: 0, amount: 0 }, { dinomination: 5, qty: 0, amount: 0 }, { dinomination: 2, qty: 0, amount: 0 }, { dinomination: 1, qty: 0, amount: 0 }],
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "status": 2,
            "billno": ""
        };

        $scope.ClosingBalance = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        });

        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
        });
        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.ALLLocation = results;
        });

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.ErrorMessage = "";

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.ClosingBalance = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.Edited = function () {
            $scope.IsPrint = false;
        }

        $scope.AmountChanged = function (item) {
            var oldValue = item.amount;
            item.amount = isNaN($scope.GetNumber(item.dinomination) * $scope.GetNumber(item.qty)) ? 0 : $scope.GetNumber(item.dinomination) * $scope.GetNumber(item.qty);
            $scope.ClosingBalance.closingamount = $scope.GetNumber($scope.ClosingBalance.closingamount) - $scope.GetNumber(oldValue) + $scope.GetNumber(item.amount);
            $scope.IsPrint = false;
        };

        $scope.TakenAmountChanged = function (item) {
            var oldValue = item.amount;
            item.amount = isNaN($scope.GetNumber(item.dinomination) * $scope.GetNumber(item.qty)) ? 0 : $scope.GetNumber(item.dinomination) * $scope.GetNumber(item.qty);
            if ($scope.ClosingBalance.takenamount == undefined || $scope.ClosingBalance.takenamount == null)
                $scope.ClosingBalance.takenamount = 0;
            $scope.ClosingBalance.takenamount = $scope.GetNumber($scope.ClosingBalance.takenamount) - $scope.GetNumber(oldValue) + $scope.GetNumber(item.amount);
            $scope.IsPrint = false;
        };

        $scope.New = function () {
            $scope.ClosingBalance = (JSON.parse(JSON.stringify($scope.Entity)));

        };

        $scope.Save = function () {
            if (($scope.ClosingBalance.locationid == 0 || $scope.ClosingBalance.locationid == 'undefined' || $scope.ClosingBalance.locationid == null)) {
                $rootScope.Toaster.Error("Please select location");
                return;
            }
            if (($scope.ClosingBalance.counterid == 0 || $scope.ClosingBalance.counterid == 'undefined' || $scope.ClosingBalance.counterid == null)) {
                $rootScope.Toaster.Error("Please select cash counter");
                return;
            }
            if (($scope.ClosingBalance.cashierid == 0 || $scope.ClosingBalance.cashierid == 'undefined' || $scope.ClosingBalance.cashierid == null)) {
                $rootScope.Toaster.Error("Please select cashier");
                return;
            }
            if ($scope.ClosingBalance.id == null || $scope.ClosingBalance.id == 0) {
                $rootScope.Toaster.Error("Please Create Opening balance for the given cachier with counter");
                return;
            }

            $scope.ClosingBalance.status = 2;
            request = {
                "Item": $scope.ClosingBalance,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CashService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Saved Successfully.");
                    $scope.ClosingBalance = result;
                    $scope.IsPrint = true;
                    //$scope.ClosingBalance = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }

        $scope.SearchCondition = {
            receivedbyemployeid: 0,
            paidbyemployeeid: 0,
            paidon: new Date(),
            locationid: 0,
            counterid: 0
        }


        $scope.SearchOpening = function () {
            var request = {
                "Item": {
                    'paidon': $scope.ClosingBalance.returnedon,
                    'locationid': $scope.ClosingBalance.locationid,
                    'status': 9
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CashService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ShowOpenCounters(result);
            }).error(function (result) {
            });
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CashService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ShowResult(result);
            }).error(function (result) {
            });

        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                if (item.status == 1 && !$scope.UserInfo.p_CASHOPENING.modify) {
                    $rootScope.Toaster.Info("Don't have permission to delete.");
                    return;
                }
                if (item.status == 2 && !$scope.UserInfo.p_CASHCLOSING.modify) {
                    $rootScope.Toaster.Info("Don't have permission to delete.");
                    return;
                }

                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
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
                                    url: "CashService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
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
                if ($scope.UserInfo.p_CASHCLOSING.modify || item.closingamount == 0) {
                    $scope.Navigation.SetMode('EDIT');
                    $scope.ShowSearch = false;
                    $scope.ClosingBalance = item;
                    if ($rootScope.GetNumber($scope.ClosingBalance.closingamount) > 0)
                        $scope.IsPrint = true;

                    if ($scope.ClosingBalance.closingdinomination == null) {
                        $scope.ClosingBalance.closingdinomination = [{ dinomination: 2000, qty: 0, amount: 0 }, { dinomination: 500, qty: 0, amount: 0 }, { dinomination: 200, qty: 0, amount: 0 }, { dinomination: 100, qty: 0, amount: 0 }, { dinomination: 50, qty: 0, amount: 0 },
                        { dinomination: 20, qty: 0, amount: 0 }, { dinomination: 10, qty: 0, amount: 0 }, { dinomination: 5, qty: 0, amount: 0 }, { dinomination: 2, qty: 0, amount: 0 }, { dinomination: 1, qty: 0, amount: 0 }];

                        $scope.ClosingBalance.returnedon = $scope.ClosingBalance.paidon;
                    }
                    if ($scope.ClosingBalance.takendinomination == null) {
                        $scope.ClosingBalance.takendinomination = [{ dinomination: 2000, qty: 0, amount: 0 }, { dinomination: 500, qty: 0, amount: 0 }, { dinomination: 200, qty: 0, amount: 0 }, { dinomination: 100, qty: 0, amount: 0 }, { dinomination: 50, qty: 0, amount: 0 },
                        { dinomination: 20, qty: 0, amount: 0 }, { dinomination: 10, qty: 0, amount: 0 }, { dinomination: 5, qty: 0, amount: 0 }, { dinomination: 2, qty: 0, amount: 0 }, { dinomination: 1, qty: 0, amount: 0 }];

                    }

                }
                else {
                    $rootScope.Toaster.Info("You don't have permission to edit");
                }
            }
        }

        $scope.PrintDenomination = function () {
            //if ($scope.ClosingBalance.locationid == 0 || $scope.ClosingBalance.locationid == 'undefined' || $scope.ClosingBalance.locationid == null) {
            //    $rootScope.Toaster.Info("Please select location");
            //    return;
            //}
            if (($scope.ClosingBalance.counterid == 0 || $scope.ClosingBalance.counterid == 'undefined' || $scope.ClosingBalance.counterid == null)) {
                $rootScope.Toaster.Info("Please select cash counter");
                return;
            }
            if (($scope.ClosingBalance.cashierid == 0 || $scope.ClosingBalance.cashierid == 'undefined' || $scope.ClosingBalance.cashierid == null)) {
                $rootScope.Toaster.Info("Please select cashier");
                return;
            }

            var companydisplayname = '';
            $.each($scope.ALLLocation, function (k, v) {
                if (v.id == $scope.ClosingBalance.locationid && v.additionalinfo != null && v.additionalinfo.parentid > 0) {
                    companydisplayname = $scope.SearchRef(v.additionalinfo.parentid, $scope.CompanyList);
                }
            });

            var list = [];

            if ($rootScope.UserInfo.CompanyName == 'undefined' || $rootScope.UserInfo.CompanyName == null || $rootScope.UserInfo.CompanyName == '')
                list.push($rootScope.PrintFormater.Bold(companydisplayname));
            else
                list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
            list.push($rootScope.PrintFormater.Bold("DENOMINATION BREAKUP SLIP"));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Normal("  LOCATION : " + $scope.SearchRef($scope.ClosingBalance.locationid, $scope.Locations)));
            list.push($rootScope.PrintFormater.Normal("  DATE     : " + $filter('date')($scope.ClosingBalance.returnedon, 'dd-MM-yyyy')));
            list.push($rootScope.PrintFormater.Normal("  COUNTER  : " + $scope.SearchRef($scope.ClosingBalance.counterid, $scope.Counters)));
            list.push($rootScope.PrintFormater.Normal("  CASHIER  : " + $scope.SearchRef($scope.ClosingBalance.cashierid, $scope.Employee)));

            list.push($rootScope.PrintFormater.Normal("  DENOMINATION  : "));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));


            $.each($scope.ClosingBalance.closingdinomination, function (k, v) {
                var amount = $rootScope.GetNumber(v.dinomination) * $rootScope.GetNumber(v.qty);

                list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.dinomination, 10, true) + '   * '
                    + $rootScope.PrintFormater.Align(v.qty, 10, true)
                    + '   = ' + $rootScope.PrintFormater.Align($rootScope.GetNumber(amount), 14, true)));
            })
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Bold("  TOTAL          : " + $rootScope.PrintFormater.Align(parseFloat($scope.ClosingBalance.closingamount), 20, true)));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));

            list.push("\r\n");
            list.push($rootScope.PrintFormater.Cutter());
            list.push("\r\n");

            var printdocument = window.btoa(list.join('\r\n'));
            $rootScope.Sent2Printer(printdocument);

        }


        $scope.PrintSettlement = function (ledgertype) {
            if (($scope.ClosingBalance.locationid == 0 || $scope.ClosingBalance.locationid == 'undefined' || $scope.ClosingBalance.locationid == null)) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            if (($scope.ClosingBalance.counterid == 0 || $scope.ClosingBalance.counterid == 'undefined' || $scope.ClosingBalance.counterid == null)) {
                $rootScope.Toaster.Info("Please select cash counter");
                return;
            }
            if (($scope.ClosingBalance.cashierid == 0 || $scope.ClosingBalance.cashierid == 'undefined' || $scope.ClosingBalance.cashierid == null)) {
                $rootScope.Toaster.Info("Please select cashier");
                return;
            }

            if ($scope.system_cashclosewithtaken)
                ledgertype = true;

            $scope.SettlementResult = [];
            request = {
                "item": {
                    locationid: $scope.ClosingBalance.locationid,
                    counterid: $scope.ClosingBalance.counterid,
                    employeeid: $scope.ClosingBalance.cashierid,
                    fromdate: $scope.ClosingBalance.returnedon,
                    todate: $scope.ClosingBalance.returnedon,
                    cashid: $scope.ClosingBalance.id
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSettlementForReconcile",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    $scope.SettlementResult = result;
                    //if (ledgertype)
                    //    $scope.printreconcilationledger();
                    //else
                    $scope.printreconcilationreport();
                }
            }).error(function (result) {
            });
        }

        $scope.Post2Finiance = function () {
            var ids = [];
            $.each($scope.grid.GetSelectedRows(), function (k, v) {
                ids.push(v.id);
            });
            request = {
                "item": {
                    locationid: $scope.SearchCondition.locationid,
                    paidon: $scope.SearchCondition.paidon,
                    closings: ids,
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CashService/Post2Finance",
                data: srequest
            }).success(function (result) {
                if (result && result != null) {
                    var message = [];
                    message.push('Cash : ' + result.cash);
                    message.push('UPI : ' + result.upi + ' (' + result.upicount+')');
                    message.push('Bank : ' + result.bank + ' (' + result.bankcount + ')');
                    message.push('Total : ' + result.total);
                    $rootScope.ShowAlert('Finance Posting', message.join('<br>'));
                }
            }).error(function (result) {
            });
        }

        $scope.grid = null;
        $scope.ShowResult = function (results) {
            if ($scope.grid != null)
                $scope.grid.destroy();

            var grid = new InfyGrid('gridsearchresult', '450px', '100%', true, true, true, 10);
            grid.options.showHeaderRow = true;
            grid.AddReferenceColumn('locationid', 'Location', 100, $scope.Locations);
            grid.AddReferenceColumn('counterid', 'Counter', 100, $scope.Counters);
            grid.AddReferenceColumn('cashierid', 'Cashier', 100, $scope.Employee);
            grid.AddReferenceColumn('paidbyemployeeid', 'Paid By', 100, $scope.Employee);
            grid.AddReferenceColumn('receivedbyemployeid', 'Received By', 100, $scope.Employee);
            grid.AddFormatedColumn('paidon', 'Paid On', 120, 'dd/MM/yyyy');
            grid.AddFormatedColumn('returnedon', 'Received On', 120, 'dd/MM/yyyy');
            grid.AddFormatedColumn('openingamount', 'Opening', 60, 'N2', 2).total = true;

            if ($scope.UserInfo.p_CASHCLOSING.modify) {
                grid.AddFormatedColumn('takenamount', 'Taken', 60, 'N2', 2).total = true;
                grid.AddFormatedColumn('cardamount', 'Card', 60, 'N2', 2).total = true;
                grid.AddFormatedColumn('upiamount', 'UPI', 60, 'N2', 2).total = true;
                var column = grid.AddCustomColumn('total', 'Total', 100, function (item, col) {
                    var cash = (item.closingamount - item.openingamount);
                    return $filter('IndianNumber')(cash+item.cardamount+item.upiamount, 2);
                });
                column.total = true;
                column.cssClass = 'rightalign';
                column.getValue = function (item, col) {
                    var cash = (item.closingamount - item.openingamount);
                    return cash + item.cardamount + item.upiamount;
                }
                grid.AddFormatedColumn('closingamount', 'Closing', 80, 'N2', 2).total = true;
                var column = grid.AddCustomColumn('remain', 'Remaining', 80, function (item, col) {
                    var cash = (item.closingamount - item.takenamount);
                    return $filter('IndianNumber')(cash, 2);
                });
                column.total = true;
                column.cssClass = 'rightalign';
                column.getValue = function (item, col) {
                    var cash = (item.closingamount - item.takenamount);
                    return cash;
                }
            }

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            grid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Delete', args.item);
                });
            });

            $scope.selectedcount = 0;
            grid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            grid.EnableTotal();
            grid.Initialize();
            $scope.grid = grid;
            $scope.grid.UpdateData(results);
        }

        $scope.pgrid = null;

        $scope.ShowOpenCounters = function (results) {
            if ($scope.pgrid != null)
                $scope.pgrid.destroy();

            var pgrid = new InfyGrid('gridPending', '450px', '100%', true, false, true, 150);
            pgrid.options.showHeaderRow = true;
            pgrid.AddFormatedColumn('paidon', 'Date', 120, 'dd-MM-yyyy hh:mm a').total = true;
            pgrid.AddReferenceColumn('cashierid', 'Cashier', 200, $scope.Employee);
            pgrid.AddFormatedColumn('openingamount', 'Opening', 60, 'N2', 2).total = true;

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Closing'/> &nbsp;"
            pgrid.AddButtonColumn('action', '&nbsp;', 70, commands, function (e, args) {
                $scope.$apply(function () {
                    $scope.ManageSearchItem('Edit', args.item)
                });
            });



            pgrid.Initialize();
            $scope.pgrid = pgrid;
            $scope.pgrid.UpdateData(results);
        }

        $scope.LedgerLine_Credit = 0;
        $scope.LedgerLine_Debit = 0;
        $scope.AddLedgerline = function (description, debit, credit, compute) {
            if (credit != '') credit = credit.round(0);
            if (debit != '') debit = debit.round(0);
            if (compute == undefined || compute) {
                $scope.LedgerLine_Credit += credit == '' ? 0 : credit;
                $scope.LedgerLine_Debit += debit == '' ? 0 : debit;
            }
            if (description.length > 25)
                description = description.substring(0, 25);
            return $rootScope.PrintFormater.Align(description, 25) + " " +
                $rootScope.PrintFormater.Align(debit, 8, true) + ' ' +
                $rootScope.PrintFormater.Align(credit, 8, true);
        }

        $scope.printreconcilationledger = function () {
            var companydisplayname = '';
            $.each($scope.ALLLocation, function (k, v) {
                if (v.id == $scope.ClosingBalance.locationid && v.additionalinfo != null && v.additionalinfo.parentid > 0) {
                    companydisplayname = $scope.SearchRef(v.additionalinfo.parentid, $scope.CompanyList);
                }
            });

            var list = [];
            if ($scope.ClosingBalance.expenses == 'undefined' || $scope.ClosingBalance.expenses == null)
                $scope.ClosingBalance.expenses = 0;

            var total1 = ($rootScope.GetNumber($scope.SettlementResult[0].paid_cash) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_card) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_upi) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_credit) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_voucher) +
                $rootScope.GetNumber($scope.SettlementResult[0].adjustment_paid) +
                $rootScope.GetNumber($scope.SettlementResult[0].discount) +
                $rootScope.GetNumber($scope.SettlementResult[0].giftvoucherissued) +
                $rootScope.GetNumber($scope.SettlementResult[0].customerorderadvance));

            var total2 = ($rootScope.GetNumber($scope.ClosingBalance.expenses) +
                $rootScope.GetNumber($scope.SettlementResult[0].creditnoteissued) +
                $rootScope.GetNumber($scope.SettlementResult[0].return_cash));

            var total3 = total1 - total2;
            var total4 = total3 + $rootScope.GetNumber($scope.ClosingBalance.openingamount);

            var closingamt = ($rootScope.GetNumber($scope.ClosingBalance.openingamount) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_cash) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_card) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_upi) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_credit) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_voucher) +
                $rootScope.GetNumber($scope.SettlementResult[0].adjustment_paid) +
                $rootScope.GetNumber($scope.SettlementResult[0].discount) +
                $rootScope.GetNumber($scope.SettlementResult[0].advancepaid));

            var cashinhand = ($rootScope.GetNumber($scope.ClosingBalance.openingamount) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_cash)) -
                ($rootScope.GetNumber($scope.SettlementResult[0].return_cash) + $rootScope.GetNumber($scope.ClosingBalance.expenses));

            if ($rootScope.UserInfo.CompanyName == 'undefined' || $rootScope.UserInfo.CompanyName == null || $rootScope.UserInfo.CompanyName == '')
                list.push($rootScope.PrintFormater.Bold(companydisplayname));
            else
                list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));


            // Summary
            var total5 = ($rootScope.GetNumber($scope.ClosingBalance.openingamount) +
                $rootScope.GetNumber($scope.SettlementResult[0].salesvalue) +
                $rootScope.GetNumber($scope.SettlementResult[0].creditnoteissued));

            var total6 = ($rootScope.GetNumber($scope.SettlementResult[0].salesreturnvalue) +
                $rootScope.GetNumber($scope.SettlementResult[0].adjustment_paid) +
                $rootScope.GetNumber($scope.SettlementResult[0].discount) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_voucher) +
                $rootScope.GetNumber($scope.SettlementResult[0].return_cash) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_card) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_upi) +
                $rootScope.GetNumber($scope.ClosingBalance.expenses) +
                $rootScope.GetNumber($scope.SettlementResult[0].customerorderadvance) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_credit));

            var total7 = total5 - total6;

            list.push($rootScope.PrintFormater.Bold("SALES RECONCILATION SUMMARY "));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Normal("  LOCATION : " + $scope.SearchRef($scope.ClosingBalance.locationid, $scope.Locations)));
            list.push($rootScope.PrintFormater.Normal("  DATE     : " + $filter('date')($scope.ClosingBalance.returnedon, 'dd-MM-yyyy')));
            list.push($rootScope.PrintFormater.Normal("  COUNTER  : " + $scope.SearchRef($scope.ClosingBalance.counterid, $scope.Counters)));
            list.push($rootScope.PrintFormater.Normal("  CASHIER  : " + $scope.SettlementResult[0].employeename));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Normal("  Opening Balance             : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.openingamount, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Sales (Before Return)       : " + $rootScope.PrintFormater.Align($rootScope.GetNumber($scope.SettlementResult[0].salesvalue), 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Credit Note Issued          : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].creditnoteissued, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Advance Issued              : "));
            list.push($rootScope.PrintFormater.Normal("  Gift Voucher Issued         : "));
            list.push($rootScope.PrintFormater.Normal("  Credit Received             : "));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Bold("  TOTAL      (A) : " + $rootScope.PrintFormater.Align(total5.round(2), 10, true)));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Normal("  Sale Return                 : " + $rootScope.PrintFormater.Align($rootScope.GetNumber($scope.SettlementResult[0].salesreturnvalue), 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Adjustment Amount           : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].adjustment_paid, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Discount Amount             : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].discount, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Credit Note Received Payment: " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].paid_voucher, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Cash Refund Amount          : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].return_cash, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Card Payment                : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].paid_card, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  UPI Payment                 : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].paid_upi, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Petty Cash Expenses         : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.expenses, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Advance Amount              : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].customerorderadvance, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Gift Voucher Received       : "));
            list.push($rootScope.PrintFormater.Normal("  Credit Sale                 : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].paid_credit, 10, true)));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Bold("  TOTAL      (B) : " + $rootScope.PrintFormater.Align(total6.round(2), 10, true)));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Bold("  BALANCE C=A-B  : " + $rootScope.PrintFormater.Align(total7.round(2), 10, true)));
            list.push($rootScope.PrintFormater.Normal(" Closing Balance (D)         : " + $rootScope.PrintFormater.Align($rootScope.GetNumber($scope.ClosingBalance.closingamount).round(2), 10, true)));
            list.push($rootScope.PrintFormater.Bold("  DIFFERENCE (D-C): " + $rootScope.PrintFormater.Align(($rootScope.GetNumber($scope.ClosingBalance.closingamount) - total7).round(2), 10, true)));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            //

            var tline = [];
            var mline = [];
            var bline = [];
            $.each($scope.ClosingBalance.closingdinomination, function (k, v) {
                tline.push($rootScope.PrintFormater.Align(v.dinomination, 6, false))
                bline.push($rootScope.PrintFormater.Align(v.qty, 6, false))
                mline.push('_____');
            })
            list.push($rootScope.PrintFormater.Bold(" Closing Denomination : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.closingamount, 10, true)));
            list.push($rootScope.PrintFormater.Condensed(tline.join('')));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Condensed(bline.join('')));

            list.push($rootScope.PrintFormater.Normal("__________________________________________"));

            if ($scope.SettlementResult[0].lstcarddetails != null && $scope.SettlementResult[0].lstcarddetails.length > 0) {
                list.push($rootScope.PrintFormater.Bold(" Credit Card Collection Details : "));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                $.each($scope.SettlementResult[0].lstcarddetails, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.bankname, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                })
            }

            if ($scope.SettlementResult[0].lstupidetails != null && $scope.SettlementResult[0].lstupidetails.length > 0) {
                list.push($rootScope.PrintFormater.Bold(" UPI Collection Details  : "));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                $.each($scope.SettlementResult[0].lstupidetails, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.bankname, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                })
            }

            if ($scope.SettlementResult[0].creditnoteissued_list != null && $scope.SettlementResult[0].creditnoteissued_list.length > 0) {
                list.push($rootScope.PrintFormater.Bold(" Credit Note Issue : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].creditnoteissued, 10, true)));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                $.each($scope.SettlementResult[0].creditnoteissued_list, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.code, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                })
            }

            if ($scope.SettlementResult[0].giftvoucherissued_list != null && $scope.SettlementResult[0].giftvoucherissued_list.length > 0) {
                list.push($rootScope.PrintFormater.Bold(" Gift Voucher Issue : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].giftvoucherissued, 10, true)));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                $.each($scope.SettlementResult[0].giftvoucherissued_list, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.code, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                })
            }

            if ($scope.SettlementResult[0].customerorderadvance_list != null && $scope.SettlementResult[0].customerorderadvance_list.length > 0) {
                list.push($rootScope.PrintFormater.Bold(" Customer - Advance : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].customerorderadvance, 10, true)));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                $.each($scope.SettlementResult[0].customerorderadvance_list, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.code, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                })
            }

            if ($scope.SettlementResult[0].creditnotereceived_list != null && $scope.SettlementResult[0].creditnotereceived_list.length > 0) {
                list.push($rootScope.PrintFormater.Bold(" Credit Voucher Received : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].creditnotereceived, 10, true)));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                $.each($scope.SettlementResult[0].creditnotereceived_list, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.code, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                })
            }

            if ($scope.SettlementResult[0].lstexpensedetails != null && $scope.SettlementResult[0].lstexpensedetails.length > 0) {
                list.push($rootScope.PrintFormater.Bold(" Petti Cash Expense : "));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                var totalexpense = 0;
                $.each($scope.SettlementResult[0].lstexpensedetails, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.name, 22, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                    totalexpense += v.amount;
                });
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align('TOTAL', 22, false) + ' - ' + $rootScope.PrintFormater.Align(totalexpense, 10, true)));

                if (totalexpense != $scope.ClosingBalance.expenses) {
                    list.push($rootScope.PrintFormater.Normal("----------EXPENSE MISSMATCH-----------"));
                }
            }

            if ($scope.SettlementResult[0].lstexpensedetails != null && $scope.SettlementResult[0].lstcreditcollection.length > 0) {
                list.push($rootScope.PrintFormater.Bold(" Credit Payment Collection : "));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                var totalexpense = 0;
                var totalcash = 0;
                $.each($scope.SettlementResult[0].lstcreditcollection, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.name, 15, false) +
                        +' ' + v.mode + ' ' +
                        $rootScope.PrintFormater.Align(v.amount, 8, true)));
                    totalexpense += $rootScope.GetNumber(v.amount);
                    totalcash += $rootScope.GetNumber(v.cash);
                });
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align('TOTAL', 15, false) + ' - ' + $rootScope.PrintFormater.Align(totalexpense, 10, true)));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align('CASH', 15, false) + ' - ' + $rootScope.PrintFormater.Align(totalcash, 10, true)));

                var diff = parseFloat($scope.ClosingBalance.closingamount) - $rootScope.GetNumber(cashinhand);
                if (Math.abs(diff) < 2 && totalcash > 2) {
                    list.push($rootScope.PrintFormater.Normal("CREDIT COLLECTION EXCESS NOT IN HAND"));
                } else if (Math.abs(diff - totalcash) > 2) {
                    list.push($rootScope.PrintFormater.Normal("CREDIT PAYMENT MISSMATCH - " + (diff - totalcash).toString()));
                }
            }

            if ($scope.system_cashclosewithtaken) {
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Bold(" Taken Denomination  : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.takenamount, 10, true)));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));

                var tline = [];
                var mline = [];
                var bline = [];
                $.each($scope.ClosingBalance.takendinomination, function (k, v) {
                    tline.push($rootScope.PrintFormater.Align(v.dinomination, 6, false))
                    bline.push($rootScope.PrintFormater.Align(v.qty, 6, false))
                    mline.push('_____');
                })
                list.push($rootScope.PrintFormater.Condensed(tline.join('')));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Condensed(bline.join('')));

                list.push($rootScope.PrintFormater.Normal("__________________________________________"));

                if ($scope.ClosingBalance.balancedinomination &&
                    $scope.ClosingBalance.balancedinomination != null) {
                    // Balance denomination
                    list.push($rootScope.PrintFormater.Bold(" Balance Denomination : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.balanceamount, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    var tline = [];
                    var mline = [];
                    var bline = [];
                    $.each($scope.ClosingBalance.balancedinomination, function (k, v) {
                        tline.push($rootScope.PrintFormater.Align(v.dinomination, 6, false))
                        bline.push($rootScope.PrintFormater.Align(v.qty, 6, false))
                        mline.push('_____');
                    })
                    list.push($rootScope.PrintFormater.Condensed(tline.join('')));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Condensed(bline.join('')));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                }
            }

            list.push("\r\n");
            list.push($rootScope.PrintFormater.Cutter());
            list.push("\r\n");

            var printdocument = window.btoa(list.join('\r\n'));
            $rootScope.Sent2Printer(printdocument);
        }

        $scope.printreconcilationreport = function () {

            var companydisplayname = '';
            $.each($scope.ALLLocation, function (k, v) {
                if (v.id == $scope.ClosingBalance.locationid && v.additionalinfo != null && v.additionalinfo.parentid > 0) {
                    companydisplayname = $scope.SearchRef(v.additionalinfo.parentid, $scope.CompanyList);
                }
            });

            var list = [];
            if ($scope.ClosingBalance.expenses == 'undefined' || $scope.ClosingBalance.expenses == null)
                $scope.ClosingBalance.expenses = 0;

            var closingamt = ($rootScope.GetNumber($scope.ClosingBalance.openingamount) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_cash) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_card) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_upi));

            //+
            //$rootScope.GetNumber($scope.SettlementResult[0].advancepaid)

            var cashinhand = ($rootScope.GetNumber($scope.ClosingBalance.openingamount) +
                $rootScope.GetNumber($scope.SettlementResult[0].paid_cash)) -
                ($rootScope.GetNumber($scope.SettlementResult[0].return_cash) + $rootScope.GetNumber($scope.ClosingBalance.expenses));

            if ($rootScope.UserInfo.CompanyName == 'undefined' || $rootScope.UserInfo.CompanyName == null || $rootScope.UserInfo.CompanyName == '')
                list.push($rootScope.PrintFormater.Bold(companydisplayname));
            else
                list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
            list.push($rootScope.PrintFormater.Bold("SALES RECONCILATION REPORT"));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Normal("  LOCATION : " + $scope.SearchRef($scope.ClosingBalance.locationid, $scope.Locations)));
            list.push($rootScope.PrintFormater.Normal("  DATE     : " + $filter('date')($scope.ClosingBalance.returnedon, 'dd-MM-yyyy')));
            list.push($rootScope.PrintFormater.Normal("  COUNTER  : " + $scope.SearchRef($scope.ClosingBalance.counterid, $scope.Counters)));
            list.push($rootScope.PrintFormater.Normal("  CASHIER  : " + $scope.SettlementResult[0].employeename));

            list.push($rootScope.PrintFormater.Normal("  Closing Denomination  : "));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));

            var tline = [];
            var mline = [];
            var bline = [];
            $.each($scope.ClosingBalance.closingdinomination, function (k, v) {
                if ($rootScope.GetNumber(v.dinomination) <= 20) {
                    tline.push($rootScope.PrintFormater.Align(v.dinomination, 5, false))
                    bline.push($rootScope.PrintFormater.Align(v.qty, 5, false))
                    mline.push('_____');
                }
                else {
                    tline.push($rootScope.PrintFormater.Align(v.dinomination, 6, false))
                    bline.push($rootScope.PrintFormater.Align(v.qty, 6, false))
                    mline.push('_____');
                }
            })
            list.push($rootScope.PrintFormater.Condensed(tline.join('')));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Condensed(bline.join('')));

            list.push($rootScope.PrintFormater.Normal("__________________________________________"));

            list.push($rootScope.PrintFormater.Normal("  Collection Amout        (Z) : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.closingamount, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Opening Balance         (A) : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.openingamount, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Cash Payment            (B) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].paid_cash, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Card Payment            (C) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].paid_card, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  UPI Payment             (U) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].paid_upi, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Refund Amount           (D) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].return_cash, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Discount Amount         (E) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].discount, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Adjustment Amount       (F) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].adjustment_paid, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Advance Amount          (G) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].advancepaid, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Petty Cash Expenses     (X) : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.expenses, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Closing Balance(I=(A+B+C+U)): " + $rootScope.PrintFormater.Align(closingamt, 10, true)));
            list.push($rootScope.PrintFormater.Normal("  Cash In Hand(H=((A+B)-(D+X))): " + $rootScope.PrintFormater.Align(cashinhand, 9, true)));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Normal("  Diffrence             (Z-H) : " + $rootScope.PrintFormater.Align((parseFloat($scope.ClosingBalance.closingamount) - cashinhand).toString(), 10, true)));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            list.push($rootScope.PrintFormater.Normal(""));
            if ($scope.SettlementResult[0].lstcarddetails != null && $scope.SettlementResult[0].lstcarddetails.length > 0) {
                list.push($rootScope.PrintFormater.Normal("         CARD COLLECTION DETAILS  "));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                $.each($scope.SettlementResult[0].lstcarddetails, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align(v.bankname, 15, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                })
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            }
            if ($scope.SettlementResult[0].lstupidetails != null && $scope.SettlementResult[0].lstupidetails.length > 0) {
                list.push($rootScope.PrintFormater.Normal("         UPI COLLECTION DETAILS  "));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                $.each($scope.SettlementResult[0].lstupidetails, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align(v.bankname, 15, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                })
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            }

            if ($scope.SettlementResult[0].lstexpensedetails != null && $scope.SettlementResult[0].lstexpensedetails.length > 0) {
                list.push($rootScope.PrintFormater.Normal("             EXPENSE DETAILS  "));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                var totalexpense = 0;
                $.each($scope.SettlementResult[0].lstexpensedetails, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align(v.name, 15, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                    totalexpense += v.amount;
                });
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align('TOTAL', 15, false) + ' - ' + $rootScope.PrintFormater.Align(totalexpense, 10, true)));

                if (totalexpense != $scope.ClosingBalance.expenses) {
                    list.push($rootScope.PrintFormater.Normal("----------EXPENSE MISSMATCH-----------"));
                }
            }

            if ($scope.SettlementResult[0].lstexpensedetails != null && $scope.SettlementResult[0].lstcreditcollection.length > 0) {
                list.push($rootScope.PrintFormater.Normal("        CREDIT COLLECTION DETAILS  "));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                var totalexpense = 0;
                var totalcash = 0;
                $.each($scope.SettlementResult[0].lstcreditcollection, function (k, v) {
                    list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align(v.name, 15, false) +
                        +' ' + v.mode + ' ' +
                        $rootScope.PrintFormater.Align(v.amount, 8, true)));
                    totalexpense += $rootScope.GetNumber(v.amount);
                    totalcash += $rootScope.GetNumber(v.cash);
                });
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align('TOTAL', 15, false) + ' - ' + $rootScope.PrintFormater.Align(totalexpense, 10, true)));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Normal($rootScope.PrintFormater.Align('CASH', 15, false) + ' - ' + $rootScope.PrintFormater.Align(totalcash, 10, true)));

                var diff = parseFloat($scope.ClosingBalance.closingamount) - $rootScope.GetNumber(cashinhand);
                if (Math.abs(diff) < 2 && totalcash > 2) {
                    list.push($rootScope.PrintFormater.Normal("CREDIT COLLECTION EXCESS NOT IN HAND"));
                } else if (Math.abs(diff - totalcash) > 2) {
                    list.push($rootScope.PrintFormater.Normal("CREDIT PAYMENT MISSMATCH - " + (diff - totalcash).toString()));
                }
            }

            if ($scope.system_cashclosewithtaken) {

                if ($scope.SettlementResult[0].creditnoteissued_list != null && $scope.SettlementResult[0].creditnoteissued_list.length > 0) {
                    list.push($rootScope.PrintFormater.Bold(" Credit Note Issue : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].creditnoteissued, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    $.each($scope.SettlementResult[0].creditnoteissued_list, function (k, v) {
                        list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.code, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                    })
                }

                if ($scope.SettlementResult[0].giftvoucherissued_list != null && $scope.SettlementResult[0].giftvoucherissued_list.length > 0) {
                    list.push($rootScope.PrintFormater.Bold(" Gift Voucher Issue : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].giftvoucherissued, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    $.each($scope.SettlementResult[0].giftvoucherissued_list, function (k, v) {
                        list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.code, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                    })
                }

                if ($scope.SettlementResult[0].customerorderadvance_list != null && $scope.SettlementResult[0].customerorderadvance_list.length > 0) {
                    list.push($rootScope.PrintFormater.Bold(" Customer - Advance : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].customerorderadvance, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    $.each($scope.SettlementResult[0].customerorderadvance_list, function (k, v) {
                        list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.code, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                    })
                }

                if ($scope.SettlementResult[0].creditnotereceived_list != null && $scope.SettlementResult[0].creditnotereceived_list.length > 0) {
                    list.push($rootScope.PrintFormater.Bold(" Credit Voucher Received : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].creditnotereceived, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    $.each($scope.SettlementResult[0].creditnotereceived_list, function (k, v) {
                        list.push($rootScope.PrintFormater.Normal(" " + $rootScope.PrintFormater.Align(v.code, 20, false) + ' - ' + $rootScope.PrintFormater.Align(v.amount, 10, true)));
                    })
                }

                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Bold(" Taken Denomination  : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.takenamount, 10, true)));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));

                var tline = [];
                var mline = [];
                var bline = [];
                $.each($scope.ClosingBalance.takendinomination, function (k, v) {
                    if ($rootScope.GetNumber(v.dinomination) <= 20) {
                        tline.push($rootScope.PrintFormater.Align(v.dinomination, 5, false))
                        bline.push($rootScope.PrintFormater.Align(v.qty, 5, false))
                        mline.push('_____');
                    }
                    else {
                        tline.push($rootScope.PrintFormater.Align(v.dinomination, 6, false))
                        bline.push($rootScope.PrintFormater.Align(v.qty, 6, false))
                        mline.push('_____');
                    }
                })
                list.push($rootScope.PrintFormater.Condensed(tline.join('')));
                list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                list.push($rootScope.PrintFormater.Condensed(bline.join('')));

                list.push($rootScope.PrintFormater.Normal("__________________________________________"));

                if ($scope.ClosingBalance.balancedinomination &&
                    $scope.ClosingBalance.balancedinomination != null) {
                    // Balance denomination
                    list.push($rootScope.PrintFormater.Bold(" Balance Denomination : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.balanceamount, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    var tline = [];
                    var mline = [];
                    var bline = [];
                    $.each($scope.ClosingBalance.balancedinomination, function (k, v) {
                        if ($rootScope.GetNumber(v.dinomination) <= 20) {
                            tline.push($rootScope.PrintFormater.Align(v.dinomination, 5, false))
                            bline.push($rootScope.PrintFormater.Align(v.qty, 5, false))
                            mline.push('_____');
                        }
                        else {
                            tline.push($rootScope.PrintFormater.Align(v.dinomination, 6, false))
                            bline.push($rootScope.PrintFormater.Align(v.qty, 6, false))
                            mline.push('_____');
                        }
                    })
                    list.push($rootScope.PrintFormater.Condensed(tline.join('')));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Condensed(bline.join('')));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                }

            }

            list.push("\r\n");
            list.push($rootScope.PrintFormater.Cutter());
            list.push("\r\n");

            var printdocument = window.btoa(list.join('\r\n'));
            $rootScope.Sent2Printer(printdocument);
        }

        $scope.LoadAppSetting = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 50);
                return;
            }
            $scope.system_canpostdayclose = ($rootScope.UserInfo.p_CANPOSTDAYCLOSE == undefined || $rootScope.UserInfo.p_CANPOSTDAYCLOSE == null ? false : $rootScope.UserInfo.p_CANPOSTDAYCLOSE.view);
            $scope.system_cashclosewithtaken = $rootScope.UserInfo.appsettings.system_cashclosewithtaken || false;
            $scope.system_cashclosedenomination = $rootScope.UserInfo.appsettings.system_cashclosedenomination || false;
        }

        $timeout(function () {
            $scope.LoadAppSetting();
            var request = {
                "Item": {
                    'status': 10
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CashService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {
                    $scope.ManageSearchItem('Edit', result[0]);
                }
            }).error(function (result) {
            });
        }, 50);

    }]);