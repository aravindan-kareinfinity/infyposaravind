var app = angular.module('InfinityLab');

app.controller('VoucherController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', '$filter', 'localStorageService', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter, localStorageService, $stateParams) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.SetMode('SEARCH');
        $scope.ShowNew = false;
        $scope.LastVoucher = null;

        $scope.FilterTypes = [{ 'id': 0, 'name': 'All' },
        { 'id': 1, 'name': 'Detail' },
        { 'id': 2, 'name': 'Duplicate' },
        { 'id': 9, 'name': 'Filter' }];


        $scope.ItemTypes = [{ 'id': 1, 'name': 'Capital' },
        { 'id': 2, 'name': 'Fixed asset' },
        { 'id': 19, 'name': 'Current asset' },
        { 'id': 3, 'name': 'Libalities' },
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
        { 'id': 21, 'name': 'Petty Cash Direct Income' }, //4
        { 'id': 22, 'name': 'Petty Cash In-Direct Income' } //9
        ];

        $scope.refvouchertypelist = [
            { 'id': 0, 'typeid': 0, 'name': 'Cash Voucher' },
            { 'id': -1, 'typeid': 1, 'name': 'Payment' },
            { 'id': -2, 'typeid': 2, 'name': 'Receipt' },
            { 'id': -3, 'typeid': 3, 'name': 'Purchase' },
            { 'id': -4, 'typeid': 4, 'name': 'Sales' },
            { 'id': -5, 'typeid': 5, 'name': 'Credit Note' },
            { 'id': -6, 'typeid': 6, 'name': 'Debit Note' },
            { 'id': -7, 'typeid': 7, 'name': 'Contra Cr' },
            { 'id': -17, 'typeid': 17, 'name': 'Contra Dr' },
            { 'id': -8, 'typeid': 8, 'name': 'Journal Cr' },
            { 'id': -16, 'typeid': 16, 'name': 'Journal Dr' },
            { 'id': -14, 'typeid': 14, 'name': 'Advance Payment' },
            { 'id': -18, 'typeid': 18, 'name': 'Advance Refund' },
            { 'id': -15, 'typeid': 15, 'name': 'Petty Cash Expense' },
            { 'id': -33, 'typeid': 33, 'name': 'Purchase Order' },

            { 'id': -9, 'typeid': 9, 'name': 'Cash Deposit' },
            { 'id': -10, 'typeid': 10, 'name': 'Cash Widhrawal' },
            { 'id': -11, 'typeid': 11, 'name': 'Main Cash to Petty Cash' },
            { 'id': -12, 'typeid': 12, 'name': 'Petty Cash to Main Cash' },
            { 'id': -13, 'typeid': 13, 'name': 'Cash Counter Expense' },
            { 'id': -99, 'typeid': 99, 'name': 'EOD Posting' }
        ];


        $scope.TransferModes = [
            { 'id': 1, 'name': 'NEFT' },
            { 'id': 2, 'name': 'CHEQUE' },
            { 'id': 3, 'name': 'DD' }
        ];

        $scope.EnumCRDR = {
            'Cr': 1,
            'Dr': 2
        }

        $scope.Company = restService.Get('ReferencelistService/FULLCOMPANY', true).then(function (results) {
            $scope.Company = results;
            //    $rootScope.BindCompany($scope, 'SearchCondition.companyid');
            //    $rootScope.BindCompany($scope, 'Entity.companyid');
        });

        $scope.Location = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.Location = results;
            //    $rootScope.BindLocation($scope, 'SearchCondition.locationid');
            //    $rootScope.BindLocation($scope, 'Entity.locationid');
        });

        restService.Get('ReferencelistService/FULL-VOUCHERTYPE', true).then(function (results) {
            var newvoucherList = (JSON.parse(JSON.stringify($scope.refvouchertypelist)));
            $.each(results, function (k, v) {
                newvoucherList.push({ 'id': v.id, 'name': v.name, 'typeid': v.additionalinfo.parentid });
            })
            $scope.refvouchertypelist = newvoucherList;
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.PurchaseType = [];
        restService.Get('ReferencelistService/FULL-PURCHASETYPE', true).then(function (results) {
            $scope.PurchaseType = results;
        });

        $scope.system_disableautoledger = false;
        $rootScope.OnUserInfoReady(function (userinfo) {
            if (userinfo.appsettings && userinfo.appsettings != null)
                $scope.system_disableautoledger = userinfo.appsettings.system_disableautoledger;
        });

        $scope.allledgerlist = [];
        $scope.refbanklist = [];

        //1-credit,2-debit
        $scope.Entity = {
            "id": 0,
            "vouchertype": 0,
            "voucherdate": new Date(),
            "voucherno": "",
            "adjustmentledgerid": 0,
            "narration": "",
            "adjustmentcredit": false,
            "amount": '',
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": new Date(),
            "voucheritems": [],
            "paytype": 1,
            "crdr": 1,
            "paybyid": 0,
            "attributes": { 'dgst': false, 'dtds': false }
        }

        $scope.VoucherItemEntity = {
            "id": 0,
            "voucherid": 0,
            "iscredit": 0,
            "ledgerid": 0,
            "amount": '',
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": new Date()
        }
        $scope.Voucheritems = (JSON.parse(JSON.stringify($scope.VoucherItemEntity)));

        $scope.LedgerDisplay = function (ledgerid) {
            var ledger = $scope.allledgerlist.find(function (item) {
                return item.id == ledgerid;
            });
            if (ledger && ledger != null)
                return ledger.displayname;
            return '';
        }

        $scope.GetVoucherType = function (vouchertypeid) {
            var vouchertype = $scope.refvouchertypelist.find(function (item) {
                return item.id == vouchertypeid;
            });
            if (vouchertype && vouchertype != null)
                return vouchertype.typeid;
            return -1;
        }

        $scope.CreateTax = function () {
            $scope.Voucher.TaxInfo = {
                'TaxLedgers': [],
                'TaxList': [],
                'IGSTPercentage': 0,
                'CGSTPercentage': 0,
                'SGSTPercentage': 0,
                'IGSTTax': 0,
                'CGSTTax': 0,
                'SGSTTax': 0,
                'TDSTax': 0,
                'GSTTax': 0,
                'HasTax': false,
                'Clear': function () {
                    this.TaxLedgers = [];
                    this.TaxList = [];
                    this.IGSTPercentage = 0;
                    this.CGSTPercentage = 0;
                    this.SGSTPercentage = 0;
                    this.IGSTTax = 0;
                    this.CGSTTax = 0;
                    this.SGSTTax = 0;
                    this.TDSTax = 0;
                    this.GSTTax = 0;
                    this.HasTax = false;
                },
                'UpdateTax': function () {
                    if (this.TaxLedgers.length > 0) {
                        var taxInfo = this;
                        taxInfo.HasTax = true;
                        taxInfo.TaxList = [];

                        $.each(this.TaxLedgers, function (k, v) {
                            var taxpercentage = v.taxpercentage;
                            var taxtype = v.taxtype == 'IGST' ? 'IGST' : (v.taxtype == 'TDS' ? 'TDS' : 'GST');
                            var existtl = taxInfo.TaxList.find(function (tsitem) {
                                return tsitem.taxtype == taxtype && tsitem.taxpercentage == v.taxpercentage;
                            });
                            if (existtl == undefined || existtl == null) {
                                existtl = {
                                    'taxtype': taxtype,
                                    'taxpercentage': taxpercentage,
                                    'IGSTPercentage': 0,
                                    'CGSTPercentage': 0,
                                    'SGSTPercentage': 0,
                                    'TDSPercentage': 0,
                                    'IGSTTax': 0,
                                    'CGSTTax': 0,
                                    'SGSTTax': 0,
                                    'TDSTax': 0,
                                    'TDSLedgerID': 0
                                };
                                taxInfo.TaxList.push(existtl);
                            }
                            if (v.taxtype == 'IGST') {
                                taxInfo.IGSTPercentage = v.taxpercentage;
                                taxInfo.IGSTTax += v.amount;
                                existtl.IGSTPercentage = v.taxpercentage;
                                existtl.IGSTTax += v.amount;
                            } else if (v.taxtype == 'CGST') {
                                taxInfo.CGSTPercentage = v.taxpercentage;
                                taxInfo.CGSTTax += v.amount;
                                existtl.CGSTPercentage = v.taxpercentage;
                                existtl.CGSTTax += v.amount;
                            } else if (v.taxtype == 'SGST') {
                                taxInfo.SGSTPercentage = v.taxpercentage;
                                taxInfo.SGSTTax += v.amount;
                                existtl.SGSTPercentage = v.taxpercentage;
                                existtl.SGSTTax += v.amount;
                            }
                            else if (v.taxtype == 'TDS') {
                                taxInfo.TDSPercentage = v.taxpercentage;
                                taxInfo.TDSTax += v.amount;
                                existtl.TDSPercentage = v.taxpercentage;
                                existtl.TDSTax += v.amount;
                                existtl.TDSLedgerID = v.ledgerid;
                            }
                            existtl.GSTTax = existtl.IGSTTax + existtl.CGSTTax + existtl.SGSTTax;
                        });
                        taxInfo.GSTTax = taxInfo.IGSTTax + taxInfo.CGSTTax + taxInfo.SGSTTax;
                    }
                    else {
                        var taxInfo = this;
                        taxInfo.TaxList = [];
                        taxInfo.IGSTPercentage = 0;
                        taxInfo.CGSTPercentage = 0;
                        taxInfo.SGSTPercentage = 0;
                        taxInfo.TDSPercentage = 0;
                        taxInfo.IGSTTax = 0;
                        taxInfo.CGSTTax = 0;
                        taxInfo.SGSTTax = 0;
                        taxInfo.TDSTax = 0;
                        taxInfo.GSTTax = 0;
                        taxInfo.HasTax = false;
                    }
                }
            };
        }

        $scope.uploadimagecomplete = function (data) {
            $scope.Voucher.attributes.imgid = data[0];
            $scope.imgid = null;
        };

        $scope.ComputeGSTTax = function () {

            var partyledger = $scope.allledgerlist.find(function (item) {
                return item.id == $scope.Voucher.adjustmentledgerid;
            });
            var taxledger = null;
            var taxledgerentry = null;
            var totaltaxable = 0;
            var taxableledger = null;
            var tdsledger = null;
            $scope.Voucher.TaxInfo.TDSTotalTransaction = 0;
            $scope.Voucher.TaxInfo.TDSSingleValue = 0;
            $scope.Voucher.TaxInfo.TDSTotalValue = 0;
            $scope.Voucher.TaxInfo.TDSReason = '';
            $scope.Voucher.TaxInfo.TDSExist = false;
            if ($scope.Voucher.TaxInfo && $scope.Voucher.TaxInfo != null) {
                if ($scope.Voucher.TaxInfo.Clear == undefined) {
                    $scope.CreateTax();
                }
                else {
                    $scope.Voucher.TaxInfo.Clear();
                }
            }

            $.each($scope.Voucher.voucheritems, function (k, v) {
                v.attributes.lguid = $scope.CreateUID();
                var ledger = $scope.allledgerlist.find(function (item) {
                    return item.id == v.ledgerid;
                });
                var taxable = v.amount;
                if (ledger && ledger.attributes &&
                    ledger.attributes.hasgst &&
                    (!$scope.Voucher.attributes.dgst)) {

                    taxledger = ledger;
                    taxledgerentry = v;

                    if (taxledger && taxledger.attributes && taxledger.attributes != null && taxledgerentry) {
                        if ($scope.Voucher.isinterstate) {
                            var igstledger = $scope.allledgerlist.find(function (item) {
                                return item.id == taxledger.attributes.ledgerigstid;
                            });
                            v.taxpercentage = igstledger.taxpercentage;
                            $scope.Voucher.TaxInfo.TaxLedgers.push({
                                'ledgerid': igstledger.id, 'taxable': taxable,
                                'taxpercentage': igstledger.taxpercentage,
                                'taxtype': 'IGST', 'isactive': true,
                                'iscredit': false, 'hastax': true,
                                'attributes': {
                                    'ledgertype': igstledger.typeid,
                                    'hasgst': true,
                                    'lguid': v.attributes.lguid
                                },
                                'amount': Math.round(taxable * igstledger.taxpercentage / 100)
                            });
                        }
                        else {

                            var cgstledger = $scope.allledgerlist.find(function (item) {
                                return item.id == taxledger.attributes.ledgercgstid;
                            });
                            var sgstledger = $scope.allledgerlist.find(function (item) {
                                return item.id == taxledger.attributes.ledgersgstid;
                            });

                            v.taxpercentage = 0;
                            if (cgstledger && cgstledger != null) {
                                $scope.Voucher.TaxInfo.TaxLedgers.push({
                                    'ledgerid': cgstledger.id, 'taxable': taxable,
                                    'taxpercentage': cgstledger.taxpercentage, 'taxtype': 'CGST',
                                    'iscredit': false, 'hastax': true, 'isactive': true,
                                    'attributes': {
                                        'ledgertype': cgstledger.typeid,
                                        'hasgst': true,
                                        'lguid': v.attributes.lguid
                                    },
                                    'amount': Math.round(taxable * cgstledger.taxpercentage / 100)
                                });
                                v.taxpercentage += cgstledger.taxpercentage;
                            }

                            if (sgstledger && sgstledger != null) {
                                $scope.Voucher.TaxInfo.TaxLedgers.push({
                                    'ledgerid': sgstledger.id, 'taxable': taxable,
                                    'taxpercentage': sgstledger.taxpercentage, 'taxtype': 'SGST',
                                    'iscredit': false, 'hastax': true, 'isactive': true,
                                    'attributes': {
                                        'ledgertype': sgstledger.typeid,
                                        'hasgst': true,
                                        'lguid': v.attributes.lguid
                                    },
                                    'amount': Math.round(taxable * sgstledger.taxpercentage / 100)
                                });
                                v.taxpercentage += sgstledger.taxpercentage;
                            }
                        }
                    }
                }
                v.taxable = 0;
                v.tds = 0;
                if (ledger.typeid != 16) {
                    totaltaxable += taxable;
                    taxableledger = (taxableledger == null ? v : (taxableledger.amount < v.amount ? v : taxableledger));
                }
            });

            var tdsvoucheritems = $scope.Voucher.voucheritems.find(function (item) {
                return item.attributes && item.attributes.hastds;
            });

            if (!$scope.Voucher.attributes.dtds &&
                (tdsvoucheritems == undefined || tdsvoucheritems == null)) {


                var hastds = ($scope.Voucher.vouchertype == 3 ||
                    $scope.Voucher.vouchertype == 1 ||
                    $scope.Voucher.vouchertype == 14);

                if (hastds) {
                    hastds = (partyledger && partyledger.attributes
                        && partyledger.attributes.hastds);
                    if (!hastds)
                        hastds = tdsvoucheritems && tdsvoucheritems != null;
                }
                if (hastds) {

                    //$.each($scope.Voucher.TaxInfo.TaxLedgers, function (k, v) {
                    //    totaltaxable += v.amount;
                    //});

                    if (partyledger && partyledger.attributes
                        && partyledger.attributes.hastds) {
                        sledgerid = partyledger.ledgerid;
                        tdsledger = $scope.allledgerlist.find(function (item) {
                            return item.id == partyledger.attributes.ledgertdsid;
                        });
                    } else if (tdsvoucheritems && tdsvoucheritems != null) {
                        tdsledger = $scope.allledgerlist.find(function (item) {
                            return item.id == tdsvoucheritems.attributes.ledgertdsid;
                        });
                        sledgerid = tdsvoucheritems.ledgerid;
                    }

                    if (tdsledger && tdsledger != null) {
                        if (tdsledger.attributes && tdsledger.attributes != null &&
                            tdsledger.attributes.tdsrules && tdsledger.attributes.tdsrules != null &&
                            $scope.Voucher.TaxInfo.LedgerBalance != null) {
                            var rulemode = $scope.Voucher.vouchertype == 3 ? 1 : 2;

                            var singletransaction = tdsledger.attributes.tdsrules.find(function (item) {
                                return item.mode == rulemode && item.eligible == 1;
                            });

                            var totaltransaction = tdsledger.attributes.tdsrules.find(function (item) {
                                return item.mode == rulemode && item.eligible == 2;
                            });

                            var totalamount = ($scope.Voucher.TaxInfo.LedgerBalance.credit - $scope.Voucher.TaxInfo.LedgerBalance.debit);
                            var tdsamount = 0;

                            $scope.Voucher.TaxInfo.TDSTotalTransaction = totalamount;
                            $scope.Voucher.TaxInfo.TDSSingleValue = singletransaction ? singletransaction.value : 0;
                            $scope.Voucher.TaxInfo.TDSTotalValue = totaltransaction ? totaltransaction.value : 0;
                            $scope.Voucher.TaxInfo.TDSExist = true;
                            $scope.Voucher.TaxInfo.TDSReason = '';
                            if (singletransaction == undefined && totaltransaction == undefined) {
                                tdsamount = Math.round(totaltaxable * -1 * tdsledger.taxpercentage / 100);
                                $scope.Voucher.TaxInfo.TDSReason = 'All Transaction';
                            }
                            else if (singletransaction && singletransaction != null &&
                                singletransaction.value <= totaltaxable) {
                                tdsamount = Math.round(totaltaxable * -1 * tdsledger.taxpercentage / 100);
                                $scope.Voucher.TaxInfo.TDSReason = 'Crossed Single Transaction - ' + $scope.Voucher.TaxInfo.TDSSingleValue;
                            } else if (totaltransaction && totaltransaction != null &&
                                totaltransaction.value <= (totaltaxable + totalamount)) {
                                if (totaltransaction.value > totalamount)
                                    totaltaxable = (totaltaxable + totalamount) - totaltransaction.value;
                                tdsamount = Math.round(totaltaxable * -1 * tdsledger.taxpercentage / 100);
                                $scope.Voucher.TaxInfo.TDSReason = 'Crossed Total Transaction - ' + $scope.Voucher.TaxInfo.TDSTotalValue;
                            }

                            if (tdsamount != 0) {
                                $scope.Voucher.TaxInfo.TaxLedgers.push({
                                    'ledgerid': tdsledger.id,
                                    'taxable': totaltaxable,
                                    'taxpercentage': tdsledger.taxpercentage,
                                    'sledgerid': taxableledger != null ? taxableledger.ledgerid : 0,
                                    'taxtype': 'TDS', 'isactive': true,
                                    'iscredit': false, 'hastax': true,
                                    'attributes': {
                                        'ledgertype': tdsledger.typeid,
                                        'hasgst': false,
                                        'hastds': true
                                    },
                                    'amount': tdsamount
                                });
                                if (taxableledger != null) {
                                    if (taxableledger.tds == undefined) taxableledger.tds = 0;
                                    taxableledger.tds += tdsamount;
                                }
                            }

                        }
                        else if (totaltaxable != 0) {
                            var tdsamount = Math.round(totaltaxable * -1 * tdsledger.taxpercentage / 100);
                            $scope.Voucher.TaxInfo.TaxLedgers.push({
                                'ledgerid': tdsledger.id,
                                'taxable': totaltaxable,
                                'taxpercentage': tdsledger.taxpercentage,
                                'sledgerid': taxableledger != null ? taxableledger.ledgerid : 0,
                                'taxtype': 'TDS', 'isactive': true,
                                'iscredit': false, 'hastax': true,
                                'attributes': {
                                    'ledgertype': tdsledger.typeid,
                                    'hasgst': false,
                                    'hastds': true
                                },
                                'amount': tdsamount
                            });
                            if (taxableledger != null) {
                                if (taxableledger.tds == undefined) taxableledger.tds = 0;
                                taxableledger.tds += tdsamount;
                            }
                        }
                    }
                }
            }
            else if (
                !$scope.Voucher.attributes.dtds &&
                ($scope.Voucher.vouchertype == 1 || $scope.Voucher.vouchertype == 3) &&
                tdsvoucheritems &&
                tdsvoucheritems.attributes.ledgertdsid &&
                tdsvoucheritems.attributes.ledgertdsid > 0) {

                tdsledger = $scope.allledgerlist.find(function (item) {
                    return item.id == tdsvoucheritems.attributes.ledgertdsid;
                });

                if (tdsledger && tdsledger != null) {
                    if (tdsledger.attributes && tdsledger.attributes != null &&
                        tdsledger.attributes.tdsrules && tdsledger.attributes.tdsrules != null) {

                        var rulemode = $scope.Voucher.vouchertype == 3 ? 1 : 2;

                        var singletransaction = tdsledger.attributes.tdsrules.find(function (item) {
                            return item.mode == rulemode && item.eligible == 1;
                        });

                        var totaltransaction = tdsledger.attributes.tdsrules.find(function (item) {
                            return item.mode == rulemode && item.eligible == 2;
                        });

                        var tdsamount = 0;


                        $scope.Voucher.TaxInfo.TDSSingleValue = singletransaction ? singletransaction.value : 0;
                        $scope.Voucher.TaxInfo.TDSTotalValue = totaltransaction ? totaltransaction.value : 0;
                        $scope.Voucher.TaxInfo.TDSExist = true;
                        $scope.Voucher.TaxInfo.TDSReason = '';
                        if (singletransaction == undefined && totaltransaction == undefined) {
                            tdsamount = Math.round(totaltaxable * -1 * tdsledger.taxpercentage / 100);
                            $scope.Voucher.TaxInfo.TDSReason = 'All Transaction';
                        }
                        else if (singletransaction && singletransaction != null &&
                            singletransaction.value <= totaltaxable) {
                            tdsamount = Math.round(totaltaxable * -1 * tdsledger.taxpercentage / 100);
                            $scope.Voucher.TaxInfo.TDSReason = 'Crossed Single Transaction - ' + $scope.Voucher.TaxInfo.TDSSingleValue;
                        } else if (totaltransaction && totaltransaction != null) {

                            if (tdsvoucheritems.attributes &&
                                tdsvoucheritems.attributes != null &&
                                tdsvoucheritems.attributes.ledgertdsid &&
                                tdsvoucheritems.attributes.ledgertdsid > 0) {

                                var ledger = $scope.allledgerlist.find(function (item) {
                                    return item.id == tdsvoucheritems.ledgerid;
                                });

                                var request = {
                                    "item": {
                                        'voucherdate': $scope.Voucher.voucherdate,
                                        'ledgerid': ledger.id,
                                        'secondaryid': ledger.supplierid
                                    },
                                    "securitytoken": "String content"
                                };
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: 'VoucherService/LedgerBalance',
                                    data: srequest
                                }).success(function (result) {
                                    $scope.Voucher.TaxInfo.LedgerBalance = result;
                                    var totalamount = ($scope.Voucher.TaxInfo.LedgerBalance.credit - $scope.Voucher.TaxInfo.LedgerBalance.debit);
                                    $scope.Voucher.TaxInfo.TDSTotalTransaction = totalamount;
                                    if (totaltransaction.value <= (totaltaxable + totalamount)) {
                                        if (totaltransaction.value > totalamount)
                                            totaltaxable = (totaltaxable + totalamount) - totaltransaction.value;
                                        tdsamount = Math.round(totaltaxable * -1 * tdsledger.taxpercentage / 100);
                                        $scope.Voucher.TaxInfo.TDSReason = 'Crossed Total Transaction - ' + $scope.Voucher.TaxInfo.TDSTotalValue;
                                        if (tdsamount != 0) {
                                            $scope.Voucher.TaxInfo.TaxLedgers.push({
                                                'ledgerid': tdsledger.id,
                                                'taxable': totaltaxable,
                                                'taxpercentage': tdsledger.taxpercentage,
                                                'sledgerid': taxableledger != null ? taxableledger.ledgerid : 0,
                                                'taxtype': 'TDS', 'isactive': true,
                                                'iscredit': false, 'hastax': true,
                                                'attributes': {
                                                    'ledgertype': tdsledger.typeid,
                                                    'hasgst': false,
                                                    'hastds': true
                                                },
                                                'amount': tdsamount
                                            });
                                            if (tdsvoucheritems != null) {
                                                if (tdsvoucheritems.tds == undefined) tdsvoucheritems.tds = 0;
                                                tdsvoucheritems.tds += tdsamount;
                                            }
                                            if ($scope.Voucher.TaxInfo)
                                                $scope.Voucher.TaxInfo.UpdateTax();
                                        }
                                    }
                                });
                            }
                            return;
                        }

                        if (tdsamount != 0) {
                            $scope.Voucher.TaxInfo.TaxLedgers.push({
                                'ledgerid': tdsledger.id,
                                'taxable': totaltaxable,
                                'taxpercentage': tdsledger.taxpercentage,
                                'sledgerid': taxableledger != null ? taxableledger.ledgerid : 0,
                                'taxtype': 'TDS', 'isactive': true,
                                'iscredit': false, 'hastax': true,
                                'attributes': {
                                    'ledgertype': tdsledger.typeid,
                                    'hasgst': false,
                                    'hastds': true
                                },
                                'amount': tdsamount
                            });
                            if (taxableledger != null) {
                                if (taxableledger.tds == undefined) taxableledger.tds = 0;
                                taxableledger.tds += tdsamount;
                            }
                        }

                    }
                    else if (totaltaxable != 0) {
                        var tdsamount = Math.round(totaltaxable * -1 * tdsledger.taxpercentage / 100);
                        $scope.Voucher.TaxInfo.TaxLedgers.push({
                            'ledgerid': tdsledger.id,
                            'taxable': totaltaxable,
                            'taxpercentage': tdsledger.taxpercentage,
                            'sledgerid': taxableledger != null ? taxableledger.ledgerid : 0,
                            'taxtype': 'TDS', 'isactive': true,
                            'iscredit': false, 'hastax': true,
                            'attributes': {
                                'ledgertype': tdsledger.typeid,
                                'hasgst': false,
                                'hastds': true
                            },
                            'amount': tdsamount
                        });
                        if (taxableledger != null) {
                            if (taxableledger.tds == undefined) taxableledger.tds = 0;
                            taxableledger.tds += tdsamount;
                        }
                    }
                }
            }
            if ($scope.Voucher.TaxInfo)
                $scope.Voucher.TaxInfo.UpdateTax();
        }


        $scope.SetInterstate = function () {
            var adjustmentledger = $scope.adjustmentlist.find(function (item) {
                return item.id == $scope.Voucher.adjustmentledgerid;
            });
            var company = $scope.Company.find(function (item) {
                return item.id == $scope.Voucher.companyid;
            });
            $scope.Voucher.isinterstate = false;
            if ($scope.Voucher.TaxInfo == undefined || $scope.Voucher.TaxInfo == null)
                $scope.Voucher.TaxInfo = {};
            $scope.Voucher.TaxInfo.LedgerBalance = null;

            if (adjustmentledger == undefined ||
                company == undefined)
                return;

            if (adjustmentledger.attributes &&
                adjustmentledger.attributes != null &&
                adjustmentledger.attributes.ledgertdsid &&
                adjustmentledger.attributes.ledgertdsid > 0) {
                var request = {
                    "item": {
                        'voucherdate': $scope.Voucher.voucherdate,
                        'ledgerid': $scope.Voucher.adjustmentledgerid,
                        'secondaryid': $scope.Voucher.supplierid
                    },
                    "securitytoken": "String content"
                };
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: 'VoucherService/LedgerBalance',
                    data: srequest
                }).success(function (result) {
                    $scope.Voucher.TaxInfo.LedgerBalance = result;
                });
            }

            if (adjustmentledger &&
                company &&
                adjustmentledger != null &&
                company != null) {
                if (company.tinanddate &&
                    adjustmentledger.gstno &&
                    company.tinanddate != null &&
                    adjustmentledger.gstno != null &&
                    company.tinanddate != '' &&
                    adjustmentledger.gstno != '') {
                    $scope.Voucher.isinterstate = company.tinanddate.substr(0, 2) != adjustmentledger.gstno.substr(0, 2);
                }
            }
        }

        $scope.AddItem = function () {

            var amount = parseFloat($scope.Voucheritems.amount);
            var rate = parseFloat($scope.Voucheritems.rate);
            var qty = parseFloat($scope.Voucheritems.qty);

            if (isNaN(amount)) {
                $scope.Voucheritems.amount = '';
                $rootScope.Toaster.Info("Please enter the amount");
                return;
            }
            var ledger = $scope.allledgerlist.find(function (item) {
                return item.id == $scope.Voucheritems.ledgerid;
            });

            if (typeof (ledger) == 'undefined' || ledger == null) {
                $rootScope.Toaster.Info("Please select the ledger");
                return;
            }

            if (ledger.typeid == 16) {
                var existing = $scope.Voucher.voucheritems.find(function (item) {
                    return item.ledgerid == $scope.Voucheritems.ledgerid;
                });

                if (existing && existing != null) {
                    $rootScope.Toaster.Info(ledger.displayname + " already added.");
                    return;
                }

                if (ledger.taxmode == 3 ||
                    ledger.taxmode == 4 ||
                    ledger.taxmode == 5) {

                    if ($scope.Voucher.voucheritems.length == 0) {
                        $rootScope.Toaster.Info(" Tax can't be added as first item");
                        return;
                    }

                    var existing = $scope.Voucher.voucheritems.find(function (item) {
                        var fledger = $scope.allledgerlist.find(function (litem) {
                            return litem.id == item.ledgerid;
                        });
                        if (fledger && fledger != null) {
                            if (ledger.taxmode == 3)
                                return fledger.typeid == 16 &&
                                    (fledger.taxmode == 4 || fledger.taxmode == 5);

                            return ledger.typeid == 16 &&
                                (fledger.taxmode == 3);
                        }
                        return false;
                    });


                    if (existing && existing != null) {
                        if (!$scope.Voucher.isinterstate && ledger.taxmode == 3) {
                            $rootScope.Toaster.Info(" IGST can't add while CGST/SGST exist");
                            return;
                        }
                        if ($scope.Voucher.isinterstate && ledger.taxmode == 3) {
                            $rootScope.Toaster.Info(" CGST/SGST can't add while IGST exist");
                            return;
                        }
                    }

                    //var existing = $scope.allledgerlist.find(function (item) {
                    //    return item.id == $scope.Voucher.adjustmentledgerid;
                    //});
                    //if (existing && existing != null) {
                    //    if (existing.isinterstate) {
                    //        if (ledger.taxmode == 4 || ledger.taxmode == 5) {
                    //            $rootScope.Toaster.Info(" Interstate Party ledger, No Support for CGST/SGST");
                    //            return;
                    //        }
                    //    }
                    //    else {
                    //        if (ledger.taxmode == 3) {
                    //            $rootScope.Toaster.Info(" Local Party ledger, No Support for IGST");
                    //            return;
                    //        }
                    //    }
                    //}
                    //else {
                    //    $rootScope.Toaster.Info(" Please select the party before add the taxes");
                    //    return;
                    //}
                }
            }

            var hasgst = (ledger.attributes && ledger.attributes != null && ledger.attributes.hasgst);
            var hastds = (ledger.attributes && ledger.attributes != null && ledger.attributes.hastds);
            var voucheritem = {
                'ledgerid': $scope.Voucheritems.ledgerid,
                'amount': amount,
                'rate': (isNaN(rate) ? 0 : rate),
                'qty': (isNaN(qty) ? 0 : qty),
                'attributes': {
                    'customerid': ledger.customerid,
                    'supplierid': ledger.supplierid,
                    'ledgertype': ledger.typeid,
                    'hasgst': hasgst,
                    'hastds': hastds,
                    'mantax': !hastds && !hasgst,
                    'ledgertdsid': (ledger.attributes && ledger.attributes != null) ? ledger.attributes.ledgertdsid : 0
                },
                'linehight': { 'height': (ledger.typeid == 7 ? '60px' : '30px'), 'colour': 'red' },
                'isactive': true,
                'iscredit': false,
                'version': 1,
                'bwise': ledger.billwise ? ledger.typeid : 0
            };
            $scope.Voucher.voucheritems.push(voucheritem);
            $scope.Voucheritems = (JSON.parse(JSON.stringify($scope.VoucherItemEntity)));
            if (!hastds) {
                hastds = $scope.Voucher && $scope.Voucher.attributes && $scope.Voucher.attributes.hastds
            }
            if ($scope.Voucher.HasTax || hasgst || hastds) {
                $scope.ComputeGSTTax();
            }
            $scope.Totalamount();
            $scope.LedgerGSTNo = '';
            $scope.ShowLedgerGSTNo = false;

            if (ledger.billwise && voucheritem.bwise > 0) {
                $scope.ShowBillwiseVoucherItem(voucheritem);
            }
            else {
                if ($scope.ItemwisePurchase) {
                    $scope['setFocusitemledgerid']();
                }
                else {
                    $scope['setFocusledgerid']();
                }
            }
        }

        $scope.IsPartyLedger = true;
        $scope.VoucherTypeChanged = function (loaditem, vouchertypeid) {
            if (loaditem) {
                $scope.Voucher.vouchertype = $scope.GetVoucherType(vouchertypeid);
                $scope.IsPartyLedger = $scope.Voucher.vouchertype == 14 || $scope.Voucher.vouchertype == 18 || $scope.Voucher.vouchertype == 33 || ($scope.Voucher.vouchertype >= 3 && $scope.Voucher.vouchertype <= 6);
                $scope.LoadItems();
                $scope.PurchaeTypeChanged();
                if ($scope.Voucher.id == undefined || $scope.Voucher.id == null ||
                    $scope.Voucher.id == '' || $scope.Voucher.id == 0) {
                    $scope.Voucher.attributes.dtds = false;
                    $scope.Voucher.attributes.dgst = false;
                    /*$scope.Voucher.attributes.dgst = $scope.Voucher.vouchertype == 1;
                    $scope.Voucher.attributes.dtds = $scope.Voucher.vouchertype == 1;*/
                    $scope.ComputeGSTTax();
                    $scope.Totalamount();
                }
            }
            else {
                $scope.SearchCondition.vouchertype = $scope.GetVoucherType(vouchertypeid);
            }
        }

        $scope.ItemwisePurchase = false;
        $scope.PurchaeTypeChanged = function () {
            $scope.ItemwisePurchase = false;
            var pt = $scope.PurchaseType.find(function (item) {
                return item.id == $scope.Voucher.purchasetypeid;
            });
            if (pt && pt.additionalinfo && pt.additionalinfo.voucherconfig) {
                $scope.ItemwisePurchase = pt.additionalinfo.voucherconfig.itemwise;
                if ($scope.Voucher.attributes == undefined ||
                    $scope.Voucher.attributes == null) {
                    $scope.Voucher.attributes = {};
                }
                $scope.Voucher.attributes.hasrcm = pt.additionalinfo.voucherconfig.gstrcm;
            }
            $scope.Totalamount();
        }

        $scope.Focusledger = function () {
            if ($scope.Voucher.vouchertype == 3 || $scope.Voucher.vouchertype == 33 || $scope.Voucher.vouchertype == 4) {
                $scope['setFocuspurchasetype']();
            }
            else {
                $scope['setFocusadjustmentledgerid']();
            }
        }

        $scope.VoucheritemsAmount = function () {
            var rate = $rootScope.GetNumber($scope.Voucheritems.rate);
            var qty = $rootScope.GetNumber($scope.Voucheritems.qty);
            if (isNaN(qty)) qty = 0;
            if (isNaN(rate)) rate = 0;
            $scope.Voucheritems.amount = (qty * rate).round(2);
        }

        $scope.Totalamount = function () {
            $scope.Voucher.amount = 0;

            if ($scope.Voucher.attributes == undefined ||
                $scope.Voucher.attributes == null)
                $scope.Voucher.attributes = {};
            $scope.Voucher.attributes.hasinv = false;

            $.each($scope.Voucher.voucheritems, function (k, v) {
                $scope.Voucher.amount = $scope.Voucher.amount + v.amount;
                if (v.attributes && v.attributes.ledgertype == 8)
                    $scope.Voucher.attributes.hasinv = true;
            });

            if ($scope.Voucher.TaxInfo && $scope.Voucher.TaxInfo.HasTax) {
                if (!($scope.Voucher.attributes && $scope.Voucher.attributes.hasrcm)) {
                    $scope.Voucher.amount += $scope.Voucher.TaxInfo.GSTTax;
                }
            }

            if ($scope.Voucher.TaxInfo && $scope.Voucher.TaxInfo.HasTax
                && $scope.Voucher.TaxInfo.TDSTax && $scope.Voucher.TaxInfo.TDSTax != 0) {
                $scope.Voucher.amount += $scope.Voucher.TaxInfo.TDSTax;
            }
        }

        $scope.AddNew = function (focus) {
            $scope.Voucher = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.CreateTax();
            $scope.Voucher.voucheritems = [];
            $scope.Voucheritems = (JSON.parse(JSON.stringify($scope.VoucherItemEntity)));
            $scope.Voucher.amount = 0;
            $scope.ShowNew = true;
            $scope.ShowPrint = false;
            $scope.PurchaeTypeChanged();
            $scope.Navigation.SetMode('EDIT');
            if (focus == undefined || focus == null) {
                $timeout(function () {
                    $scope.setFocusvouchertype();
                }, 10);
            }
        }

        $scope.Cancel = function () {
            $scope.ShowNew = !$scope.ShowNew;
            $scope.ShowPrint = false;
            $scope.SearchCondition.id = 0;
            $scope.Navigation.SetMode('SEARCH');
        }
        $scope.Save = function () {

            if (typeof ($scope.Voucher.voucherdate) == 'undefined' || $scope.Voucher.voucherdate == null || $scope.Voucher.voucherdate == '') {
                $rootScope.Toaster.Info("Please enter the voucher date");
                return;
            }

            if (typeof ($scope.Voucher.employeeid) == 'undefined' || $scope.Voucher.employeeid == null || $scope.Voucher.employeeid == 0) {
                $rootScope.Toaster.Info("Please select relevent employee");
                return;
            }

            if (typeof ($scope.Voucher.companyid) == 'undefined' || $scope.Voucher.companyid == null || $scope.Voucher.companyid == 0) {
                $rootScope.Toaster.Info("Please select relevent company");
                return;
            }

            if (typeof ($scope.Voucher.locationid) == 'undefined' || $scope.Voucher.locationid == null || $scope.Voucher.locationid == 0) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }

            if (typeof ($scope.Voucher.vouchertype) == 'undefined' || $scope.Voucher.vouchertype == null) {
                $rootScope.Toaster.Info("Please select voucher type");
                return;
            }

            if (typeof ($scope.Voucher.adjustmentledgerid) == 'undefined' || $scope.Voucher.adjustmentledgerid == null || $scope.Voucher.adjustmentledgerid == 0) {
                $rootScope.Toaster.Info("Please select party name");
                return;
            }


            if ($scope.Voucher.vouchertype == 9) {
                if (typeof ($scope.saledatefrom) == 'undefined' || $scope.saledatefrom == null || $scope.saledatefrom == '') {
                    $rootScope.Toaster.Info("Please enter the sale date");
                    return;
                }

                if (typeof ($scope.Voucher.attributes) == 'undefined' || $scope.Voucher.attributes == null)
                    $scope.Voucher.attributes = {};

                if (typeof ($scope.Voucher.attributes.saledatefrom) == 'undefined' ||
                    $scope.Voucher.attributes.saledatefrom == null || $scope.Voucher.attributes.saledatefrom == '') {
                    $rootScope.Toaster.Info("Please enter the sale date");
                    return;
                }
            }

            if ($scope.Voucher.vouchertype == 1 &&
                $scope.AdjustmentLedgerType == 7 &&
                $scope.Voucher.attributes.banktransfermode &&
                $scope.Voucher.attributes.banktransfermode == 2) {
                if (typeof ($scope.Voucher.attributes.bankdate) == 'undefined' ||
                    $scope.Voucher.attributes.bankdate == null ||
                    $scope.Voucher.attributes.bankdate == '' ||
                    $scope.Voucher.attributes.bankdate == '01-01-1901') {
                    $rootScope.Toaster.Info("Please enter the cheque date");
                    return;
                }

                if (typeof ($scope.Voucher.attributes.bankdate) == 'object' &&
                    new Date($scope.Voucher.attributes.bankdate).getFullYear() < 2000) {
                    $scope.Voucher.attributes.bankdate = '';
                    $rootScope.Toaster.Info("Please enter the cheque date");
                    return;
                }
            }

            if (typeof ($scope.Voucher.amount) == 'undefined' || $scope.Voucher.amount == null || $scope.Voucher.amount == 0) {
                $rootScope.Toaster.Info("Please enter the amount");
                return;
            }

            if (typeof ($scope.Voucher.voucheritems) == 'undefined' || $scope.Voucher.voucheritems == null || $scope.Voucher.voucheritems.length == 0) {
                $rootScope.Toaster.Info("Please add ledgers to voucher");
                return;
            }

            var isupdate = $scope.Voucher.id && $scope.Voucher.id > 0;

            if (isupdate && $scope.Voucher.attributes &&
                $scope.Voucher.attributes.adjustedvouchervalue &&
                $scope.Voucher.amount &&
                $scope.Voucher.attributes.adjustedvouchervalue > 0) {
                if ($scope.Voucher.amount < $scope.Voucher.attributes.adjustedvouchervalue) {
                    $rootScope.Toaster.Error("Voucher already adjusted with Vouchers of (" + $scope.Voucher.attributes.adjustedvouchers + "), Amount - " + $scope.Voucher.attributes.adjustedvouchervalue + '( Minimum Value)');
                    return;
                }
            }

            if ($scope.Voucher.attributes.hasinv && ($scope.Voucher.vouchertype == 3 || $scope.Voucher.vouchertype == 4)) {
                if ($scope.Voucher.attributes == undefined || $scope.Voucher.attributes.invledgerid == undefined ||
                    $scope.Voucher.attributes.invledgerid == '' || $scope.Voucher.attributes.invledgerid == 0) {
                    if ($scope.Voucher.vouchertype == 4)
                        $rootScope.Toaster.Info("Please select the sale ledgers for the voucher");
                    else
                        $rootScope.Toaster.Info("Please select the purchase ledgers for the voucher");
                    return;
                }
            }

            var billwise = $scope.Voucher.bwise &&
                $scope.Voucher.bwise != null &&
                $scope.Voucher.bwise != '' &&
                $scope.Voucher.bwise > 0;

            //if (billwise == undefined || !billwise) {
            //    var billwiseledger = $scope.Voucher.voucheritems.find(function (item) {
            //        return item.bwise && item.bwise > 0;
            //    });
            //    billwise = billwiseledger && billwiseledger != null;
            //}

            if (($scope.Voucher.vouchertype == 1 || $scope.Voucher.vouchertype == 2) && billwise) {
                $scope.ShowBillwiseSettlement();
            } else if (($scope.Voucher.vouchertype == 5 || $scope.Voucher.vouchertype == 6) && billwise) {
                $scope.ShowBillwiseSettlement();
            } else if (($scope.Voucher.bwise == 11 || $scope.Voucher.bwise == 12) &&
                ($scope.Voucher.vouchertype == 3 || $scope.Voucher.vouchertype == 4)) {
                $scope.ShowAdvance();
            }
            else {
                var validationmsg = $scope.TdsRuleEngine.Validate($scope.Voucher);
                if (validationmsg == '') {
                    $scope.SaveInternal();
                }
                else {
                    $scope.ShowTDSValidation(validationmsg);
                }
            }
        }

        $scope.ShowAdvance = function () {
            var IsPayable = $scope.Voucher.bwise == 12;
            $scope.Voucher.onadvance = true;
            var url = IsPayable ? "VoucherService/Payable" : "VoucherService/Receivable";
            var request = {
                "item": $scope.Voucher,
                "securitytoken": "String content"
            };

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: url,
                data: srequest
            }).success(function (results) {
                if (results.length > 0) {
                    $scope.ShowAdvanceSettlement(results);
                }
                else {
                    var validationmsg = $scope.TdsRuleEngine.Validate($scope.Voucher);
                    if (validationmsg == '') {
                        $scope.SaveInternal();
                    }
                    else {
                        $scope.ShowTDSValidation(validationmsg);
                    }
                }
            });
        }

        $scope.ShowTDSValidation = function (msg) {

            $.confirm({
                title: 'TDS',
                content: 'TDS missing, Do you want to Proceed?<br>' + msg,
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
                            $scope.SaveInternal();
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

        $scope.ShowAdvanceSettlement = function (results) {
            var dialog = ngDialog.open({
                template: 'BillwiseAdvance',
                data: [$scope, results],
                width: '750px',
                controller: ['$scope', '$timeout', '$rootScope', 'restService', '$http',
                    function ($scope, $timeout, $rootScope, restService, $http) {
                        $scope.ParentScope = $scope.ngDialogData[0];
                        $scope.PopupList = $scope.ngDialogData[1];
                        $scope.Adjustable = $scope.ParentScope.Voucher.amount;
                        $scope.Available = 0;
                        $scope.Adjusted = 0;
                        $scope.Advance = 0;
                        $scope.MinimumAdvance = 0;
                        $scope.AdvanceLedger = null;
                        $.each($scope.PopupList, function (k, v) {
                            $scope.Available += (v.amount - (v.adjusted + v.paid));
                            v.onadvance = true;
                            v.balance = (v.amount - (v.adjusted + v.paid));
                            $scope.Adjusted += v.paid;
                        });
                        $scope.paidselected = function (item) {
                            item.paid = item.balance;
                            $scope.paidchanged();
                        }
                        $scope.paidchanged = function () {
                            $scope.Adjusted = 0;
                            $.each($scope.PopupList, function (k, v) {
                                var paid = parseFloat(v.paid);
                                if (isNaN(paid)) paid = 0;
                                v.balance = v.amount - (v.adjusted + paid);
                                if (v.balance < 0) {
                                    paid = 0;
                                    v.paid = paid;
                                    v.balance = v.amount - (v.adjusted + v.paid);
                                }
                                $scope.Adjusted += paid;
                            });
                        }
                        $scope.Selected = function () {
                            var returnlist = [];
                            $.each($scope.PopupList, function (k, v) {
                                var paid = parseFloat(v.paid);
                                if (isNaN(paid)) paid = 0;
                                if (paid > 0) {
                                    var paiditem = (JSON.parse(JSON.stringify(v)));
                                    paiditem.paid = paid;
                                    returnlist.push(paiditem);
                                }
                            });
                            $scope.closeThisDialog(returnlist);
                        };
                    }]
            });
            dialog.closePromise.then(function (data) {
                {
                    if (!(typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')) {
                        $scope.Voucher.billadjustement = data.value;
                        var validationmsg = $scope.TdsRuleEngine.Validate($scope.Voucher);
                        if (validationmsg == '') {
                            $scope.SaveInternal();
                        }
                        else {
                            $scope.ShowTDSValidation(validationmsg);
                        }
                    }
                }
            });
        }

        $scope.ShowBillwiseVoucherItem = function (voucheritem) {
            var dialog = ngDialog.open({
                template: 'BillwiseSettlement',
                data: [$scope, voucheritem],
                width: '750px',
                controller: ['$scope', '$timeout', '$rootScope', 'restService', '$http',
                    function ($scope, $timeout, $rootScope, restService, $http) {
                        $scope.ParentScope = $scope.ngDialogData[0];
                        $scope.VoucherItem = $scope.ngDialogData[1];
                        $scope.Voucher = $scope.ParentScope.Voucher;
                        $scope.PopupList = [];
                        $scope.Adjustable = $scope.VoucherItem.amount;
                        $scope.Available = 0;
                        $scope.Adjusted = 0;
                        $scope.ShowAdvance = false;
                        $scope.Advance = 0;
                        $scope.MinimumAdvance = 0;
                        $scope.AdvanceLedger = null;
                        $scope.ParentScope.Voucher.onadvance = false;
                        $scope.IsPayable = $scope.VoucherItem.bwise == 12;

                        var url = $scope.IsPayable ? "VoucherService/Payable" : "VoucherService/Receivable";
                        request = {
                            "item": {
                                'adjustmentledgerid': $scope.VoucherItem.ledgerid,
                                'voucherdate': $scope.ParentScope.Voucher.voucherdate,
                                'id': $scope.ParentScope.Voucher.id,
                                'bwise': $scope.VoucherItem.bwise
                            },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: url,
                            data: srequest
                        }).success(function (results) {
                            $scope.PopupList = [];
                            $.each(results, function (k, v) {
                                if (v.onadvance) {
                                    $scope.AdvanceLedger = v;
                                    $scope.MinimumAdvance = v.adjusted;
                                }
                                else {
                                    v.balance = (v.amount - (v.adjusted + v.paid));
                                    $scope.Available += v.balance + v.paid;
                                    $scope.PopupList.push(v);
                                }
                            });
                            if ($scope.Available < $scope.Adjustable) {
                                $scope.ShowAdvance = true;
                                $scope.Advance = $scope.Adjustable - $scope.Available;
                            }
                            if ($scope.MinimumAdvance > $scope.Advance)
                                $scope.Advance = $scope.MinimumAdvance;
                            $scope.paidchanged();
                        });

                        $scope.paidselected = function (item) {
                            item.paid = item.balance;
                            $scope.paidchanged();
                        }

                        $scope.paidchanged = function () {
                            $scope.Adjusted = $scope.Advance;
                            $.each($scope.PopupList, function (k, v) {
                                var paid = parseFloat(v.paid);
                                if (isNaN(paid)) paid = 0;
                                v.balance = v.amount - (v.adjusted + paid);
                                if (v.balance < 0) {
                                    paid = 0;
                                    v.paid = paid;
                                    v.balance = v.amount - (v.adjusted + v.paid);
                                }
                                $scope.Adjusted += paid;
                            });
                        }
                        $scope.Selected = function () {
                            var returnlist = [];
                            $.each($scope.PopupList, function (k, v) {
                                var paid = parseFloat(v.paid);
                                if (isNaN(paid)) paid = 0;
                                if (paid > 0) {
                                    var paiditem = (JSON.parse(JSON.stringify(v)));
                                    paiditem.paid = paid;
                                    returnlist.push(paiditem);
                                }
                            });

                            if ($scope.Advance > 0) {
                                returnlist.push({
                                    'onadvance': true, 'Referencetype': 'On Advance',
                                    'isactive': true, 'paid': $scope.Advance,
                                    'id': $scope.AdvanceLedger != null ? $scope.AdvanceLedger.id : 0,
                                    'ledgerid': $scope.ParentScope.Voucher.adjustmentledgerid
                                })
                            }
                            $scope.closeThisDialog(returnlist);
                        };
                    }]
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;
                    voucheritem.billadjustement = data.value;
                }
            });
        }

        $scope.ShowBillwiseSettlement = function () {
            var dialog = ngDialog.open({
                template: 'BillwiseSettlement',
                data: [$scope],
                width: '750px',
                controller: ['$scope', '$timeout', '$rootScope', 'restService', '$http',
                    function ($scope, $timeout, $rootScope, restService, $http) {
                        $scope.ParentScope = $scope.ngDialogData[0];
                        $scope.Voucher = $scope.ParentScope.Voucher;
                        $scope.PopupList = [];
                        $scope.Adjustable = $scope.ParentScope.Voucher.amount;
                        $scope.Available = 0;
                        $scope.Adjusted = 0;
                        $scope.ShowAdvance = false;
                        $scope.Advance = 0;
                        $scope.MinimumAdvance = 0;
                        $scope.AdvanceLedger = null;
                        $scope.ParentScope.Voucher.onadvance = false;
                        $scope.IsPayable = $scope.ParentScope.Voucher.bwise == 12;

                        /*$scope.IsPayable = false;
                        if ($scope.Voucher.bwise == 0) {
                            var payableledger = $scope.Voucher.voucheritems.find(function (item) {
                                return item.bwise == 12;
                            });
                            var receivableledger = $scope.Voucher.voucheritems.find(function (item) {
                                return item.bwise == 11;
                            })
                            $scope.IsPayable = payableledger && payableledger != null;
                        } else {
                            $scope.IsPayable = $scope.Voucher.bwise = 12;
                        }*/

                        var url = $scope.IsPayable ? "VoucherService/Payable" : "VoucherService/Receivable";
                        request = {
                            "item": $scope.ParentScope.Voucher,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: url,
                            data: srequest
                        }).success(function (results) {
                            $scope.PopupList = [];
                            $.each(results, function (k, v) {
                                if (v.onadvance) {
                                    $scope.AdvanceLedger = v;
                                    $scope.MinimumAdvance = v.adjusted;
                                }
                                else {
                                    v.balance = (v.amount - (v.adjusted + v.paid));
                                    $scope.Available += v.balance + v.paid;
                                    $scope.PopupList.push(v);
                                }
                            });
                            if ($scope.Available < $scope.Adjustable) {
                                $scope.ShowAdvance = true;
                                $scope.Advance = $scope.Adjustable - $scope.Available;
                            }
                            if ($scope.MinimumAdvance > $scope.Advance)
                                $scope.Advance = $scope.MinimumAdvance;
                            $scope.paidchanged();
                        });

                        $scope.paidselected = function (item) {
                            item.paid = item.balance;
                            $scope.paidchanged();
                        }

                        $scope.paidchanged = function () {
                            $scope.Adjusted = $scope.Advance;
                            $.each($scope.PopupList, function (k, v) {
                                var paid = parseFloat(v.paid);
                                if (isNaN(paid)) paid = 0;
                                v.balance = v.amount - (v.adjusted + paid);
                                if (v.balance < 0) {
                                    paid = 0;
                                    v.paid = paid;
                                    v.balance = v.amount - (v.adjusted + v.paid);
                                }
                                $scope.Adjusted += paid;
                            });
                        }
                        $scope.Selected = function () {
                            var returnlist = [];
                            $.each($scope.PopupList, function (k, v) {
                                var paid = parseFloat(v.paid);
                                if (isNaN(paid)) paid = 0;
                                if (paid > 0) {
                                    var paiditem = (JSON.parse(JSON.stringify(v)));
                                    paiditem.paid = paid;
                                    returnlist.push(paiditem);
                                }
                            });

                            if ($scope.Advance > 0) {
                                returnlist.push({
                                    'onadvance': true, 'Referencetype': 'On Advance',
                                    'isactive': true, 'paid': $scope.Advance,
                                    'id': $scope.AdvanceLedger != null ? $scope.AdvanceLedger.id : 0,
                                    'ledgerid': $scope.ParentScope.Voucher.adjustmentledgerid
                                })
                            }
                            $scope.closeThisDialog(returnlist);
                        };
                    }]
            });
            dialog.closePromise.then(function (data) {
                {
                    if (!(typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')) {
                        $scope.Voucher.billadjustement = data.value;
                        var validationmsg = $scope.TdsRuleEngine.Validate($scope.Voucher);
                        if (validationmsg == '') {
                            $scope.SaveInternal();
                        }
                        else {
                            $scope.ShowTDSValidation(validationmsg);
                        }
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

        $scope.SaveInternal = function () {
            $scope.Voucher.adjustmentcredit = $scope.Voucher.crdr == 1;

            if ($scope.Voucher.TaxInfo && $scope.Voucher.TaxInfo.HasTax) {
                $.each($scope.Voucher.TaxInfo.TaxLedgers, function (k, v) {
                    $scope.Voucher.voucheritems.push(v);
                });
            }

            var addedbilladjusted = [];
            $.each($scope.Voucher.voucheritems, function (k, v) {
                v.iscredit = !$scope.Voucher.adjustmentcredit;
                if (v.amount < 0) {
                    v.amount = Math.abs(v.amount);
                    v.iscredit = $scope.Voucher.adjustmentcredit;
                }

                if (v.billadjustement && v.billadjustement != null) {
                    if ($scope.Voucher.billadjustement == undefined ||
                        $scope.Voucher.billadjustement == null) {
                        $scope.Voucher.billadjustement = [];
                    }
                    $.each(v.billadjustement, function (kk, vv) {
                        addedbilladjusted.push(vv);
                        $scope.Voucher.billadjustement.push(vv);
                    })
                }
            });

            if ($scope.Voucher.id == undefined ||
                $scope.Voucher.id == null ||
                $scope.Voucher.id == '' ||
                $scope.Voucher.id == 0) {
                $scope.Voucher.uniqueuid = $scope.Voucher.uniqueuid == undefined ? $scope.CreateUID() : $scope.Voucher.uniqueuid;
            }

            request = {
                "item": $scope.Voucher,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $.each($scope.Voucher.voucheritems, function (k, v) {
                if (v.iscredit == $scope.Voucher.adjustmentcredit) {
                    v.amount = v.amount * -1;
                    v.iscredit = !$scope.Voucher.adjustmentcredit;
                }
            });


            $http({
                method: 'POST',
                url: "VoucherService/Save",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.LastVoucher = result;
                    $scope.LastVoucher.crdr = $scope.Voucher.crdr;
                    $scope.Voucher.id = 0;
                    delete $scope.Voucher.uniqueuid;
                    $scope.Voucher.TaxInfo = {
                        'HasTax': false
                    };
                    $scope.Voucher.voucheritems = [];
                    $scope.Voucher.amount = 0;
                    $scope.Voucher.narration = '';
                    $scope['setFocusadjustmentledgerid']();
                }
                else {
                    $.each(addedbilladjusted, function (k, v) {
                        $scope.Voucher.billadjustement.splice($scope.Voucher.billadjustement.indexOf(v), 1);
                    });
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });
        }

        $scope.ManageItem = function (action, item) {
            if (action == "Delete") {
                $scope.Voucher.voucheritems.splice($scope.Voucher.voucheritems.indexOf(item), 1);
            }
            else if (action == "Edit") {
                $scope.Voucheritems = item;
                $scope.Voucher.voucheritems.splice($scope.Voucher.voucheritems.indexOf(item), 1);
            }
            $scope.ComputeGSTTax();
            $scope.Totalamount();
        }

        $scope.ManageSearchItem = function (action, item) {

            if (action == "Delete") {

                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.ledger + "?",
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
                                var srequest = JSON.stringify(request, function (key, value) {
                                    if (key == "child") return undefined;
                                    else if (key == "grps") return undefined;
                                    else return value;
                                });
                                $http({
                                    method: 'POST',
                                    url: "VoucherService/Delete",
                                    data: srequest
                                }).success(function (result) {
                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $.each(item.child, function (k, v) {
                                            $scope.SearchResult.splice($scope.SearchResult.indexOf(v), 1);
                                        })
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

                });


            }
            else if (action == "Edit") {
                $timeout(function () {
                    $scope.ShowNew = true;
                    $scope.Search(item.id);
                    $scope.Navigation.SetMode('EDIT');
                }, 50);
            }
        }
        $scope.SearchCondition = {
            id: 0,
            vouchertype: 0,
            voucherdate: new Date(),
        }
        $scope.Search = function (id) {
            $scope.PrintCount = 0;
            $scope.ShowNew = false;
            $scope.SearchCondition.showdetail = true;
            if (typeof (id) != 'undefined')
                $scope.SearchCondition.id = id;
            request = {
                "Item": $scope.SearchCondition,
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "VoucherService/Search",
                data: srequest
            }).success(function (result) {
                if (typeof (id) != 'undefined' && result.length == 1) {
                    $scope.LoadItems(result[0].vouchertype);
                    $scope.ShowNew = true;
                    $scope.VoucherGSTNo = '';
                    $scope.ShowVoucherGSTNo = false;
                    $scope.Voucher = { 'adjustmentledgerid': 0 };
                    $timeout(function () {
                        $scope.Voucher = result[0];

                        if (result[0].vouchertypeid == undefined ||
                            result[0].vouchertypeid == null ||
                            result[0].vouchertypeid <= 0) {
                            $scope.Voucher.vouchertypeid = $scope.Voucher.vouchertype * -1;
                        }

                        var billadjustement = [];
                        $.each($scope.Voucher.voucheritems, function (k, v) {
                            if (v.iscredit == $scope.Voucher.adjustmentcredit) {
                                v.amount = v.amount * -1;
                                //v.iscredit = !$scope.Voucher.adjustmentcredit;
                            }
                        });

                        $scope.CreateTax();
                        $scope.AdjustmentLedgerType = -1;
                        var adjustmentledger = $scope.adjustmentlist.find(function (item) {
                            return item.id == $scope.Voucher.adjustmentledgerid;
                        });
                        if (adjustmentledger && adjustmentledger != null) {
                            $scope.AdjustmentLedgerType = adjustmentledger.typeid;
                            $scope.VoucherGSTNo = adjustmentledger.gstno;
                            $scope.ShowVoucherGSTNo = adjustmentledger.gstno && adjustmentledger.gstno != null && adjustmentledger.gstno != '';
                        }
                        var taxvoucheritems = [];
                        $.each($scope.Voucher.voucheritems, function (k, v) {
                            if (v.hastax == true)
                                taxvoucheritems.push(v);
                        });

                        if (taxvoucheritems.length > 0) {
                            $.each(taxvoucheritems, function (k, v) {
                                $scope.Voucher.TaxInfo.TaxLedgers.push(v);
                                $scope.Voucher.voucheritems.splice($scope.Voucher.voucheritems.indexOf(v), 1);
                            });
                            if ($scope.Voucher.TaxInfo)
                                $scope.Voucher.TaxInfo.UpdateTax();
                        }
                        $scope.Voucher.crdr = $scope.Voucher.adjustmentcredit ? 1 : 2;
                        $scope.VoucherTypeChanged();
                    }, 30);
                }
                else {
                    var newList = [];
                    var sno = 0;
                    var uid = 0;
                    $.each(result, function (k, v) {
                        //if (v.vouchertype == 4) {
                        //    v.cramount = !v.adjustmentcredit ? v.amount : '';
                        //    v.dramount = v.adjustmentcredit ? v.amount : '';
                        //}
                        //else {
                        v.dramount = !v.adjustmentcredit ? v.amount : '';
                        v.cramount = v.adjustmentcredit ? v.amount : '';
                        //}

                        v.ledger = '-None-';
                        var ledger = $scope.allledgerlist.find(function (item) {
                            return item.id == v.adjustmentledgerid;
                        })
                        if (typeof (ledger) != 'undefined' && ledger != null)
                            v.ledger = ledger.displayname;
                        v.parent = true;
                        sno++;
                        v.sno = sno;
                        v.parentid = v.id;
                        newList.push(v);
                        if (typeof (v.voucheritems) == 'undefined' || v.voucheritems == null)
                            v.voucheritems = [];
                        v.child = [];
                        v.grpid = 0;
                        var grpList = [];
                        grpList.push(v);
                        uid++;
                        v.isheader = true;
                        v.collapsed = true;
                        v.show = true;
                        v.uid = uid;
                        $.each(v.voucheritems, function (kk, vv) {

                            if (v.attributes &&
                                v.attributes.hasrcm &&
                                vv.hastax) {
                                if (vv.taxtype == 'CGST' ||
                                    vv.taxtype == 'SGST' ||
                                    vv.taxtype == 'IGST')
                                    return;
                            }

                            id--;
                            var newitem = {
                                'adjustmentledgerid': vv.ledgerid, 'vouchertype': v.vouchertype,
                                'adjustmentcredit': !v.adjustmentcredit, 'parent': false
                            };

                            if (v.adjustmentcredit != !vv.iscredit) {
                                //    newitem.dramount = !v.adjustmentcredit ? '' : vv.amount*-1;
                                //    newitem.cramount = v.adjustmentcredit ? '' : vv.amount*-1;

                                newitem.dramount = v.adjustmentcredit ? '' : vv.amount;
                                newitem.cramount = !v.adjustmentcredit ? '' : vv.amount;

                            }
                            else {
                                newitem.dramount = !v.adjustmentcredit ? '' : vv.amount;
                                newitem.cramount = v.adjustmentcredit ? '' : vv.amount;
                            }

                            newitem.ledger = '-None-';
                            var ledger = $scope.allledgerlist.find(function (item) {
                                return item.id == vv.ledgerid;
                            })
                            if (typeof (ledger) != 'undefined' && ledger != null)
                                newitem.ledger = ledger.displayname;

                            newitem.parentid = v.id;
                            newitem.grpid = vv.id;
                            grpList.push(newitem);
                            newitem.show = false;
                            newitem.isheader = false;
                            newitem.uid = uid;
                            newList.push(newitem);
                            v.child.push(newitem);
                        });

                        $.each(grpList, function (k, v) {
                            v.grps = grpList;
                        })

                    });

                    //newList.sort(function (a, b) {
                    //    if (a.sno != b.sno)
                    //        return (a.sno > a.sno) ? 1 : -1;

                    //    return (a.dramount + a.cramount) > (b.dramount + b.cramount) ? 1 : -1;
                    //});

                    $scope.ConfigGrid();
                    $scope.SearchResult = newList;
                    $scope.igrid.UpdateData(newList, '_id');
                }
            }).error(function (result) {
            });
        }

        $scope.ShowLedgerGSTNo = false;
        $scope.LedgerChanged = function () {
            $scope.LedgerGSTNo = '';
            $scope.ShowLedgerGSTNo = false;
            if ($scope.Voucheritems && $scope.Voucheritems.ledgerid) {
                if ($scope.Voucheritems.ledgerid == -1) {
                    $scope.Save();
                    return;
                }
                var selectedledger = $scope.ledgeritemlist.find(function (item) {
                    return item.id == $scope.Voucheritems.ledgerid
                });
                if (selectedledger && selectedledger != null) {
                    $scope.LedgerType = selectedledger.typeid;
                    $scope.LedgerGSTNo = selectedledger.gstno;
                    $scope.ShowLedgerGSTNo = selectedledger.gstno && selectedledger.gstno != null && selectedledger.gstno != '';
                    if (selectedledger.typeid == 30) {
                        var taxable = 0;
                        $.each($scope.Voucher.voucheritems, function (k, v) {
                            taxable += v.amount;
                        });
                        $scope.Voucheritems.amount = (Math.round(taxable) - taxable).round(2);

                    } else if (selectedledger.typeid == 16) {
                        var taxable = 0;
                        var tax = 0;
                        $.each($scope.Voucher.voucheritems, function (k, v) {
                            var selectedledger = $scope.ledgeritemlist.find(function (item) {
                                return item.id == v.ledgerid;
                            });
                            if (selectedledger && selectedledger != null) {
                                if (selectedledger.typeid == 16) {
                                    tax += v.amount;
                                }
                                else {
                                    taxable += v.amount;
                                }
                            }
                        })
                        if (selectedledger.taxpercentage > 0) {
                            if (selectedledger.taxmode == 3 ||
                                selectedledger.taxmode == 4 ||
                                selectedledger.taxmode == 5) {
                                $scope.Voucheritems.amount = (taxable * selectedledger.taxpercentage / 100).round(2);
                            } else if (selectedledger.taxmode == 2) {
                                $scope.Voucheritems.amount = (tax * selectedledger.taxpercentage / 100).round(2);
                            }
                        }
                    }
                }
            }
        }

        $scope.ConfigGrid = function () {

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, false, true, 50);
            igrid.options.showHeaderRow = true;

            igrid.AddFormatrerColumn('grouping', "<span class='infygridtoggle expand'></span>", 40, function (row, cell, value, columnDef, dataContext) {
                var spacer = "<span style='display:inline-block;height:1px;width:8px'></span>";
                if (dataContext.isheader) {
                    if (dataContext.collapsed) {
                        return spacer + " <span class='infygridtoggle expand'></span>&nbsp;";
                    } else {
                        return spacer + " <span class='infygridtoggle collapse'></span>&nbsp;";
                    }
                } else {
                    return spacer + " <span class='infygridtoggle'></span>&nbsp;";
                }
            }).sortable = false;

            var column = igrid.AddCustomColumn('sno', 'S.No', 50, function (item, col) {
                if (item.parent) {
                    return item.sno;
                }
                return '';
            });

            var column = igrid.AddCustomColumn('locationid', 'Location', 100, function (item, col) {
                if (item.parent)
                    return $scope.SearchRef(item.locationid, $scope.Location, 'Main Office');
                return '';
            });

            var column = igrid.AddCustomColumn('vouchertype', 'Voucher', 100, function (item, col) {
                if (item.parent) {
                    if (item.vouchertypeid && item.vouchertypeid > 0)
                        return $scope.SearchRef(item.vouchertypeid, $scope.refvouchertypelist, 'Petty Cash');

                    if (item.vouchertype == 4 && item.sourcetype == 3)
                        return 'EOD Posting - Cash';

                    if (item.vouchertype == 4 && item.sourcetype == 4)
                        return 'EOD Posting - Card';

                    if (item.vouchertype == 4 && item.sourcetype == 5)
                        return 'EOD Posting - UPI';

                    if ($scope.PurchaseType &&
                        $scope.PurchaseType.length > 0 &&
                        item.vouchertype == 3 && item.purchasetypeid && item.purchasetypeid > 0)
                        return $scope.SearchRef(item.purchasetypeid, $scope.PurchaseType, 'Purchase Voucher');

                    return $scope.SearchRef(item.vouchertype * -1, $scope.refvouchertypelist, 'Petty Cash');
                }
                return '';
            });

            var column = igrid.AddCustomColumn('voucherdate', 'Date', 70, function (item, col) {
                if (item.parent)
                    return $filter('date')(item.voucherdate, datePicker.shortDisplay)
                return '';
            });

            var column = igrid.AddCustomColumn('voucherno', 'No', 70, function (item, col) {
                if (item.parent)
                    return item.voucherno;
                return '';
            });


            if ($scope.SearchCondition.filtertype == 2) {
                var column = igrid.AddCustomColumn('inoicedate', 'Inv.Date', 70, function (item, col) {
                    if (item.parent)
                        return $filter('date')(item.attributes.invoicedate, datePicker.shortDisplay)
                    return '';
                });
                var column = igrid.AddCustomColumn('voucherdate', 'Inv.No', 70, function (item, col) {
                    if (item.parent)
                        return item.attributes.invoiceno;
                    return '';
                });
            }

            igrid.AddSimpleColumn('ledger', 'Detail', 250).sortable = false;

            var column = igrid.AddCustomColumn('Debit', 'Debit', 70, function (item, col) {
                if (item.dramount != 0) {
                    return item.dramount;
                }
                return '';
            });
            column.getValue = function (item, col) {
                if (item.dramount != 0) {
                    return item.dramount;
                }
                return 0;
            }
            column.cssClass = 'rightalign';
            column.total = true;
            var column = igrid.AddCustomColumn('Credit', 'Credit', 70, function (item, col) {
                if (item.cramount != 0) {
                    return item.cramount;
                }
                return '';
            });
            column.getValue = function (item, col) {
                if (item.cramount != 0) {
                    return item.cramount;
                }
                return 0;
            }
            column.cssClass = 'rightalign';
            column.total = true;

            if ($scope.SearchCondition.filtertype == 1) {
                var column = igrid.AddCustomColumn('purchasetype', 'Purchase Type', 150, function (item, col) {
                    if (item.parent) {
                        if (item.purchasetypeid && item.purchasetypeid > 0) {
                            return $scope.SearchRef(item.purchasetypeid, $scope.PurchaseType, '')
                        }
                    }
                    return '';
                });

                var column = igrid.AddCustomColumn('narration', 'Narration', 300, function (item, col) {
                    if (item.parent) {
                        return item.narration;
                    }
                    return '';
                });
            }
            else {

                if ($scope.SearchCondition.filtertype == 2) {
                    var column = igrid.AddCustomColumn('narration', 'Narration', 300, function (item, col) {
                        if (item.parent) {
                            return item.narration;
                        }
                        return '';
                    });
                }

                if ($rootScope.UserInfo.p_VOUCHERENTRY.delete) {
                    var column = igrid.AddCustomColumn('Delete', '', 40, function (item, col) {
                        if (item.parent)
                            return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
                        return '';
                    });
                }

                if ($rootScope.UserInfo.p_VOUCHERENTRY.modify) {
                    var column = igrid.AddCustomColumn('Edit', '', 40, function (item, col) {
                        if (item.parent)
                            return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
                        return '';
                    });
                }

                if ($rootScope.UserInfo.p_VOUCHERENTRY.print) {
                    var column = igrid.AddCustomColumn('Print', '', 40, function (item, col) {
                        if (item.parent)
                            return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-info btn-xs'><i class='fa fa-file-pdf-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
                        return '';
                    });
                }
            }

            var column = igrid.AddCustomColumn('createdon', 'Created', 250, function (item, col) {

                if (item.parent) {
                    if (item.cemp && item.cemp != null && item.cemp != '')
                        return $filter('date')(item.createdon, 'dd-MMM-yyyy') + ' @ ' + item.cemp;

                    return $filter('date')(item.createdon, 'dd-MMM-yyyy');
                }
                return '';
            });

            var column = igrid.AddCustomColumn('modifiedon', 'Modified', 250, function (item, col) {
                if (item.parent) {
                    if (item.memp && item.memp != null && item.memp != '')
                        return $filter('date')(item.modifiedon, 'dd-MMM-yyyy') + ' @ ' + item.memp;

                    return $filter('date')(item.modifiedon, 'dd-MMM-yyyy');
                }
                return '';
            });


            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                if (!item.parent) return;
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $scope.ManageSearchItem('Delete', item);
                    }
                    else if (command == 'Edit') {
                        $scope.ManageSearchItem('Edit', item);
                    }
                    else if (command == 'Print') {
                        var url = 'VoucherService/Print/' + $scope.UserInfo.OrganizationID + '/' + item.id;
                        if (item.vouchertype == 33 || item.vouchertypeid == -33)
                            url = 'VoucherService/PrintPO/' + $scope.UserInfo.OrganizationID + '/' + item.id;
                        $rootScope.Download(url, 'voucherprint.pdf', 'application/pdf');
                    }
                });
            });

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    var count = 0;
                    $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                        if (v.parent)
                            count++;
                    });
                    $scope.PrintCount = count;
                }, 10);
            };

            igrid.grid.onSort.unsubscribe(igrid.sortFunction);
            igrid.grid.onSort.subscribe(function (e, args) {

                var grid = args.grid;
                var data = args.grid.getData();
                var cols = args.sortCols;

                var sortstring = false;
                if (typeof (cols[0].sortCol.getValue) != 'undefined' &&
                    data.getLength() > 0) {
                    var value1 = cols[0].sortCol.getValue(data.getItem(0), cols[0].sortCol);
                    var value2 = cols[0].sortCol.getValue(data.getItem(data.getLength() - 1), cols[0].sortCol);
                    if (typeof (value1) == 'string' ||
                        typeof (value2) == 'string')
                        sortstring = true;
                }
                else {
                    if (data.getLength() > 0) {
                        if (typeof (data.getItem(0)[cols[0].sortCol.field]) == 'string' ||
                            typeof (data.getItem(data.getLength() - 1)[cols[0].sortCol.field]) == 'string')
                            sortstring = true;
                    }
                }

                var strcomparer = function (a, b) {
                    if (!a.parent || !b.parent) {
                        if (a.parentid == b.parentid)
                            return (a.grpid > b.grpid) ? 1 : -1;
                        return (a.parentid > b.parentid) ? 1 : -1;
                    }
                    var value1 = a[args.sortCols[0].sortCol.field];
                    var value2 = b[args.sortCols[0].sortCol.field];
                    if (value1 && value1 != null) value1 = value1.toLowerCase();
                    if (value2 && value2 != null) value2 = value2.toLowerCase();
                    return (value1 > value2) ? 1 : -1;
                }

                var comparer = function (a, b) {
                    if (!a.parent || !b.parent) {
                        if (a.parentid == b.parentid)
                            return (a.grpid > b.grpid) ? 1 : -1;
                        return (a.parentid > b.parentid) ? 1 : -1;
                    }
                    return (a[args.sortCols[0].sortCol.field] > b[args.sortCols[0].sortCol.field]) ? 1 : -1;
                }

                var comparerfromgetvalue = function (a, b) {
                    if (!a.parent || !b.parent) {
                        if (a.parentid == b.parentid)
                            return (a.grpid > b.grpid) ? 1 : -1;
                        return (a.parentid > b.parentid) ? 1 : -1;
                    }
                    return (args.sortCols[0].sortCol.getValue(a, args.sortCols[0].sortCol) >
                        args.sortCols[0].sortCol.getValue(b, args.sortCols[0].sortCol)) ? 1 : -1;
                }

                var strfunctioncomparer = function (a, b) {
                    if (!a.parent || !b.parent) {
                        if (a.parentid == b.parentid)
                            return (a.grpid > b.grpid) ? 1 : -1;
                        return (a.parentid > b.parentid) ? 1 : -1;
                    }
                    var value1 = args.sortCols[0].sortCol.getValue(a, args.sortCols[0].sortCol);
                    var value2 = args.sortCols[0].sortCol.getValue(b, args.sortCols[0].sortCol);
                    if (value1 && value1 != null) value1 = value1.toLowerCase();
                    if (value2 && value2 != null) value2 = value2.toLowerCase();
                    return value1 > value2 ? 1 : -1;
                }

                var functioncomparer = function (a, b) {
                    if (!a.parent || !b.parent) {
                        if (a.parentid == b.parentid)
                            return (a.grpid > b.grpid) ? 1 : -1;
                        return (a.parentid > b.parentid) ? 1 : -1;
                    }
                    return args.sortCols[0].sortCol.getValue(a, args.sortCols[0].sortCol) > args.sortCols[0].sortCol.getValue(b, args.sortCols[0].sortCol) ? 1 : -1;
                }

                var formattercomparer = function (a, b) {
                    if (!a.parent || !b.parent) {
                        if (a.parentid == b.parentid)
                            return (a.grpid > b.grpid) ? 1 : -1;
                        return (a.parentid > b.parentid) ? 1 : -1;
                    }
                    var avalue = args.sortCols[0].sortCol.formatter(0, 0, a[args.sortCols[0].sortCol.field], args.sortCols[0].sortCol, a);
                    var bvalue = args.sortCols[0].sortCol.formatter(0, 0, b[args.sortCols[0].sortCol.field], args.sortCols[0].sortCol, b);
                    return avalue > bvalue ? 1 : -1;
                }

                if (args.sortCols[0].sortCol.filtertype == 'number' &&
                    data.getLength() > 0) {
                    if (typeof (data.getItem(0)[args.sortCols[0].sortCol.field]) != 'undefined')
                        data.sort(comparer, args.sortCols[0].sortAsc);
                    else
                        data.sort(functioncomparer, args.sortCols[0].sortAsc);
                }
                else if (cols[0].sortCol.formatter != undefined && cols[0].sortCol.formatter != null) {
                    data.sort(formattercomparer, args.sortCols[0].sortAsc);
                }
                else if (typeof (cols[0].sortCol.getValue) != 'undefined') {

                    data.sort(sortstring ? strfunctioncomparer : functioncomparer, args.sortCols[0].sortAsc);
                }
                else {
                    data.sort(sortstring ? strcomparer : comparer, args.sortCols[0].sortAsc);
                }

                grid.invalidate();
                grid.render();
            });
            $scope.igrid = igrid;

            igrid.grid.onHeaderClick.subscribe(function (e, args) {
                if (args.column.id == "grouping") {
                    var columnElement = e.target;
                    if ((e.target).className.indexOf('expand') > 0) {
                        $(columnElement).removeClass("expand");
                        $(columnElement).addClass("collapse");
                        $.each($scope.SearchResult, function (k, v) {
                            v.show = true;
                            v.collapsed = false;
                        });
                    }
                    else {
                        $(columnElement).addClass("expand");
                        $(columnElement).removeClass("collapse");
                        $.each($scope.SearchResult, function (k, v) {
                            v.show = v.isheader;
                            v.collapsed = true;
                        });
                    }
                    e.stopImmediatePropagation();
                    args.grid.getData().refresh();
                    args.grid.invalidate();
                    args.grid.render();
                }
            });

            $scope.igrid.filter = function (item, args) {
                if (!item.show)
                    return false;
                var count = 0;
                for (var columnId in args.columnFilters) {
                    count++;
                }
                if (count == 0) return item.show;
                var hassuccess = false;
                for (var i = 0; i < item.grps.length; i++) {
                    if ($scope.FilterGroupLevel(item.grps[i], args)) {
                        hassuccess = true;
                        break;
                    }
                }
                return hassuccess;
            }

            $scope.FilterGroupLevel = function (item, args) {
                var result = null;
                for (var columnId in args.columnFilters) {
                    if (columnId !== undefined && args.columnFilters[columnId] !== "") {
                        var cindex = args.grid.getColumnIndex(columnId);
                        var c = args.grid.getColumns()[cindex];
                        if (typeof (c.commafilter) != 'undefined' && c.commafilter) {
                            if (item[c.field] == null) {
                                result = false;
                                break;
                            }
                            var listofvalues = args.columnFilters[columnId].split(',');
                            var exist = false;
                            for (var lv = 0; !exist && lv < listofvalues.length; lv++) {
                                if (item[c.field].toString().toLowerCase().startsWith(listofvalues[lv].toLowerCase())) {
                                    exist = true;
                                }
                                if (listofvalues[lv].toString().toLowerCase().startsWith('%') &&
                                    item[c.field].toString().toLowerCase().indexOf(listofvalues[lv].toLowerCase().substr(1)) >= 0)
                                    exist = true;
                            }
                            if (!exist) {
                                result = false;
                                break;
                            }
                        }
                        else if (typeof (args.columnTypes[columnId]) != 'undefined' && args.columnTypes[columnId] == 'number') {
                            if (args.columnFilters[columnId].length > 0) {
                                var operator = args.columnFilters[columnId].charAt(0);
                                if (operator == '>' || operator == '<' || operator == '=') {
                                    var d = parseFloat(args.columnFilters[columnId].substr(1));
                                    switch (args.columnFilters[columnId].charAt(0)) {
                                        case '>':
                                            result = item[c.field] > d;
                                            break;
                                        case '<':
                                            result = item[c.field] < d;
                                            break;
                                        case '=':
                                            result = item[c.field] == d;
                                            break;
                                    }
                                }
                                else {
                                    var d = parseFloat(args.columnFilters[columnId]);
                                    if (typeof (item[c.field]) != 'undefined') {
                                        result = item[c.field] == d;
                                    }
                                    var fvalue = c.formatter(0, cindex, item[c.field], c, item);
                                    result = fvalue == d;
                                }
                                if (result != null)
                                    break;
                                //return item[c.field] != null && item[c.field].toString().indexOf(args.columnFilters[columnId]) > 0;
                            }
                        }
                        else if (typeof (args.columnTypes[columnId]) != 'undefined' && args.columnTypes[columnId] == 'reference') {
                            //if (item[c.field] == null)
                            //    return false;

                            var fvalue = c.formatter(0, cindex, item[c.field], c, item);
                            if (typeof (fvalue) == 'undefined' || fvalue == null ||
                                !fvalue.toString().toLowerCase().startsWith(args.columnFilters[columnId].toLowerCase())) {
                                if (!args.columnFilters[columnId].startsWith('%') ||
                                    fvalue.toString().toLowerCase().indexOf(args.columnFilters[columnId].toLowerCase().substr(1)) < 0) {
                                    result = false;
                                    break;
                                }
                            }
                        }
                        else {

                            var fvalue = typeof (item[c.field]) != 'undefined' ? item[c.field] : c.formatter(0, cindex, item[c.field], c, item);
                            if (fvalue == null ||
                                !fvalue.toString().toLowerCase().startsWith(args.columnFilters[columnId].toLowerCase())) {
                                if (!args.columnFilters[columnId].startsWith('%') ||
                                    fvalue.toString().toLowerCase().indexOf(args.columnFilters[columnId].toLowerCase().substr(1)) < 0) {
                                    result = false;
                                    break;
                                }
                            }
                            result = true;
                            break;
                        }
                    }
                }
                if (result == null)
                    return true;
                return result;
            }

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.cell == 0) {
                    var item = $scope.igrid.dataView.getItem(args.row);
                    if (item) {
                        item.collapsed = !item.collapsed;
                        $.each($scope.SearchResult, function (k, v) {
                            if (v.uid == item.uid && !v.isheader) {
                                v.show = !item.collapsed;
                            }
                        });
                        $timeout(function () {
                            $scope.igrid.referesh();
                        }, 100);
                    }
                    e.stopImmediatePropagation();
                }
            });

            $scope.igrid.UpdateData([]);
            $scope.PrintCount = 0;
        }

        $scope.PrintCount = 0;

        $scope.PrintSelectedVouchers = function (voucherid, vouchertype) {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                $rootScope.Toaster.Info("Please select printer");
                return;
            }
            var ids = [];
            if (voucherid && voucherid != null) {
                ids.push(voucherid);
            }
            else {
                $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                    if (v.parent) {
                        ids.push(v.id)
                        vouchertype = v.vouchertype;
                    }
                });
            }

            var request = {
                "item": { 'ids': ids, 'vouchertype': vouchertype },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "VoucherService/Print",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length > 0) {
                    var base64data = [];
                    $.each(result, function (k, v) {
                        base64data.push(Base64.encode(v));
                    })
                    $rootScope.Sent2Printer(base64data.join(';'), printerData, 'PDF');
                }
            });
        }

        $scope.PrintVoucher = function (voucher) {
            var url = 'VoucherService/Print/' + $scope.UserInfo.OrganizationID + '/' + voucher.id;
            if (voucher.vouchertype == 33)
                url = 'VoucherService/PrintPO/' + $scope.UserInfo.OrganizationID + '/' + voucher.id;
            $rootScope.Download(url, 'voucherprint.pdf', 'application/pdf');
        }

        $scope.exporttoExcel = function (infygrid, filename) {
            var title = 'Voucher';
            if ($scope.SearchCondition.ledgerid &&
                $scope.SearchCondition.ledgerid > 0) {
                var ledger = $scope.LedgerList.find(function (item) {
                    return item.id == $scope.SearchCondition.ledgerid;
                });
                if (ledger && ledger != null)
                    title = ledger.displayname;
            }

            var fromdate = $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy')
            var todate = $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy')
            subtitle = fromdate + ' - ' + todate;
            var additionalrows = [];

            infygrid.ExportXLSX(infygrid.grid, title, subtitle, true, additionalrows);

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

        $scope.LedgerCondition = {
            typeid: 0
        }
        $scope.GetVoucher = function () {
            request = {
                "Item": $scope.LedgerCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LedgerService/Search",
                data: srequest
            }).success(function (result) {
                $scope.LedgerList = result;
                $.each(result, function (k, v) {
                    $scope.allledgerlist.push(v);
                });
                $scope.LoadPaytype();
                if (typeof ($location.search().id) != 'undefined' &&
                    $location.search().id.length > 0) {
                    $scope.ShowNew = true;
                    $scope.Search($location.search().id);
                    $scope.Navigation.SetMode('EDIT');
                }
                else if ($stateParams &&
                    $stateParams.voucher &&
                    $stateParams.voucher != null) {
                    $scope.ShowNew = true;
                    $scope.Search($stateParams.voucher.id);
                    $scope.Navigation.SetMode('EDIT');
                }
                else {
                    $scope.Search();
                }
            }).error(function (result) {
            });
        }

        $scope.LoadPaytype = function () {
            $scope.refbanklist = [];

            $.each($scope.allledgerlist, function (k, v) {
                if (v.typeid == 1 || v.typeid == 2) {
                    $scope.refbanklist.push(v);
                }
            });
        }


        $scope.ShowLedger = function (item) {
            var dialog = ngDialog.open({
                template: 'clientside/views/ledger.html?d=' + (new Date()).getTime(),
                data: [$scope, item.ledgerid],
                width: '750px',
                controller: 'ledgerController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;
                    $scope.allledgerlist.push(data.value);
                    $scope.LoadItems();
                }
            });
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
                    $scope.allledgerlist.push(data.value);
                    $scope.LoadItems();
                }
            });
        }

        $scope.LoadItems = function (vouchertype) {

            if (typeof (vouchertype) == 'undefined' || vouchertype == null)
                vouchertype = $scope.Voucher.vouchertype;

            vouchertype = parseInt(vouchertype);

            if (vouchertype == 6 || vouchertype == 4 || vouchertype == 2 || vouchertype == 16 || vouchertype == 17 || vouchertype == 19 ) {
                $scope.Voucher.crdr = $scope.EnumCRDR.Dr;
            }
            else {
                $scope.Voucher.crdr = $scope.EnumCRDR.Cr;
            }

            if ($scope.system_disableautoledger || vouchertype == 0) {
                $scope.adjustmentlist = $scope.allledgerlist;
                $scope.ledgeritemlist = $scope.allledgerlist;
                return;
            }

            var ledgeritemlist = [];
            var adjustmentlist = [];
            var salespurchaseledgers = [];
            $.each($scope.allledgerlist, function (k, v) {

                if (v.typeid == 30) {
                    ledgeritemlist.push(v);
                }
                else {
                    switch (vouchertype) {
                        case 1:
                            if (v.typeid == 12 || v.typeid == 5 || v.typeid == 10 || v.typeid == 2 || v.typeid == 20 || v.typeid == 1 || v.typeid == 16 || v.typeid == 17 || v.typeid == 18)
                                ledgeritemlist.push(v);
                            else if (v.typeid == 6 || v.typeid == 7 || v.typeid == 9 || v.typeid == 22 || v.typeid == 4 || v.typeid == 21 || v.typeid == 13)
                                adjustmentlist.push(v);
                            else if (vouchertype == 1 && ((v.typeid == 11 && v.customerid > 0) || (v.typeid == 12 && v.supplierid > 0)))
                                ledgeritemlist.push(v);
                            else if (v.typeid == 19) {
                                adjustmentlist.push(v);
                                ledgeritemlist.push(v);
                            }
                            break;
                        case 2:
                            if (v.typeid == 6 || v.typeid == 7 || v.typeid == 13 || v.typeid == 19)
                                adjustmentlist.push(v); //dr
                            else if (v.typeid == 4 || v.typeid == 21 || v.typeid == 9 || v.typeid == 22 || v.typeid == 1 || v.typeid == 11 || v.typeid == 12 || v.typeid == 20)
                                ledgeritemlist.push(v);
                            break;
                        case 3:
                        case 33:
                            if (v.typeid == 15)
                                salespurchaseledgers.push(v);
                            if (v.typeid == 12 || v.typeid == 6 || v.typeid == 7 || v.typeid == 19)
                                adjustmentlist.push(v); //cr
                            else if (v.typeid == 15 || v.typeid == 16 || v.typeid == 8 || v.typeid == 5 || v.typeid == 10 || v.typeid == 2 || v.typeid == 20 || v.typeid == 3 || v.typeid == 17 || v.typeid == 18 || v.typeid == 8)
                                ledgeritemlist.push(v);
                            break;
                        case 4:
                            if (v.typeid == 14)
                                salespurchaseledgers.push(v);

                            if (v.typeid == 11 || v.typeid == 7 || v.typeid == 6 || v.typeid == 19)
                                adjustmentlist.push(v); //dr
                            else if (v.typeid == 14 || v.typeid == 16 || v.typeid == 4 || v.typeid == 21 || v.typeid == 9 || v.typeid == 22 || v.typeid == 8)
                                ledgeritemlist.push(v);
                            break;
                        case 5:
                            if (v.typeid == 11)
                                adjustmentlist.push(v);//cr
                            else if (v.typeid == 14 || v.typeid == 16 || v.typeid == 4 || v.typeid == 21 || v.typeid == 9 || v.typeid == 22)
                                ledgeritemlist.push(v);
                            break;
                        case 6:
                            if (v.typeid == 12)
                                adjustmentlist.push(v);//dr
                            else if (v.typeid == 15 || v.typeid == 16 || v.typeid == 8 || v.typeid == 5 || v.typeid == 10 || v.typeid == 17 || v.typeid == 18)
                                ledgeritemlist.push(v);
                            break;
                        case 7:
                        case 17:
                            if (v.typeid == 6 || v.typeid == 7) {
                                adjustmentlist.push(v); //dr
                                ledgeritemlist.push(v);
                            }
                            break;
                        case 8:
                        case 16:
                            if (!(v.typeid == 6 || v.typeid == 8 || v.typeid == 13 || v.typeid == 14 || v.typeid == 15)) {
                                adjustmentlist.push(v); //dr
                                ledgeritemlist.push(v);
                            }
                            break;
                        case 9:
                            if (v.typeid == 6) {
                                adjustmentlist.push(v);
                            }
                            else if (v.typeid == 7) {
                                ledgeritemlist.push(v);
                            }
                            break;
                        case 10:
                            if (v.typeid == 7) {
                                adjustmentlist.push(v);
                            }
                            else if (v.typeid == 6) {
                                ledgeritemlist.push(v);
                            }
                            break;
                        case 11:
                            if (v.typeid == 6) {
                                adjustmentlist.push(v);
                            }
                            else if (v.typeid == 13) {
                                ledgeritemlist.push(v);
                            }
                            break;
                        case 12:
                            if (v.typeid == 13) {
                                adjustmentlist.push(v);
                            }
                            else if (v.typeid == 6) {
                                ledgeritemlist.push(v);
                            }
                            break;
                        case 13:
                        case 15:
                            if (v.typeid == 12 || v.typeid == 5 || v.typeid == 10 || v.typeid == 2 || v.typeid == 20 || v.typeid == 1 || v.typeid == 17 || v.typeid == 18 || v.typeid == 19)
                                ledgeritemlist.push(v);
                            else if (v.typeid == 6 || v.typeid == 7 || v.typeid == 19 || v.typeid == 9 || v.typeid == 22 || v.typeid == 4 || v.typeid == 21 || v.typeid == 13)
                                adjustmentlist.push(v);
                            else if (vouchertype == 1 && ((v.typeid == 11 && v.customerid > 0) || (v.typeid == 12 && v.supplierid > 0)))
                                ledgeritemlist.push(v);
                            break;
                        case 14:
                        case 18:
                            if (v.typeid == 6 || v.typeid == 7 || v.typeid == 19 || v.typeid == 13)
                                adjustmentlist.push(v);
                            else if (v.typeid == 11 || v.typeid == 12)
                                ledgeritemlist.push(v);
                            break;
                    }
                }
            });
            ledgeritemlist.push({ 'id': -1, 'typeid': -1, 'displayname': '-End Of Voucher-' });
            $scope.adjustmentlist = adjustmentlist;
            $scope.ledgeritemlist = ledgeritemlist;
            $scope.salespurchaseledgers = salespurchaseledgers;
        }

        $timeout(function () {
            $scope.SearchCondition.fromdate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.SearchCondition.todate = $filter('date')(new Date(), datePicker.shortDisplay);

            $scope.GetVoucher();
            $scope.AddNew(false);

        }, 50);


        $scope.ShowVoucherGSTNo = false;
        $scope.VoucherGSTNo = '';
        $scope.setadjustmentledgertype = function (next) {
            $scope.AdjustmentLedgerType = -1;
            var adjustmentledger = $scope.adjustmentlist.find(function (item) {
                return item.id == $scope.Voucher.adjustmentledgerid;
            });


            if (adjustmentledger && adjustmentledger != null) {
                $scope.AdjustmentLedgerType = adjustmentledger.typeid;
                $scope.VoucherGSTNo = adjustmentledger.gstno;
                $scope.ShowVoucherGSTNo = adjustmentledger.gstno && adjustmentledger.gstno != null && adjustmentledger.gstno != '';

                if (adjustmentledger.billwise) {
                    $scope.Voucher.bwise = adjustmentledger.typeid;
                }

                if ($scope.Voucher.attributes == undefined || $scope.Voucher.attributes == null)
                    $scope.Voucher.attributes = {};

                $scope.Voucher.ledgertdsid = 0;
                $scope.Voucher.attributes.hastds = adjustmentledger && adjustmentledger.attributes &&
                    adjustmentledger.attributes.hastds;

                if ($scope.Voucher.attributes.hastds)
                    $scope.Voucher.ledgertdsid = adjustmentledger.attributes.ledgertdsid;

                $scope.Voucher.attributes.ledgertype = adjustmentledger.typeid;

                if ($scope.AdjustmentLedgerType == 7) {
                    next = 'vbanktransfermode';
                    $timeout(function () {
                        $scope['setFocus' + next]();
                    }, 10);
                    return;
                }
            }

            if (($scope.AdjustmentLedgerType == 11 || $scope.AdjustmentLedgerType == 12)
                && ($scope.Voucher.vouchertype == 3 || $scope.Voucher.vouchertype == 33 || $scope.Voucher.vouchertype == 4)) {
                next = 'vinvoiceno';
            }

            if ($scope.Voucher.vouchertype == 9)
                next = 'salesranges';

            if (next && next != null)
                $scope['setFocus' + next]();
        }

        $scope.SearchVoucherName = function (searchValue, listValues) {
            if (typeof (listValues) == 'undefined' || listValues == null)
                return "Loading";
            var found = listValues.find(function (arritem) {
                return arritem.id == searchValue;
            });
            if (typeof (found) != 'undefined' && found != null)
                return typeof (found.id) == 'undefined' ? 0 : found.displayname;

            return typeof (defaultValue) == 'undefined' ? "None" : defaultValue;
        };


        $scope.ShowEInvoice = function (item, callback) {
            var dialog = ngDialog.open({
                template: 'EInvoice',
                data: [$scope, $scope.Voucher],
                width: '850px',
                controller: ['$scope', '$timeout', '$http', 'restService', '$rootScope',
                    function ($scope, $timeout, $http, restService, $rootScope) {
                        $scope.Voucher = $scope.ngDialogData[1];
                        $scope.Parent = $scope.ngDialogData[0];
                        $scope.Generate = function () {
                            request = {
                                "Item": $scope.Voucher,
                                "SecurityToken": "String content"
                            };
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "VoucherService/GenerateEInvoice",
                                data: srequest
                            }).success(function (result) {
                                if (result.ErrorDetails && result.ErrorDetails.length > 0) {
                                    var errors = [];
                                    $.each(result.ErrorDetails, function (k, v) {
                                        errors.push(v.ErrorMessage);
                                    });
                                    $rootScope.ShowAlert('Error', errors.join('<br>'));
                                } else {
                                    $scope.Voucher.attributes = result.Voucher.attributes;
                                }
                            });
                        }

                        $scope.Question = function (title, content, callback) {
                            $.confirm({
                                title: title,
                                content: content,
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
                                            callback();
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

                        $scope.CancelEInvoice = function (mode) {
                            $scope.Question('Einvoice Cancellation', 'Are you sure, do you want to cancel the e-invoice for this bill', function () {
                                $scope.InternalCancelEInvoice(mode);
                            })
                        }

                        $scope.InternalCancelEInvoice = function (mode) {
                            request = {
                                "Item": $scope.Voucher,
                                "SecurityToken": "String content"
                            };
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "VoucherService/CancelEInvoice",
                                data: srequest
                            }).success(function (result) {

                                if (result.ErrorDetails && result.ErrorDetails.length > 0) {
                                    var errors = [];
                                    $.each(result.ErrorDetails, function (k, v) {
                                        errors.push(v.ErrorMessage);
                                    });
                                    $rootScope.ShowAlert('Error', errors.join('<br>'));
                                } else {
                                    $scope.Voucher.attributes = result.Voucher.attributes;
                                }

                            });
                        }
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        callback(data.value);
                    }
                }
            });
        };

        $scope.ChangeTaxStatus = function (type, status) {
            if (type == 'TDS') {
                $scope.Voucher.attributes.dtds = status;
            }
            if (type == 'GST') {
                $scope.Voucher.attributes.dgst = status;
            }
            $scope.ComputeGSTTax();
            $scope.Totalamount();
        }

        $scope.focusledger = function () {
            $scope.focusconditiononenter($scope.ItemwisePurchase, 'itemledgerid', 'ledgerid');
        }

        $scope.focusconditiononenter = function (issuccess, sfocus, ffocus) {
            $scope[issuccess ? 'setFocus' + sfocus : 'setFocus' + ffocus]();
        }

        function tddsRuleEngine(scope) {
            this.scope = scope;
            this.Initialize = function () {
            }
            this.Validate = function (voucher) {
                if (voucher == undefined)
                    voucher = $scope.Voucher;
                var missingtds = [];
                var displaymsg = [];
                $.each(voucher.voucheritems, function (k, v) {
                    var ledger = $scope.allledgerlist.find(function (item) {
                        return item.id == v.ledgerid;
                    });
                    if (ledger && ledger.attributes && ledger.attributes.hastds) {
                        var ledgertdsid = ledger.attributes.ledgertdsid;
                        var tdsvoucherentry = voucher.TaxInfo.TaxLedgers.find(function (item) {
                            return item.ledgerid == ledgertdsid
                        });
                        if (voucher.TaxInfo.TaxLedgers == undefined || voucher.TaxInfo.TaxLedgers == null ||
                            tdsvoucherentry == undefined || tdsvoucherentry == null) {
                            missingtds.push(v);
                            displaymsg.push($scope.LedgerDisplay(v.ledgerid) - $scope.LedgerDisplay(ledgertdsid));
                        }
                    }
                });


                var ledger = $scope.allledgerlist.find(function (item) {
                    return item.id == voucher.adjustmentledgerid;
                });
                if (ledger && ledger.attributes && ledger.attributes.hastds) {
                    var ledgertdsid = ledger.attributes.ledgertdsid;
                    var tdsvoucherentry = voucher.TaxInfo.TaxLedgers.find(function (item) {
                        return item.ledgerid == ledgertdsid
                    });
                    if (voucher.TaxInfo.TaxLedgers == undefined || voucher.TaxInfo.TaxLedgers == null ||
                        tdsvoucherentry == undefined || tdsvoucherentry == null) {
                        missingtds.push({ 'ledgerid': voucher.adjustmentledgerid });
                        displaymsg.push($scope.LedgerDisplay(voucher.adjustmentledgerid) + ' - ' + $scope.LedgerDisplay(ledgertdsid));
                    }
                }

                if (missingtds.length == 0)
                    return '';

                return displaymsg.join(',');
            }
        };

        $scope.TdsRuleEngine = new tddsRuleEngine();
        $scope.panelheight = { 'height': '400px' };
        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 50) + 'px';
            }
            $scope.TdsRuleEngine.Initialize();
            $scope.Cancel();
        })

    }]);