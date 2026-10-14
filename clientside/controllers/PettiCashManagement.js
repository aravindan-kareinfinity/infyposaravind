
var app = angular.module('InfinityLab');

app.controller('PettiCashManagementController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter) {
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = false;
        $scope.AllowEnter = false;
        $scope.SearchResult = [];

        $scope.CashLedgers = restService.Get('ReferencelistService/LEDGERS/13', true).then(function (results) {
            $scope.CashLedgers = results;
            if (results.length == 1) {
                $scope.SearchCondition.ledgerid = results[0].id;
            }
        });

        $scope.AllLedgers = restService.Get('ReferencelistService/LEDGERS/17_18_21_22_7_6_14_13_11', true).then(function (results) {
            var filterledgers = [];
            $.each(results, function (k, v) {
                if (v.typeid == 6 || v.typeid == 13) return;
                if (v.typeid == 14 || v.tagid > 0) return;
                    filterledgers.push(v);
            })
            $scope.AllLedgers = filterledgers;
        });

        $scope.ExpenseTypes = restService.Get('ReferencelistService/LEDGERS/17_18_21_22', true).then(function (results) {
            $scope.ExpenseTypes = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
            $rootScope.BindLocation($scope, 'SearchCondition.locationid');
        });

        $scope.Counter = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counter = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'SearchCondition.companyid');
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Entity = {
            "id": 0,
            opening: "",
            closing: "",
            ebreading: "",
            bankdeposit: "",
            banktransaction: "",
            expenses: "",
            expensestransaction: "",
            cashin: "",
            cashintransaction: "",
            cashout: "",
            cashouttransaction: "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": ""
        };

        $scope.PettyCash = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            name: "",
            contactperson: "",
            yesterdayopening: 0,
            opening: 0,
            closing: 0,
            expense: 0,
            cashwithdraw: 0,
            cashdeposit: 0
        }

        $scope.Clear = function () {
            $scope.PettyCash = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.ShowLedgerCreation = function () {
            var dialog = ngDialog.open({
                template: 'clientside/views/ledger.html?d=' + (new Date()).getTime(),
                data: [$scope],
                width: '750px',
                controller: 'ledgerController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;

                    if (data.value.typeid == 13) {
                        $scope.CashLedgers.push(data.value);
                    }
                    else if (data.value.typeid == 17 || data.value.typeid == 18) {
                        $scope.ExpenseTypes.push(data.value);
                        $scope.BuildGrid();
                    }

                }
            });
        }

        $scope.LastUUID = '';
        $scope.CreateUID = function () {
            var newuuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'
                .replace(/[xy]/g, function (c) {
                    const r = Math.random() * 16 | 0,
                        v = c == 'x' ? r : (r & 0x3 | 0x8);
                    return v.toString(16);
                });

            if (newuuid == $scope.LastUUID)
                return $scope.CreateUID();

            $scope.LastUUID = newuuid;
            return $scope.LastUUID;
        }

        $scope.SavePettyCash = function () {
            if (typeof ($scope.Voucher) == 'undefined' || $scope.Voucher == null) {
                $scope.Voucher = { 'voucheritems': [] };
            }
            if ($scope.Voucher.uniqueuid == undefined || $scope.Voucher.uniqueuid == null)
                $scope.Voucher.uniqueuid = $scope.CreateUID();

            $scope.Voucher.adjustmentledgerid = $scope.SearchCondition.ledgerid;
            $scope.Voucher.locationid = $scope.SearchCondition.locationid;
            $scope.Voucher.employeeid = $scope.SearchCondition.employeeid;
            $scope.Voucher.companyid = $scope.SearchCondition.companyid;
            if ($scope.Voucher.attributes == undefined || $scope.Voucher.attributes == null)
                $scope.Voucher.attributes = {};
            $scope.Voucher.attributes.counterid = $scope.SearchCondition.counterid;
            $scope.Voucher.voucherdate = $scope.SearchCondition.voucherdate;
            $scope.Voucher.sourceid = 1;
            $scope.Voucher.amount = 0;
            $scope.Voucher.adjustmentcredit = true;

            var removeitems = [];
            if (!$scope.finance_pettycashvoucher) {
                $scope.Voucher.voucheritems = [];
                $.each($scope.VoucherData, function (k, v) {
                    if (v.typeid == 21 || v.typeid == 2) {
                        $scope.Voucher.amount -= v.total;
                    }
                    else {
                        $scope.Voucher.amount += v.total;
                    }
                    $scope.AddVoucherItem(v.ledgerid, v.value_1, v.existingvalue, 'value_1');
                    $scope.AddVoucherItem(v.ledgerid, v.value_2, v.existingvalue, 'value_2');
                    $scope.AddVoucherItem(v.ledgerid, v.value_3, v.existingvalue, 'value_3');
                    $scope.AddVoucherItem(v.ledgerid, v.value_4, v.existingvalue, 'value_4');
                    $scope.AddVoucherItem(v.ledgerid, v.value_5, v.existingvalue, 'value_5');
                    $scope.AddVoucherItem(v.ledgerid, v.value_6, v.existingvalue, 'value_6');
                    $scope.AddVoucherItem(v.ledgerid, v.value_7, v.existingvalue, 'value_7');
                    $scope.AddVoucherItem(v.ledgerid, v.value_8, v.existingvalue, 'value_8');
                    $scope.AddVoucherItem(v.ledgerid, v.value_9, v.existingvalue, 'value_9');
                    $scope.AddVoucherItem(v.ledgerid, v.value_10, v.existingvalue, 'value_10');
                });
                //    if ($scope.Voucher.amount < 0) {
                //        $scope.Voucher.amount = $scope.Voucher.amount * -1;
                //        $scope.Voucher.adjustmentcredit = false;
                //        $.each($scope.Voucher.voucheritems, function (k, v) {
                //            v.iscredit = !v.iscredit;
                //        })
                //    }
            } else {
                $.each($scope.Voucher.voucheritems, function (k, v) {
                    if (v.id < 0) {
                        removeitems.push(v);
                    }
                });
                $.each(removeitems, function (k, v) {
                    $scope.Voucher.voucheritems.splice($scope.Voucher.voucheritems.indexOf(v), 1);
                })
            }

            $.each($scope.Voucher.voucheritems, function (k, v) {
                if (v.iscredit)
                    $scope.Voucher.amount -= v.amount;
                else
                    $scope.Voucher.amount += v.amount;
            });

            if ($scope.Voucher.amount < 0) {
                $scope.Voucher.amount = Math.abs($scope.Voucher.amount);
                $scope.Voucher.adjustmentcredit = false;
            }

            request = {
                "Item": $scope.Voucher,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "VoucherService/SavePettyCash",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("PettyCash Saved Successfully.!");
                    $scope.SearchCondition.opening = 0;
                    $scope.SearchCondition.yesterdayopening = 0;
                    $scope.SearchCondition.closing = 0;
                    $scope.SearchCondition.cashdeposit = 0;
                    $scope.SearchCondition.cashwithdraw = 0;
                    $scope.AllowEnter = false;
                    if ($scope.igrid != null)
                        $scope.igrid.destroy();
                    $scope.SearchPettyCash();
                }
                else {
                    $.each(removeitems, function (k, v) {
                        $scope.Voucher.voucheritems.splice(0, 0, v);
                        $scope.BuildGrid();
                    })
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            }).error(function (result) {
            });
        }

        $scope.AddVoucherItem = function (id, amount, existingvalue, columnname) {
            if (typeof (amount) == 'undefined' || amount == null) return null;
            amount = parseFloat(amount);
            if (isNaN(amount) || amount == 0) return null;
            var eitem = existingvalue.find(function (item) {
                return item.column == columnname;
            });
            var ledger = $scope.ExpenseTypes.find(function (item) {
                return item.id == id;
            });
            var iscredit = (ledger && (ledger.typeid == 21 || ledger.typeid == 22));
            $scope.Voucher.voucheritems.push({
                'ledgerid': id, 'amount': amount, 'isactive': true, 'version': 1, 'iscredit': iscredit,
                'id': typeof (eitem) != 'undefined' && eitem != null ? eitem.value.id : 0
            });
        }

        $scope.DownloadPDF = function () {
            $rootScope.PostDownload('VoucherService/DownloadPettyCash', $scope.SearchCondition,
                'pettycash.pdf', 'application/pdf');
        }

        $scope.CanDownload = false;
        
        $scope.Report = function (type) {
            if (type == 'pdf') {
                return $scope.DownloadPDF();
            }
            
            $scope.SearchCondition.type = type;
            $scope.SearchCondition.attributes = { 'counterid': $scope.SearchCondition.counterid };

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "VoucherService/SearchPettyCashReport",
                data: srequest
            }).success(function (result) {
                $scope.CanDownload = true;
                $scope.Report.yesterdayopening = Math.abs(result.opening);
                $scope.Report.yesterdayopeningcrdr = result.opening > 0 ?'Cr':'Dr';
                $scope.Report.cashdeposit = result.cashdeposit;
                $scope.Report.cashwithdraw = result.cashwithdraw;
                $scope.Report.cashincome = result.cashincome;
                $scope.Report.cashexpense = result.cashexpense;
                result.closing = result.opening + (result.cashincome + result.cashwithdraw) - (result.cashdeposit + result.cashexpense);
                $scope.Report.closingcrdr = result.closing > 0 ? 'Cr' : 'Dr';
                $scope.Report.closing = Math.abs(result.closing);
                if ($scope.igrid != null)
                    $scope.igrid.destroy();
                var igrid = new InfyGrid('report', '400px', '100%', true, true, true, 50);
                igrid.pagescope = $scope;
                if ($scope.SearchCondition.type == 'detail') {
                    igrid.AddFormatedColumn('date', 'Date', 100, 'dd-MM-yyyy');
                }
                igrid.AddSimpleColumn('ledger', 'Ledger', 100);
                if ($scope.SearchCondition.type == 'detail') {
                    igrid.AddSimpleColumn('narr', 'Narration', 200);
                }
                igrid.AddFormatedColumn('debit', 'Debit', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('credit', 'Credit', 100, 'N2', 2).total = true;
                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result.items,'_id');
            });
        }

        $scope.SearchPettyCash = function () {
            $scope.SearchCondition.attributes = { 'counterid': $scope.SearchCondition.counterid };
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "VoucherService/SearchPettyCash",
                data: srequest
            }).success(function (result) {
                if (result.message && result.message != null && result.message != '') {
                    $rootScope.Toaster.Info(result.message);
                }
                $scope.AllowEnter = true;
                $scope.Voucher = result.voucher;
                if ($scope.Voucher.voucheritems == undefined || $scope.Voucher.voucheritems == null)
                    $scope.Voucher.voucheritems = [];

                $scope.SearchCondition.employeeid = $scope.Voucher.employeeid;
                $scope.SearchCondition.yesterdayopening = result.opening;
                $scope.SearchCondition.cashdeposit = result.cashdeposit;
                $scope.SearchCondition.cashwithdraw = result.cashwithdraw;

                $scope.SearchCondition.cashincome = result.cashincome;
                $scope.SearchCondition.cashexpense = result.cashexpense;

                $scope.SearchCondition.expense = 0;
                $scope.SearchCondition.income = 0;
                $scope.SearchCondition.opening = result.opening + result.cashwithdraw
                    - result.cashdeposit + result.cashincome - result.cashexpense;

                $scope.SearchCondition.closing = $scope.SearchCondition.opening;
                $.each($scope.Voucher.voucheritems, function (k, v) {
                    $scope.ComputeTotal(1, v);
                });
                $scope.BuildGrid();

            }).error(function (result) {
            });
        }

        $scope.IsBankTransaction = false;
        $scope.ledgerchanged = function () {
            var ledger = $scope.AllLedgers.find(function (item) {
                return item.id == $scope.vouchertransaction.ledgerid;
            });
            if (ledger == undefined || ledger == null) {
                return;
            }
            $scope.IsBankTransaction = ledger.typeid == 7 || ledger.typeid == 19;
        }

        $scope.Focuscrdr = function (type) {
            var ledger = $scope.AllLedgers.find(function (item) {
                return item.id == $scope.vouchertransaction.ledgerid;
            });
            if (ledger == undefined || ledger == null) {
                return;
            }
            if (type == 'all') {
                if (ledger.typeid == 17 || ledger.typeid == 18) {
                    $scope.setFocuscredit(); 
                }
                else {
                    $scope.setFocusdebit();
                }
            }
            else if (type == 'debit') {
                if ($scope.vouchertransaction.debit > 0) {
                    $scope.AddVoucherTransaction();
                    $rootScope.Toaster.Info("Debit added on " + ledger.displayname);
                } else {
                    $scope.setFocuscredit();
                }
            } if (type == 'credit') {
                if ($scope.vouchertransaction.credit > 0) {
                    $scope.AddVoucherTransaction();
                    $rootScope.Toaster.Info("Credit added on " + ledger.displayname);
                } else {
                    $scope.setFocusaddbutton();
                }
            }
        }

        $scope.AddVoucherTransaction = function () {

            if ($scope.Voucher == null) {
                $rootScope.Toaster.Info("Please create voucher by click (+) button");
                return;
            }

            if ($scope.vouchertransaction.ledgerid == undefined ||
                $scope.vouchertransaction.ledgerid == null ||
                $scope.vouchertransaction.ledgerid == '') {
                $rootScope.Toaster.Info("Please select the ledger");
                return;
            }
            var credit = parseFloat($scope.vouchertransaction.credit);
            var debit = parseFloat($scope.vouchertransaction.debit);
            if (isNaN(credit)) credit = 0;
            if (isNaN(debit)) debit = 0;
            if (debit > 0 && credit > 0) {
                $rootScope.Toaster.Info("Can't allow credit / debit at the same time");
                return;
            }
            if (debit == 0 && credit == 0) {
                $rootScope.Toaster.Info("Please endter either credit or debit");
                return;
            }

            var ledger = $scope.AllLedgers.find(function (item) {
                return item.id == $scope.vouchertransaction.ledgerid;
            });

            var iscredit = credit > 0;
            var amount = iscredit ? credit : debit;

            var voucheritem = {
                'ledgerid': ledger.id, 'amount': amount, 'isactive': true, 'version': 1, 'iscredit': iscredit,
                'attributes': {
                    'ledgertype': ledger.typeid,
                    'narration': $scope.vouchertransaction.narration
                }
            };

            $scope.Voucher.voucheritems.push(voucheritem);
            $scope.ComputeTotal(1, voucheritem);
            if ($scope.Voucher.voucheritems.length == 1) {
                $scope.BuildVoucherGrid();
            }

            $scope.igrid.UpdateData($scope.Voucher.voucheritems, '_id');
            $scope.vouchertransaction.id = 0;
            $scope.vouchertransaction.narration = '';
            $scope.vouchertransaction.credit = 0;
            $scope.vouchertransaction.debit = 0;
            $scope.setFocusledgerid();
        }

        $scope.ComputeTotal = function (multiplier, voucheritem) {
            if (voucheritem.attributes == undefined || voucheritem.attributes == null) voucheritem.attributes = { 'ldgertype': 0 };
            if (voucheritem.id && voucheritem.id < 0) return;

            if (voucheritem.iscredit) {
                if (voucheritem.attributes.ledgertype == 7) {
                    $scope.SearchCondition.cashwithdraw += (voucheritem.amount * multiplier);
                }
                else if (voucheritem.attributes.ledgertype == 17 ||
                    voucheritem.attributes.ledgertype == 18 ||
                    voucheritem.attributes.ledgertype == 14 ||
                    voucheritem.attributes.ledgertype == 21 ||
                    voucheritem.attributes.ledgertype == 22) {
                    $scope.SearchCondition.income += (voucheritem.amount * multiplier);
                }
                else {
                    $scope.SearchCondition.cashincome += (voucheritem.amount * multiplier);
                }
            }
            else {
                if (voucheritem.attributes.ledgertype == 7) {
                    $scope.SearchCondition.cashdeposit += (voucheritem.amount * multiplier);
                }
                else if (voucheritem.attributes.ledgertype == 17 ||
                    voucheritem.attributes.ledgertype == 18 ||
                    voucheritem.attributes.ledgertype == 21 ||
                    voucheritem.attributes.ledgertype == 14 ||
                    voucheritem.attributes.ledgertype == 22) {
                    $scope.SearchCondition.expense += (voucheritem.amount * multiplier);
                }
                else {
                    $scope.SearchCondition.cashexpense += (voucheritem.amount * multiplier);
                }
            }
            $scope.SearchCondition.closing = $scope.SearchCondition.opening
                + $scope.SearchCondition.cashwithdraw
                + $scope.SearchCondition.cashincome
                + $scope.SearchCondition.income
                - $scope.SearchCondition.cashdeposit
                - $scope.SearchCondition.cashexpense
                - $scope.SearchCondition.expense;
        }


        $scope.BuildVoucherGrid = function () {


            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 50);
            igrid.pagescope = $scope;
            var column = igrid.AddCustomColumn('ledgerid', 'Ledger', 300, function (item, col) {
                if (item.ledgerid &&
                    item.ledgerid > 0) {
                    var exist = $scope.AllLedgers.find(function (fitem) {
                        return fitem.id == item.ledgerid;
                    });
                    if (exist && exist != null)
                        return exist.displayname;
                }
                return '';
            });

            var column = igrid.AddCustomColumn('narration', 'Narration', 300, function (item, col) {
                if (item.attributes &&
                    item.attributes.narration)
                    return item.attributes.narration;
                return '';
            });

            var column = igrid.AddCustomColumn('debit', 'Debit', 80, function (item, col) {
                if (!item.iscredit)
                    return item.amount;
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (!item.iscredit)
                    return item.amount;
                return 0;
            }

            var column = igrid.AddCustomColumn('credit', 'Credit', 80, function (item, col) {
                if (item.iscredit)
                    return item.amount;
                return '';
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (item.iscredit)
                    return item.amount;
                return 0;
            }


            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $timeout(function () {
                            $scope.ComputeTotal(-1, item);
                            $scope.Voucher.voucheritems.splice($scope.Voucher.voucheritems.indexOf(item), 1);
                            $scope.igrid.UpdateData($scope.Voucher.voucheritems, '_id');
                        }, 10);
                    }
                    else if (command == 'Edit') {
                        $timeout(function () {
                            $scope.ComputeTotal(-1, item);
                            $scope.Voucher.voucheritems.splice($scope.Voucher.voucheritems.indexOf(item), 1);
                            $scope.igrid.UpdateData($scope.Voucher.voucheritems, '_id');
                            $scope.vouchertransaction = {};
                            $scope.vouchertransaction.id = item.id;
                            $scope.vouchertransaction.ledgerid = item.ledgerid;
                            $scope.vouchertransaction.narration = item.attributes ? item.attributes.narration : '';
                            $scope.vouchertransaction.credit = item.iscredit ? item.amount : 0;
                            $scope.vouchertransaction.debit = !item.iscredit ? item.amount : 0;
                        }, 10);
                    }
                });
            });
            $scope.igrid = igrid;
            if ($scope.Voucher && $scope.Voucher != null && $scope.Voucher.voucheritems) {
                $scope.igrid.UpdateData($scope.Voucher.voucheritems, '_id');
            }
            else {
                $scope.igrid.UpdateData([]);
            }
            //igrid.grid.setOptions({ autoEdit: true });
        }


        $scope.Voucher = null;
        $scope.VoucherData = null;
        $scope.BuildGrid = function () {

            if ($scope.finance_pettycashvoucher) {
                $scope.BuildVoucherGrid();
                return;
            }

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);

            igrid.pagescope = $scope;

            igrid.AddSimpleColumn('detail', 'Particulars', 200);
            var column = igrid.AddCustomColumn('mode', 'Mode', 50, function (item, col) {
                if (item.typeid == 17 || item.typeid == 18) {
                    return 'Exp';
                }
                return 'Inc';
            });
            igrid.AddSimpleColumn('total', 'Total', 80); ''
            igrid.AddSimpleColumn('value_1', '1', 80).editor = Slick.Editors.NumberEditor;
            igrid.AddSimpleColumn('value_2', '2', 80).editor = Slick.Editors.NumberEditor;
            igrid.AddSimpleColumn('value_3', '3', 80).editor = Slick.Editors.NumberEditor;
            igrid.AddSimpleColumn('value_4', '4', 80).editor = Slick.Editors.NumberEditor;
            igrid.AddSimpleColumn('value_5', '5', 80).editor = Slick.Editors.NumberEditor;
            igrid.AddSimpleColumn('value_6', '6', 80).editor = Slick.Editors.NumberEditor;
            igrid.AddSimpleColumn('value_7', '7', 80).editor = Slick.Editors.NumberEditor;
            igrid.AddSimpleColumn('value_8', '8', 80).editor = Slick.Editors.NumberEditor;
            igrid.AddSimpleColumn('value_9', '9', 80).editor = Slick.Editors.NumberEditor;
            igrid.AddSimpleColumn('value_10', '10', 80).editor = Slick.Editors.NumberEditor;
            var list = [];
            $scope.VoucherData = list;
            $.each($scope.ExpenseTypes, function (k, v) {

                var item = {
                    'detail': v.displayname, 'ledgerid': v.id, 'typeid': v.typeid, 'value_1': 0, 'value_2': 0, 'value_3': 0, 'value_4': 0, 'value_5': 0, 'value_6': 0,
                    'value_7': 0, 'value_8': 0, 'value_9': 0, 'value_10': 0, 'total': 0, 'existingvalue': []
                };
                list.push(item);

                if ($scope.Voucher != null && $scope.Voucher.voucheritems != null && $scope.Voucher.voucheritems.length > 0) {
                    var index = 0;

                    for (var i = 0; i < $scope.Voucher.voucheritems.length; i++) {
                        vv = $scope.Voucher.voucheritems[i];
                        if (vv.ledgerid == v.id) {
                            index++;
                            if (index == 11) {
                                var item = {
                                    'detail': v.displayname, 'ledgerid': v.id, 'typeid': v.typeid, 'value_1': 0, 'value_2': 0, 'value_3': 0, 'value_4': 0, 'value_5': 0, 'value_6': 0,
                                    'value_7': 0, 'value_8': 0, 'value_9': 0, 'value_10': 0, 'total': 0, 'existingvalue': []
                                };
                                list.push(item);
                                index = 1;
                            }
                            item['value_' + index] = vv.amount;
                            item.existingvalue.push({ 'column': 'value_' + index, 'value': vv });
                            item.total += vv.amount;
                        }
                    }

                    if (v.typeid == 21 || v.typeid == 22) {
                        $scope.SearchCondition.income += item.total;
                        $scope.SearchCondition.closing += item.total;
                    }
                    else {
                        $scope.SearchCondition.expense += item.total;
                        $scope.SearchCondition.closing -= item.total;
                    }
                    if (index == 10) {
                        var item = {
                            'detail': v.displayname, 'typeid': v.typeid,
                            'ledgerid': v.id, 'value_1': 0, 'value_2': 0, 'value_3': 0, 'value_4': 0, 'value_5': 0, 'value_6': 0,
                            'value_7': 0, 'value_8': 0, 'value_9': 0, 'value_10': 0, 'total': 0, 'existingvalue': []
                        };
                        list.push(item);
                    }
                }
            });

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(list, '_id');
            //igrid.grid.setOptions({ autoEdit: true });

            igrid.grid.onCellChange.subscribe(function (e, args) {

                args.grid.invalidateRow(args.row);
                var row = args.grid.infyGrid.dataView.getItem(args.row);

                if (row.typeid == 21 || row.typeid == 22) {
                    args.grid.infyGrid.pagescope.SearchCondition.income -= args.item.total;
                    args.grid.infyGrid.pagescope.SearchCondition.closing -= args.item.total;
                }
                else {
                    args.grid.infyGrid.pagescope.SearchCondition.expense -= args.item.total;
                    args.grid.infyGrid.pagescope.SearchCondition.closing += args.item.total;
                }

                args.item.total = args.item.value_1 + args.item.value_2 + args.item.value_3 + args.item.value_4 + args.item.value_5 +
                    args.item.value_6 + args.item.value_7 + args.item.value_8 + args.item.value_9 + args.item.value_10;

                if (row.typeid == 21 || row.typeid == 22) {
                    args.grid.infyGrid.pagescope.SearchCondition.closing += args.item.total;
                    args.grid.infyGrid.pagescope.SearchCondition.income += args.item.total;
                }
                else {
                    args.grid.infyGrid.pagescope.SearchCondition.closing -= args.item.total;
                    args.grid.infyGrid.pagescope.SearchCondition.expense += args.item.total;
                }

                args.grid.infyGrid.dataView.updateItem(args.item.id, row);
                args.grid.render();

                $scope.$digest();
            });
        }

        $scope.exporttoExcel = function (infygrid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'Petty Cash - ' +
                $filter('date')($scope.SearchCondition.voucherdate, 'dd-MM-yyyy');

            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "pettycash.xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }


        $scope.OpeningChanged = function () {
            var total = 0;
            $.each($scope.VoucherData, function (k, v) {
                total += v.total;
            });
            $scope.SearchCondition.closing = $scope.SearchCondition.opening - total;
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
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
                                    url: "ShopactivityService/Delete",
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
                $scope.PettyCash = item;
            }
        }

        $scope.Search = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ShopactivityService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }

        $scope.finance_pettycashvoucher = false;
        $scope.LoadAppSetting = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadAppSetting, 100);
            } else {
                $scope.finance_pettycashvoucher = $rootScope.UserInfo.appsettings.finance_pettycashvoucher;
                $scope.BuildGrid();
            }
        }

        $timeout(function () {
            $scope.LoadAppSetting();
        }, 10);
    }]);