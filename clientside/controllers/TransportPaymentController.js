
var app = angular.module('InfinityLab');

app.controller('TransportPaymentController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {

        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SupplierCompanyList = []
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
            $rootScope.BindCompany($scope, 'TransportPayment.companyid');
        });

        $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transports = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
            $scope.ShowEntryLocation = $rootScope.UserInfo.appsettings.system_showroompurchase || false;
            $rootScope.BindLocation($scope, 'TransportPayment.locationid');
        });

        $scope.LoadingCharges = 0;
        $scope.DiscountAmount = 0;
        $scope.OtherCharges = 0;
        $scope.AddedAmount = 0;
        $scope.TDSAmount = 0;
        $scope.TotalInvoiceAmount = 0;
        $scope.TotalChequeAmount = 0;
        $scope.TotalDiscountAmount = 0;

        $scope.SourceModes = [{ 'id': 0, 'name': 'Transport' },
        { 'id': 1, 'name': 'Courier' }];

        $scope.PaymentModes = [{ 'id': 0, 'name': 'NEFT/RTGS' },
        { 'id': 1, 'name': 'Cheque/DD' },
        { 'id': 2, 'name': 'Cash' },
        { 'id': 3, 'name': 'Multiple Cheque/DD' },
        { 'id': 4, 'name': 'PDC' }
        ];

        $scope.Entity = {
            "id": 0,
            "supplierid": 0,
            "suppliercompanyid": 0,
            "companyid": 0,
            "receiptno": null,
            "paymentdate": null,
            "datatransmitted": false,
            "paidamount": 0.0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "Transportpaymentinvoice": [],
            "bankcheques": []
        };
        $scope.TransportPayment = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.AddedAmount = 0;
        $scope.BalanceAmount = 0;
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.ShowPendingBills = false;
        $scope.showSearchResult = true;
        $scope.SearchResult = [];
        $scope.ErrorMessage = "";
        $scope.LoadPendinginvoice = [];



        $scope.TransportPayment.paymentdate = new Date();

        $scope.debitnote = {
            "background-color": "lightgray"
        };

        $scope.lrinvoice = {
            "background-color": "white"
        };

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.TransportPayment = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.LoadPendinginvoice = [];
        }
        $scope.AddedAmountChanged = function () {
            $scope.AddedAmount = 0;
            $scope.LoadingCharges = 0;
            $scope.OtherCharges = 0;
            $scope.DiscountAmount = 0;
            $.each($scope.LoadPendinginvoice, function (k, v) {
                if ($rootScope.GetNumber(v.settlementamount) < 0)
                    v.settlementamount = 0;
                if ($rootScope.GetNumber(v.settlementamount) > (v.totalamount - v.paidamount))
                    v.settlementamount = (v.totalamount - v.paidamount);

                $scope.DiscountAmount += $rootScope.GetNumber(v.discountamount);
                if ($rootScope.GetNumber(v.settlementamount) > 0) {
                    $scope.AddedAmount += $rootScope.GetNumber(v.settlementamount);
                    $scope.LoadingCharges += v.loadingcharge;
                    $scope.OtherCharges += v.othercharges;
                }
            });
            $scope.TransportPayment.adjustableamount = $scope.AddedAmount;
        }


        $scope.SearchItems = function () {
            var request = {
                "Item": {
                    transportid: $scope.TransportPayment.transportid,
                    companyid: $scope.TransportPayment.companyid,
                    sourcemode: $scope.TransportPayment.sourcemode,
                    fromdate: $scope.TransportPayment.fromdate,
                    todate: $scope.TransportPayment.todate
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TransportpaymentService/SearchPendingPayment",
                data: srequest
            }).success(function (result) {
                if ($scope.TransportPayment && $scope.TransportPayment.id &&
                    $scope.TransportPayment.id > 0) {
                    $.each(result, function (k, v) {
                        $scope.LoadPendinginvoice.push(v);
                    })
                }
                else {
                    $scope.LoadPendinginvoice = result;
                }
                $scope.SearchCondition.showSearchResult = $scope.LoadPendinginvoice.length > 0;
                $scope.ShowPendingBills = $scope.LoadPendinginvoice.length > 0;

                $scope.LoadingCharges = 0;
                $scope.OtherCharges = 0;
                $scope.BalanceAmount = 0;
                $scope.AddedAmount = 0;
                $scope.DiscountAmount = 0;
                $.each($scope.LoadPendinginvoice, function (k, v) {
                    $scope.BalanceAmount += (v.frightcharge - v.paidamount);
                    v.discountamount = 0;
                });
                $timeout(function () {
                    $scope.ConfigInvoiceGrid();
                }, 10);
            }).error(function (result) {
            });
        };

        $scope.AddMultipleCheque = function () {
            $scope.TransportPayment.bankcheques.push({
                bankid: $scope.TransportPayment.bankid,
                chequedate: $scope.TransportPayment.chequedate,
                chequeno: $scope.TransportPayment.referencedetail,
                amount: parseFloat($scope.TransportPayment.chequeamount),
                sourcetype: 'TPMT',
                companyid: $scope.TransportPayment.companyid,
                accountname: $scope.Transports.find(function (item) {
                    return item.id == $scope.TransportPayment.transportid;
                }).name
            });
            $scope.TotalChequeAmount += parseFloat($scope.TransportPayment.chequeamount);
        }

        $scope.tdspercentagechanged = function () {
            var tdspercentage = parseFloat($scope.TransportPayment.tdspercentage);
            if (isNaN(tdspercentage)) tdspercentage = 0;
            if (tdspercentage != 0) {
                $scope.TDSAmount = Math.round(($scope.AddedAmount - $scope.DiscountAmount) * tdspercentage / 100);
            }
            else {
                $scope.TDSAmount = 0;
            }
            $scope.TransportPayment.tdsamount = $scope.TDSAmount;
        }

        $scope.ManagePaymentItem = function (action, item) {
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
                                    url: "TransportpaymentService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.igrid.UpdateData($scope.SearchResult, '_id');
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



            } else if (action == "Edit") {
                $scope.TransportPayment = {
                    'supplierid': item.supplierid
                };
                var request = {
                    "Item": { 'id': item.id },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "TransportpaymentService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length == 1) {
                        $scope.TransportPayment = result[0];
                        $scope.ShowSearch = false;
                        $scope.Navigation.SetMode('EDIT');
                        $scope.LoadPendinginvoice = $scope.TransportPayment.Transportpaymentinvoice;
                        $scope.LoadingCharges = 0;
                        $scope.OtherCharges = 0;
                        $scope.BalanceAmount = 0;
                        $scope.AddedAmount = 0;
                        $scope.DiscountAmount = 0;
                        $.each($scope.LoadPendinginvoice, function (k, v) {
                            v.settlementamount = v.paidamount;
                            v.paidamount = 0;
                            v.fullvalue = true;
                            $scope.BalanceAmount += (v.frightcharge - v.paidamount);
                        });
                        $timeout(function () {
                            $scope.AddedAmountChanged();
                            $scope.ConfigInvoiceGrid();
                        }, 10);
                    }
                }).error(function (result) {
                });
            }
        }


        $scope.Apply = function () {
            if ($scope.TransportPayment.adjustableamount > 0) {
                var amount = $scope.TransportPayment.adjustableamount;

                $.each($scope.LoadPendinginvoice, function (k, v) {

                    if (amount >= 0) {
                        var balanceamt = (v.loadingcharge + v.frightcharge) - v.paidamount;
                        v.settlementamount = parseFloat(v.settlementamount);
                        if (isNaN(v.settlementamount))
                            v.settlementamount = 0;

                        if (balanceamt > amount) {
                            v.settlementamount += amount;
                            amount = 0;
                        }
                        else {
                            v.settlementamount += balanceamt;
                            amount -= balanceamt;
                        }
                    }
                });

                $scope.AddedAmountChanged();
                $scope.invgrid.UpdateData($scope.LoadPendinginvoice,'_id');
            }
        }

        $scope.Save = function () {

            if ($scope.TransportPayment.paymentmode == 3) {
                $scope.TransportPayment.paidamount = $scope.TotalChequeAmount;
            }
            else {
                $scope.TransportPayment.bankcheques = [];
                $scope.TransportPayment.bankcheques.push({
                    bankid: $scope.TransportPayment.bankid,
                    chequedate: $scope.TransportPayment.chequedate,
                    chequeno: $scope.TransportPayment.referencedetail,
                    amount: parseFloat($scope.TransportPayment.paidamount),
                    sourcetype: 'TPMT',
                    companyid: $scope.TransportPayment.companyid,
                    accountname: $scope.Transports.find(function (item) {
                        return item.id == $scope.TransportPayment.transportid;
                    }).name
                });
            }

            if ($scope.TransportPayment.paidamount == null || $scope.TransportPayment.paidamount == 0) {
                $scope['setFocuspaidamount']();
                $rootScope.Toaster.Info("Please enter paidamount");
                return;
            }

            if ($scope.TransportPayment.paymentdate == null || $scope.TransportPayment.paymentdate == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info("Please select paymentdate");
                return;
            }

            if ($scope.TransportPayment.transportid == null || $scope.TransportPayment.transportid == 0) {
                $scope['setFocustransportid']();
                $rootScope.Toaster.Info("Please select transport");
                return;
            }

            if ($scope.TransportPayment.companyid == null || $scope.TransportPayment.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select company");
                return;
            }

            $scope.TransportPayment.Transportpaymentinvoice = [];
            $.each($scope.LoadPendinginvoice, function (k, v) {
                if (v.id && v.id > 0) {
                    v.paidamount = v.settlementamount;
                    $scope.TransportPayment.Transportpaymentinvoice.push(v);
                }
                else if (v.settlementamount > 0 || v.discountamount > 0) {
                    $scope.TransportPayment.Transportpaymentinvoice.push({
                        'transportid': $scope.TransportPayment.transportid,
                        'lrid': v.lrid,
                        'lrno': v.lrno,
                        'lrdate': v.lrdate,
                        'receivedon': v.receivedon,
                        'paidamount': v.settlementamount,
                        'discountamount': v.discountamount,
                        'fromlocation': v.fromlocation
                    });
                }
            });

            request = {
                "Item": $scope.TransportPayment,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TransportpaymentService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with Reference No : " + result.receiptno);
                    $scope.New();
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }
        $scope.SearchCondition = {
            supplierid: 0,
            paymentdate: new Date(),
            lrinvoiceno: ""
        }

        $scope.invgrid = null;
        $scope.ConfigInvoiceGrid = function () {
            if ($scope.invgrid != null)
                $scope.invgrid.destroy();

            var igrid = new InfyGrid('gridinvoiceContainer', '370px', '100%', true, false, true, 80);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.LoadPendinginvoice);
            igrid.AddCustomColumn('fullvalue',
                '<input type="checkbox" style="z-index: auto;margin-left:5px">', 30, function (item, col) {
                    if (item.fullvalue)
                        return '<input type="checkbox" style="z-index: auto;" checked>';
                    else
                        return '<input type="checkbox" style="z-index: auto;">';
                });
            var dcolumn = igrid.AddColumn(
                {
                    id: "settlementamount", name: "Amount", field: "settlementamount",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            var dcolumn = igrid.AddColumn(
                {
                    id: "discountamount", name: "Discount", field: "discountamount",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            if (typeof ($scope.TransportPayment.transportid) == 'undefined' ||
                $scope.TransportPayment.transportid <= 0) {
                igrid.AddSimpleColumn('transportname', 'Transport', 120);
            }

            igrid.AddSimpleColumn('supplier', 'Supplier', 120);
            igrid.AddSimpleColumn('lrno', 'LR.No', 80);
            igrid.AddSimpleColumn('lrentryno', 'Entry No', 100);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
            igrid.AddSimpleColumn('fromlocation', 'From', 100);
            igrid.AddFormatedColumn('loadingcharge', 'Loading', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('frightcharge', 'Fright', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('othercharges', 'Other', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax Amount', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('noofboxesreceived', 'Boxs', 40, 'N2', 2).total = true;
            igrid.AddFormatedColumn('noofbundelsreceived', 'Bundle', 50, 'N2', 2).total = true;
            //igrid.AddCustomColumn('boxbundles',
            //    'Box/Bndl', 50, function (item, col) {
            //        return item.noofboxesreceived + '/' + item.noofbundelsreceived;
            //    });
            igrid.AddFormatedColumn('chargedweight', 'Weight', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('lrdate', 'LR Date', 80, 'dd/MM/yyyy');
            igrid.AddFormatedColumn('receivedon', 'LR Rec Date', 80, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('lrreceiptno', 'LR Recpt No', 80);
            igrid.AddFormatedColumn('paidamount', 'Paid', 90, 'N2', 2).total = true;

            igrid.SubscribeClickEvent();
            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();
            //igrid.grid.setOptions({ 'frozenColumn': 4 });

            igrid.grid.onHeaderClick.subscribe(function (e, args) {

                if (args.column.id == 'fullvalue' && $(e.target).is(":checkbox")) {
                    if (igrid.grid.getEditorLock().isActive() && !igrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }

                    if ($(e.target).is(":checked")) {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.fullvalue = true;
                            item.settlementamount = item.fullvalue ? (item.totalamount - item.paidamount) : 0;
                        }
                    } else {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.fullvalue = false;
                            item.settlementamount = 0;
                        }
                    }
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                    $timeout(function () {
                        $scope.AddedAmountChanged();
                        args.grid.invalidate();
                        args.grid.render();
                    }, 10);
                }

            });

            //igrid.dataView.getItemMetadata = function (row) {
            //    var item = igrid.dataView.getItem(row);
            //    if (!item.isinvoice) {
            //        return { cssClasses: 'igriddebitnoterow' };
            //    }
            //};

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'fullvalue') {
                    item = args.grid.getDataItem(args.row);
                    item.fullvalue = !item.fullvalue && $(e.target).val() == 'on';
                    item.settlementamount = item.fullvalue ? (item.totalamount - item.paidamount) : 0;

                    $timeout(function () {
                        $scope.AddedAmountChanged();
                        args.grid.invalidate();
                        args.grid.render();
                    }, 10);

                }
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'discountamount') {
                    if ((item.totalamount - item.paidamount) < $rootScope.GetNumber(item.settlementamount) +
                        $rootScope.GetNumber(item.discountamount)) {
                        item.settlementamount = (item.totalamount - item.paidamount) - $rootScope.GetNumber(item.discountamount);
                    }
                    $timeout(function () {
                        $scope.AddedAmountChanged();
                        args.grid.invalidate();
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'settlementamount') {
                    if ($rootScope.GetNumber(item.settlementamount) +
                        $rootScope.GetNumber(item.discountamount) > (item.totalamount - item.paidamount)) {
                        item.settlementamount = (item.totalamount - item.paidamount) -
                            $rootScope.GetNumber(item.discountamount);
                        $timeout(function () {
                            $scope.AddedAmountChanged();
                            args.grid.invalidate();
                            args.grid.render();
                        }, 10);
                    }
                    else {
                        $timeout(function () {
                            $scope.AddedAmountChanged();
                        }, 10);
                    }
                }
            });

            $scope.invgrid = igrid;
            $scope.invgrid.UpdateData($scope.LoadPendinginvoice, '_id');
        }

        $scope.New = function () {
            $scope.TransportPayment = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchCondition.showSearchResult = false;
            $scope.ShowSearch = false;
            $scope.LoadPendinginvoice = [];
            $scope.AddedAmount = 0;
            $scope.AdjustedAmount = 0;
            $scope.TotalInvoiceAmount = 0;
            $scope.TotalChequeAmount = 0;
            $scope.TotalDiscountAmount = 0;
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TransportpaymentService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ShowSearchResult();
            }).error(function (result) {
            });
        }

        $scope.igrid = null;
        $scope.ShowSearchResult = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.SearchResult);
            igrid.AddReferenceColumn('transportid', 'Transport', 200, $scope.Transports);
            igrid.AddReferenceColumn('sourcemode', 'Transport Mode', 100, $scope.SourceModes);
            igrid.AddSimpleColumn('receiptno', 'Receipt No', 100);
            igrid.AddFormatedColumn('paymentdate', 'Payment Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('paymentmode', 'Payment Mode', 120, $scope.PaymentModes);
            igrid.AddSimpleColumn('referencedetail', 'Ref.No', 100);
            igrid.AddSimpleColumn('lrentrynos', 'Entry No', 100);
            igrid.AddFormatedColumn('paidamount', 'Amount', 100, 'N2', 2).total = true;

            var commands = "";
            if ($rootScope.UserInfo.p_TRANSPORTPAYMENT.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_TRANSPORTPAYMENT.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            if ($rootScope.UserInfo.p_TRANSPORTPAYMENT.print)
                commands += "<input type='button' class='btn btn-success btn-xs' value='Print'/> &nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 160, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManagePaymentItem('Edit', args.item);
                    else if ($(e.target).val() == 'Print')
                        $scope.PrintCheuqeAdvice(args.item.id);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManagePaymentItem('Delete', args.item);
                });
            });

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult, '_id');
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'TRANSPORT PAYMENT - INFORMATION'
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

        $scope.PrintCheuqeAdvice = function (id) {
            $rootScope.Download('TransportPaymentService/PrintCheuqeAdvicePDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.SupplierPayment.id : id),
                'paymentadvice.pdf', 'application/pdf');
            return;
        }

        $timeout(function () {
            $scope.ConfigInvoiceGrid();
        }, 10);

    }]);

