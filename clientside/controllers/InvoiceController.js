var app = angular.module('InfinityLab');
app.controller('InvoiceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope',
    '$window', 'restService', '$timeout', 'Excel', '$filter', '$stateParams', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, Excel, $filter,
        $stateParams, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();

        $scope.PaymentModes = [
            { 'id': 0, 'name': 'All' },
            { 'id': 1, 'name': 'Yes' },
            { 'id': 2, 'name': 'No' }]

        $scope.EntryTypes = [
            { 'id': -1, 'name': 'None', 'discount': true, 'amount': true, 'tax': false, 'directamount': false, 'lrexpense': false },
            { 'id': 0, 'name': 'Amount', 'discount': true, 'amount': true, 'tax': true, 'directamount': false, 'lrexpense': false },
            { 'id': 1, 'name': 'Tax', 'discount': false, 'amount': false, 'tax': true, 'directamount': false, 'lrexpense': false },
            { 'id': 2, 'name': 'Discount', 'discount': true, 'amount': false, 'tax': false, 'directamount': false, 'lrexpense': false },
            { 'id': 3, 'name': 'OnDiscount', 'discount': true, 'amount': false, 'tax': false, 'directamount': false, 'lrexpense': false },
            { 'id': 4, 'name': 'Insurance', 'discount': false, 'amount': true, 'tax': true, 'directamount': false, 'lrexpense': true },
            { 'id': 5, 'name': 'Service', 'discount': false, 'amount': true, 'tax': true, 'directamount': false, 'lrexpense': true },
            { 'id': 6, 'name': 'Courier', 'discount': false, 'amount': true, 'tax': true, 'directamount': false, 'lrexpense': true },
            { 'id': 7, 'name': 'Packing', 'discount': false, 'amount': true, 'tax': true, 'directamount': false, 'lrexpense': true },
            { 'id': 8, 'name': 'Fright', 'discount': false, 'amount': true, 'tax': true, 'directamount': false, 'lrexpense': true },
            { 'id': 9, 'name': 'Rounding', 'discount': true, 'amount': true, 'tax': true, 'directamount': false, 'lrexpense': false },
            { 'id': 10, 'name': 'Job Work', 'discount': false, 'amount': true, 'tax': true, 'directamount': false, 'lrexpense': true },
            { 'id': 11, 'name': 'Cess', 'discount': false, 'amount': false, 'tax': true, 'directamount': false, 'lrexpense': true },
            { 'id': 12, 'name': 'TCS', 'discount': false, 'amount': false, 'tax': true, 'directamount': false, 'lrexpense': true },
            { 'id': 13, 'name': 'Agnt Comm', 'discount': false, 'amount': true, 'tax': true, 'directamount': false, 'lrexpense': true },
            { 'id': 14, 'name': 'Agnt Discount', 'discount': true, 'amount': false, 'tax': false, 'directamount': false, 'lrexpense': false }];


        $scope.LRInfo = { lrid: $location.search().lrid };

        $scope.POList = [];
        $scope.POType = restService.Get('ReferencelistService/POTYPE', true).then(function (results) {
            $scope.POType = results;
            results.push({ id: -3, name: 'Vendor Agreement' });
            results.push({ id: -4, name: 'Daily Trade' });
        });

        $scope.companygst = '';
        $scope.suppliergst = '';
        $scope.SearchRef = restService.SearchReference;

        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.AutoCalculation = false;
        $scope.SearchCondition = {
            lrentryno: "",
            suppliername: 0,
            fromdate: null,
            todate: null,
            invoiceno: ""
        }

        $scope.intray_pur_prt = false;
        $scope.disableinvoiceentrydate = false;
        $scope.int_finance = false;
        $scope.supportapproval = false;
        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo.appsettings && $rootScope.UserInfo.appsettings != null) {
                $scope.disableinvoiceentrydate = $rootScope.UserInfo.appsettings.system_disableinvoiceentrydate;
                $scope.int_finance = false || $rootScope.UserInfo.appsettings.int_finance;
                $scope.intray_pur_prt = false || $rootScope.UserInfo.appsettings.intray_pur_prt;
                $scope.supportapproval = $rootScope.UserInfo.p_CANAPPROVEPURCHASE && $rootScope.UserInfo.p_CANAPPROVEPURCHASE.view;
            }
            else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 50);
            }
        }
            

        $scope.SupplierCompany = null;
        restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
        });

        $scope.TDSLedgers = null;
        restService.Get('ReferencelistService/FULL-TDSLEDGERS', true).then(function (results) {
            $scope.TDSLedgers = results;
        });

        $scope.StatusList = [{ id: 1, name: 'Pending' }, { id: 2, name: 'Completed' }];

        $scope.InvoiceItemTemplate = {
            "companyid": 0, "invoiceno": null, "invoicedate": null, "noofpieces": 0, "noofbundle": 0, "baseamount": 0.0, "grossamount": 0.0, "totalamount": 0.0, "plessvalue": 0.0, "expensepercentage": 0,
            "lrid": 0, "id": 0, "otheramount": null, "modifiedon": "", "createdon": "", "createdby": 0, "modifiedby": 0, "taxamount": 0.0, "discount": 0, "discountpercentage": 0,
            "version": 1, "suppliercompanyid": 0, "isactive": true, Lrinvoicetaxitems: [], "supplierid": 0, 'isinclusiveall': false, 'isinclusivediscount': false, 'isinclusivetax': false,
            "ptype": 0, "purchaseorderid": 0
        };

        $scope.InvoiceTemplate = {
            "companyid": 0, "lrid": 0, "suppliercompanyid": 0, Items: [], "status": 0, "entrydate": new Date()
        };

        $scope.InvoiceTaxTemplate = {
            "taxname": "", "taxid": "0", "amounton": "", "percentage": "", "netamount": "", "discount": "", "version": 1, 'isactive': true, "discountpercentage": "", "taxamount": "", 'type': -1,
            'typeinfo': {}
        };

        $scope.Invoice = (JSON.parse(JSON.stringify($scope.InvoiceTemplate)));
        $scope.InvoiceItem = (JSON.parse(JSON.stringify($scope.InvoiceItemTemplate)));
        $scope.InvoiceTax = (JSON.parse(JSON.stringify($scope.InvoiceTaxTemplate)));
        $scope.purchaseTax = restService.DefaultReference('PURCHASETAX');
        $scope.suppliercompany = restService.DefaultReference('SUPPLIERCOMPANY');
        $scope.SupplierCompany = null;
        $scope.BuyingCompany = restService.DefaultReference('COMPANY');

        $scope.purchaseTaxActual = [];



        restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.purchaseTaxActual = results;
            var taxlist = [];
            $.each(results, function (k, v) {
                $.each(v.taxsplit.items, function (kk, vv) {
                    var taxitem = (JSON.parse(JSON.stringify(v)));
                    taxitem.taxsplit = null;
                    taxitem.taxpercentage = vv.cgst + vv.sgst;
                    if (v.type == 1 || v.type == 2) {
                        if (v.taxsplit.items.length > 1)
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '% (' + vv.fromamount + ' - ' + vv.toamount + ')';
                        else
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = '' + taxitem.id + '_' + kk;
                        taxlist.push(taxitem);
                    }
                    else if (v.type == 4) {
                        taxitem.name = 'TDS @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = '' + taxitem.id + '_' + kk;
                        taxlist.push(taxitem);
                    }
                    else if (v.type == 5) {
                        taxitem.name = 'TCS @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = '' + taxitem.id + '_' + kk;
                        taxlist.push(taxitem);
                    }
                    else if (v.type == 6) {
                        taxitem.name = 'CESS @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = '' + taxitem.id + '_' + kk;
                        taxlist.push(taxitem);
                    }
                });
            })
            $scope.purchaseTax = taxlist;
        });

        $scope.FilterSupplierCompany = function () {
            if ($scope.SupplierCompany && $scope.SupplierCompany != null) {
                $scope.suppliercompany = [];
                if ($scope.SupplierCompany != null && $scope.Invoice.supplierid > 0) {
                    $.each($scope.SupplierCompany, function (k, v) {
                        if (v.supplierid == $scope.Invoice.supplierid)
                            $scope.suppliercompany.push(v);
                    });
                    $scope.SupplierCompany = $scope.suppliercompany;
                    if ($scope.suppliercompany.length == 1) {
                        $scope.InvoiceItem.suppliercompanyid = 0;
                        $timeout(function () {
                            $scope.InvoiceItem.suppliercompanyid = $scope.suppliercompany[0].id;
                            $scope.InvoiceItem.isinterstatesale = $scope.suppliercompany[0].isinterstatesale;
                            $scope.suppliergst = $scope.suppliercompany[0].gst;
                        }, 100);
                    }
                }
            }
            else {
                $timeout(function () {
                    $scope.FilterSupplierCompany();
                }, 50);
            }
        };

        var suppliercompanyfiltered = false;
        /*$scope.FilterSupplierCompany = function () {
             $scope.suppliercompany = [];
             if ($scope.SupplierCompany != null) {
                 $.each($scope.SupplierCompany, function (k, v) {
                     if (v.id == $scope.Invoice.supplierid)
                         $scope.suppliercompany.push(v);
                 });
             }
         };
     
         var suppliercompanyfiltered = false;
         $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
             $scope.SupplierCompany = results;
             $scope.FilterSupplierCompany();
         });*/


        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.BuyingCompany = null;
        restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;

            if ($location.search().lrid == undefined) {
                $rootScope.BindCompany($scope, 'InvoiceItem.companyid', $scope.CompanyChanged);
            }
        });
        $scope.ReceivingLocations = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.ReceivingLocations = results;
        });
        $scope.validatetype = function () {
            if ($scope.InvoiceTax.typeinfo.id >= 0) {

                //if ($scope.InvoiceTax.typeinfo)
                //    $scope.setFocustaxnetamount();
                //else
                $scope.setFocustaxamounton();
            }
            else
                $scope.setFocusbillvalue();
        }

        $scope.validatediscountpercent = function () {
            if ($scope.InvoiceTax.discountpercentage.toString().startsWith('i')) {
                $scope.InvoiceTax.discountpercentage = $scope.InvoiceTax.discountpercentage.replace('i', '');
                var discountpercent = $scope.GetNumber($scope.InvoiceTax.discountpercentage);
                var finalamount = $scope.GetNumber($scope.InvoiceTax.amounton);
                var newdiscount = finalamount / (100 - discountpercent) * discountpercent
                var newamount = finalamount + newdiscount;
                $scope.InvoiceTax.discount = newdiscount.round(2);
                $scope.InvoiceTax.discountpercentage = discountpercent.round(4);
                $scope.InvoiceTax.amounton = newamount.round(2);
                $scope.Compute('netamount');
                if (!$scope.InvoiceTax.typeinfo.tax)
                    $scope.AddTaxItem();
                else
                    $scope.setFocustaxtaxid();
            }
            else {
                $scope.setFocustaxdiscount();
            }
        }

        $scope.validatetax = function () {
            if ($scope.InvoiceTax.discount != eval($scope.InvoiceTax.discount)) {
                $scope.InvoiceTax.discount = eval($scope.InvoiceTax.discount);
                $scope.Compute('discount')
            }
            if (!$scope.InvoiceTax.typeinfo.tax)
                $scope.AddTaxItem();
            else
                $scope.setFocustaxtaxid();
        }

        $scope.AddDirect = function (taxid, taxValue, billValue, discountpercentage, discountvalue) {
            $scope.InvoiceTax.type = 0;
            $scope.typeChanged();
            $scope.InvoiceTax.amounton = billValue + discountvalue;
            $scope.InvoiceTax.discountpercentage = discountpercentage;
            $scope.InvoiceTax.discount = discountvalue;
            $scope.InvoiceTax.taxid = taxid;
            $scope.taxChanged();
        }

        $scope.Compute = function (source) {
            if (typeof ($scope.InvoiceItem.Lrinvoicetaxitems) == 'undefined' || $scope.InvoiceItem.Lrinvoicetaxitems == null)
                $scope.InvoiceItem.Lrinvoicetaxitems = [];

            if (source == 'netamount' && ($scope.InvoiceTax.type == 11 || $scope.InvoiceTax.type == 12)) {
                if ($scope.InvoiceTax.typeinfo.tax && $scope.GetNumber($scope.InvoiceTax.percentage) > 0) {
                    var netamt = $scope.GetNumber($scope.InvoiceTax.netamount);
                    $scope.InvoiceTax.amounton = ((netamt / $scope.GetNumber($scope.InvoiceTax.percentage)) * 100).round(2);
                    $scope.InvoiceTax.taxamount = netamt;
                }
                return;
            }

            if (source == 'type') {
                if ($scope.InvoiceTax.type >= 0 && $scope.InvoiceTax.type <= 3) {
                    var baseamount = 0;
                    $.each($scope.InvoiceItem.Lrinvoicetaxitems, function (k, v) {
                        //if (v.type == 0)
                        //    baseamount += v.amounton;
                        //else if(v.type == 1 || v.type == 2)
                        baseamount += v.netamount;
                    });
                    $scope.InvoiceTax.amounton = baseamount.round(2);
                } else {
                    var baseamount = 0;
                    $.each($scope.InvoiceItem.Lrinvoicetaxitems, function (k, v) {
                        baseamount += v.netamount;
                    });
                    $scope.InvoiceTax.amounton = (baseamount - baseamount.round(0)).round(2);
                }
            }

            if (source == 'taxChanged') {
                $scope.taxpercentage = 0;
                if ($scope.taxid != null) {
                    var taxitem = $scope.purchaseTax.find(function (item) {
                        return (item.code == $scope.taxid);
                    });
                    if (taxitem != null) {
                        $scope.taxpercentage = taxitem.taxpercentage;
                        source = 'directamount';
                    };
                };
            }

            if (source == 'directamount') {
                var taxValue = $scope.GetNumber($scope.taxValue);
                $scope.billValue = (taxValue / $scope.taxpercentage * 100).round(2);
            }

            if (source == 'directdiscount') {
                var discountpercent = $scope.GetNumber($scope.discountpercentage);
                $scope.discountvalue = ($scope.billValue / (100 - discountpercent) * discountpercent).round(2)
            }

            if ($scope.InvoiceTax.typeinfo.discount) {
                if (source == 'discountpercentage' || source == 'amounton') {
                    $scope.InvoiceTax.discount = ($scope.InvoiceTax.amounton * $scope.InvoiceTax.discountpercentage / 100).round(2);
                    if (isNaN($scope.InvoiceTax.discount)) $scope.InvoiceTax.discount = 0;
                }
                if (source == 'discount') {
                    var discountvalue = eval($scope.InvoiceTax.discount);
                    $scope.InvoiceTax.discountpercentage = (discountvalue / $scope.InvoiceTax.amounton * 100).round(4);
                    if (isNaN($scope.InvoiceTax.discountpercentage)) $scope.InvoiceTax.discountpercentage = 0;
                }
            }
            else {
                $scope.InvoiceTax.discount = 0;
                $scope.InvoiceTax.discountpercentage = 0;
            }

            if (source != 'taxamount') {
                if ($scope.InvoiceTax.typeinfo.tax && $scope.GetNumber($scope.InvoiceTax.percentage) > 0) {
                    $scope.InvoiceTax.taxamount = (($scope.InvoiceTax.amounton - $scope.InvoiceTax.discount) * $scope.InvoiceTax.percentage / 100).round(2);
                    if (isNaN($scope.InvoiceTax.taxamount)) $scope.InvoiceTax.taxamount = 0;
                }
                else {
                    $scope.InvoiceTax.taxamount = 0;
                }
            }



            if (source != 'netamount') {


                if ($scope.InvoiceTax.typeinfo.amount) {
                    $scope.InvoiceTax.netamount = (($scope.GetNumber($scope.InvoiceTax.amounton) - $scope.GetNumber($scope.InvoiceTax.discount)) + $scope.GetNumber($scope.InvoiceTax.taxamount)).round(2);
                }
                else if ($scope.InvoiceTax.typeinfo.tax) {
                    if ($scope.InvoiceItem.isinclusivetax)
                        $scope.InvoiceTax.netamount = 0;
                    else
                        $scope.InvoiceTax.netamount = $scope.GetNumber($scope.InvoiceTax.taxamount);
                }
                else if ($scope.InvoiceTax.typeinfo.discount) {
                    if ($scope.InvoiceItem.isinclusivediscount)
                        $scope.InvoiceTax.netamount = 0;
                    else
                        $scope.InvoiceTax.netamount = $scope.GetNumber($scope.InvoiceTax.discount) * -1;
                } else {
                    if ($scope.InvoiceItem.isinclusiveall)
                        $scope.InvoiceTax.netamount = $scope.GetNumber($scope.InvoiceTax.amounton) * -1;
                    else
                        $scope.InvoiceTax.netamount = $scope.GetNumber($scope.InvoiceTax.amounton);
                }
            }
        }

        $scope.CalculateTotal = function (addInvoice) {
            if (addInvoice) {

                if ($scope.InvoiceTax.typeinfo.tax)
                    $scope.InvoiceItem.taxamount = $scope.InvoiceItem.taxamount + $scope.GetNumber($scope.InvoiceTax.taxamount);

                if ($scope.InvoiceTax.typeinfo.amount)
                    $scope.InvoiceItem.baseamount = $scope.InvoiceItem.baseamount + $scope.GetNumber($scope.InvoiceTax.amounton);

                if ($scope.InvoiceTax.typeinfo.directamount)
                    $scope.InvoiceItem.baseamount = $scope.InvoiceItem.baseamount + $rootScope.GetNumber($scope.InvoiceTax.netamount);

                if ($scope.InvoiceTax.typeinfo.discount)
                    $scope.InvoiceItem.discount = $scope.InvoiceItem.discount + $scope.GetNumber($scope.InvoiceTax.discount);
            }

            $scope.InvoiceItem.grossamount = $scope.GetNumber($scope.InvoiceItem.baseamount);
            //if($scope.InvoiceItem.isinclusivediscount)
            //    $scope.InvoiceItem.grossamount += $scope.GetNumber($scope.InvoiceItem.discount);
            //else
            $scope.InvoiceItem.grossamount -= $scope.GetNumber($scope.InvoiceItem.discount);

            //if($scope.InvoiceItem.isinclusivetax)
            //    $scope.InvoiceItem.grossamount -= $scope.GetNumber($scope.InvoiceItem.taxamount)
            //else
            $scope.InvoiceItem.grossamount += $scope.GetNumber($scope.InvoiceItem.taxamount)

            $scope.InvoiceItem.grossamount = $scope.InvoiceItem.grossamount - $scope.GetNumber($scope.InvoiceItem.plessdiscount).round(2);
            $scope.InvoiceItem.totalamount = ($scope.GetNumber($scope.InvoiceItem.grossamount) + $scope.GetNumber($scope.InvoiceItem.otheramount)).round(2);
            $scope.ComputeTDS();
        };
        /* $scope.CalculateRounding = function () {
           
            $scope.InvoiceItem.totalamount += $scope.InvoiceTax.amounton;
        };*/

        $scope.CalculatePless = function () {
            $scope.InvoiceItem.plessdiscount = $scope.InvoiceItem.plessvalue * $scope.InvoiceItem.noofpieces;
            $scope.CalculateTotal(false);
        }

        $scope.LRExpenseChanged = function (mode) {
            switch (mode) {
                case 'Save':
                    request = {
                        "Item": {
                            'id': $scope.Invoice.lrid,
                            'lrexpense': $scope.Invoice.lrnewexpense
                        },
                        "SecurityToken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "LrentryService/UpdateLRExpense",
                        data: srequest
                    }).success(function (result) {
                        if (result != null) {
                            $scope.Invoice.editlrexpense = false;
                            $scope.Invoice.lrexpense = $scope.Invoice.lrnewexpense;
                        }
                    });
                    break;
                case 'Cancel':
                    $scope.Invoice.editlrexpense = false;
                    break;
                case 'Show':
                    $scope.Invoice.editlrexpense = true;
                    $scope.Invoice.lrnewexpense = $scope.Invoice.lrexpense;
                    break;
            }
        }

        $scope.InitializeInvoiceList = function () {
            request = {
                "Item": {
                    lrid: $scope.LRInfo.lrid,
                    id: $scope.LRInfo.id,
                    includetdsrule: true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: srequest
            }).success(function (result) {

                if (angular.isArray(result) && result.length > 1) {
                    $scope.ShowSearch = true;
                    $scope.SearchResult = result;
                    $timeout(function () {
                        $scope.InvoiceConfig();
                    }, 50);
                    return;
                }

                $scope.Invoice.Items = result.Items;
                $scope.Invoice.lrid = result.lrid;
                $scope.Invoice.lrno = result.lrno;
                $scope.Invoice.receiveddate = result.receiveddate;
                $scope.Invoice.lrentryno = result.lrentryno;
                $scope.Invoice.lrdate = result.lrdate;
                $scope.Invoice.tdsrule = result.tdsinfo;

                $scope.Invoice.city = result.city;
                $scope.Invoice.transport = result.transport;
                $scope.Invoice.invoicedate = result.invoicedate;
                $scope.Invoice.invoiceno = result.invoiceno;
                $scope.Invoice.actualweight = result.actualweight;
                $scope.Invoice.receivedweight = result.receivedweight;
                $scope.Invoice.chargedweight = result.chargedweight;
                $scope.Invoice.noofbundels = result.noofbundels;
                $scope.Invoice.noofboxes = result.noofboxes;
                $scope.Invoice.receivinglocationid = result.receivinglocationid;

                $scope.Invoice.agentcommission = result.agentcommission;


                $scope.Invoice.frightcharge = result.frightcharge;
                $scope.Invoice.loadingcharge = result.loadingcharge;
                $scope.Invoice.insurancecharge = result.insurancecharge;
                $scope.Invoice.packingcharge = result.packingcharge;
                $scope.Invoice.forwardcharge = result.forwardcharge;
                $scope.Invoice.otherscharge = result.otherscharge;
                $scope.Invoice.agentpercentage = result.agentpercentage;

                $scope.Invoice.entrydate = result.entrydate;
                $scope.Invoice.status = result.status;
                $scope.Invoice.modeofdelivery = result.modeofdelivery;
                $scope.Invoice.supplierid = result.supplierid;
                $scope.Invoice.lrexpense = result.lrexpense;
                $scope.Invoice.editlrexpense = false;
                $scope.Invoice.lrnoInfo = result.lrentryno + " / " + result.lrno;
                $scope.InvoiceItem = (JSON.parse(JSON.stringify($scope.InvoiceItemTemplate)));
                $scope.InvoiceTax = (JSON.parse(JSON.stringify($scope.InvoiceTaxTemplate)));
                $scope.InvoiceItem.entrydate = result.entrydate;
                $scope.InvoiceItem.invoicedate = result.invoicedate;
                $scope.InvoiceItem.invoiceno = result.invoiceno;
                if ($scope.Invoice.Items == 'undefined' || $scope.Invoice.Items == null || $scope.Invoice.Items.length == 0) {
                    $scope.InvoiceItem.noofbundle = result.noofbundels;
                    $scope.InvoiceItem.noofpieces = result.noofpieces;
                }
                $scope.Invoice.fixedcompanyid = result.companyid;
                $scope.ShowSearch = false;
                $scope.Navigation.SetMode('EDIT');
                $timeout(function () {
                    $scope.FilterCompany();
                    $scope.FilterSupplierCompany();
                    $scope.SearchPO();
                }, 10);
            }).error(function (result) {
            });
        };

        $scope.Download = function () {
            $timeout(function () {
                $rootScope.Download('LrinvoiceService/PrintInvoicePDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.Invoice.Items[0].id, 'invoice.pdf', 'application/pdf');
                return;
            }, 10);
        }

        $scope.Print = function () {
            var list = [];

            list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
            list.push($rootScope.PrintFormater.Bold("LR EXPENSE SHEET"));
            list.push($rootScope.PrintFormater.Normal("DATE           :" + $filter('date')(new Date(), 'dd-MM-yyyy')));
            list.push($rootScope.PrintFormater.Normal("TIME           :" + $filter('date')(new Date(), 'hh-mm a')));
            list.push($rootScope.PrintFormater.Normal("_________________________________________________", 49));
            list.push($rootScope.PrintFormater.Normal("LR DATE        :" + $filter('date')($scope.Invoice.lrdate, 'dd-MM-yyyy')));
            list.push($rootScope.PrintFormater.Normal("LR NO          :" + $scope.Invoice.lrno));
            list.push($rootScope.PrintFormater.Normal("LR ENTRY NO    :" + $scope.Invoice.lrentryno));
            list.push($rootScope.PrintFormater.Normal("CITY           :" + $scope.SearchRef($scope.Invoice.receivinglocationid, $scope.ReceivingLocations)));
            list.push($rootScope.PrintFormater.Normal("TRANSPORT      :" + $scope.Invoice.transport));
            list.push($rootScope.PrintFormater.Normal("INVOICE NO     :" + $scope.Invoice.invoiceno));
            list.push($rootScope.PrintFormater.Normal("INVOICE DATE   :" + $filter('date')($scope.Invoice.invoicedate, 'dd-MM-yyyy')));
            list.push($rootScope.PrintFormater.Normal("DELIVERY DATE  :" + $filter('date')($scope.Invoice.receiveddate, 'dd-MM-yyyy')));
            list.push($rootScope.PrintFormater.Normal("POST DATE      :" + $filter('date')($scope.Invoice.Items[0].createdon, 'dd-MM-yyyy')));
            list.push($rootScope.PrintFormater.Normal("AC.WEIGHT      :" + $filter('IndianNumber')($scope.Invoice.actualweight, 2)));
            list.push($rootScope.PrintFormater.Normal("CH.WEIGHT      :" + $filter('IndianNumber')($scope.Invoice.chargedweight), 2));
            list.push($rootScope.PrintFormater.Normal("DELI.WEIGHT    :" + $filter('IndianNumber')($scope.Invoice.receivedweight), 2));
            list.push("\r\n");
            list.push($rootScope.PrintFormater.Normal("BUNDLES/BOXES  :" +
                $filter('IndianNumber')($scope.Invoice.noofbundels, 0) + '/' +
                $filter('IndianNumber')($scope.Invoice.noofboxes, 0)));
            list.push($rootScope.PrintFormater.Normal("_________________________________________________", 49));

            var leftlist = [];
            var rightlist = [];

            rightlist.push("FRIGHT    :" + $rootScope.PrintFormater.Normal($filter('IndianNumber')($scope.Invoice.frightcharge, 2), 12, false));
            rightlist.push("LOADING   :" + $rootScope.PrintFormater.Normal($filter('IndianNumber')($scope.Invoice.loadingcharge, 2), 12, false));
            rightlist.push("INSURANCE :" + $rootScope.PrintFormater.Normal($filter('IndianNumber')($scope.Invoice.insurancecharge, 2), 12, false));
            rightlist.push("PACKING   :" + $rootScope.PrintFormater.Normal($filter('IndianNumber')($scope.Invoice.packingcharge, 2), 12, false));
            rightlist.push("FORWARD   :" + $rootScope.PrintFormater.Normal($filter('IndianNumber')($scope.Invoice.forwardcharge, 2), 12, false));
            rightlist.push("OTHER     :" + $rootScope.PrintFormater.Normal($filter('IndianNumber')($scope.Invoice.otherscharge, 2), 12, false));
            rightlist.push("AGENT     :" + $rootScope.PrintFormater.Normal($filter('IndianNumber')($scope.Invoice.agentcommission, 2), 12, false));
            rightlist.push("           " + $rootScope.PrintFormater.Normal("____________", 12, true));
            rightlist.push("           " + $rootScope.PrintFormater.Normal($filter('IndianNumber')($scope.Invoice.lrexpense, 2), 12, false));
            rightlist.push("           " + $rootScope.PrintFormater.Normal("____________", 12, true));

            var totalbillvalue = 0;
            $.each($scope.Invoice.Items, function (k, v) {

                totalbillvalue += v.totalamount;

                leftlist.push("INV.NO    :" + $rootScope.PrintFormater.Normal(v.invoiceno, 14, false));
                leftlist.push("AMOUNT    :" + $rootScope.PrintFormater.Normal($filter('IndianNumber')(v.baseamount, 0), 14, false));

                var discount = 0;
                if (v.discount > 0) {
                    discount = (v.discount / v.baseamount * 100).toFixed(2)
                }

                leftlist.push($rootScope.PrintFormater.Normal("DIS " +
                    $rootScope.PrintFormater.Align($filter('IndianNumber')(discount, 2), 5, true) + '%:' +
                    $rootScope.PrintFormater.Align($filter('IndianNumber')(v.discount, 0), 14, true)));

                leftlist.push($rootScope.PrintFormater.Normal("PLess" +
                    $rootScope.PrintFormater.Align($filter('IndianNumber')(v.plessvalue, 0), 4, true) + ' :' +
                    $rootScope.PrintFormater.Align($filter('IndianNumber')(v.plessdiscount, 0), 14, true)));

                leftlist.push($rootScope.PrintFormater.Normal("_________________________"));

                leftlist.push("AMOUNT    :" + $rootScope.PrintFormater.Normal($filter('IndianNumber')(v.totalamount, 0), 14, false));

                leftlist.push($rootScope.PrintFormater.Normal("_________________________"));
                leftlist.push($rootScope.PrintFormater.Normal("                         "));
            });

            for (var i = leftlist.length; rightlist.length > i; i++) {
                leftlist.push($rootScope.PrintFormater.Normal("                         "));
            }

            for (var i = 0; i < leftlist.length; i++) {
                if (rightlist.length > i) {
                    list.push(leftlist[i] + ' ' + rightlist[i]);
                }
                else {
                    list.push(leftlist[i]);
                }
            }


            var expensepercentage = Math.ceil($scope.Invoice.lrexpense / totalbillvalue * 100 / 0.5) * 0.5;
            list.push($rootScope.PrintFormater.Bold("Expense", 9, true) +
                $rootScope.PrintFormater.Bold(expensepercentage.toFixed(2).toString() + "%", 10, false));

            list.push($rootScope.PrintFormater.Cutter());
            list.push(('\r\n'));
            var printdocument = window.btoa(list.join('\r\n'));
            $rootScope.Sent2Printer(printdocument);
        }

        $scope.CompanyChanged = function () {
            var comp = $scope.BuyingCompany.find(function (item) {
                return item.id == $scope.InvoiceItem.companyid;
            });
            if (comp && comp != null) {
                $scope.InvoiceItem.isvendorbill = comp.internalbuyer;
                $scope.companygst = comp.gstno;
            }
        };

        $scope.SupplierCompChanged = function () {
            var supp = $scope.SupplierCompany.find(function (item) {
                return item.id == $scope.InvoiceItem.suppliercompanyid;
            });
            if (supp != 'undefined' && supp != null)
                $scope.suppliergst = supp.gst;
        };

        $scope.FilterCompany = function () {
            if ($scope.BuyingCompany && $scope.BuyingCompany != null) {
                if ($scope.Invoice.fixedcompanyid > 0) {
                    var filtercompany = [];
                    $.each($scope.BuyingCompany, function (k, v) {
                        if (v.id == $scope.Invoice.fixedcompanyid)
                            filtercompany.push(v);
                    })
                    $scope.BuyingCompany = filtercompany;
                    if (filtercompany.length == 1) {
                        $scope.InvoiceItem.companyid = 0;
                        $timeout(function () {
                            $scope.InvoiceItem.companyid = filtercompany[0].id;
                            $scope.CompanyChanged();
                        }, 100);
                    }
                }
            } else {
                $timeout(function () {
                    $scope.FilterCompany();
                }, 50);
            }
        }

        if (typeof ($scope.LRInfo.lrid) == 'undefined' || $scope.LRInfo.lrid == 0) {
            $scope.SwitchPanel();
        }
        else {
            $scope.InitializeInvoiceList();
        }

        $scope.calculator = function (source) {
            if (source == 'amounton') {
                $scope.InvoiceTax.amounton = $rootScope.GetNumber(eval($scope.InvoiceTax.amounton));
                $scope.Compute(source);
            }
            else if (source == 'billvalue')
                $scope.InvoiceItem.billvalue = $rootScope.GetNumber(eval($scope.InvoiceItem.billvalue));
        }

        $scope.AddTaxItem = function () {
            if ($scope.InvoiceTax.typeinfo.tax) {
                if (typeof ($scope.InvoiceTax.taxinfo) != 'undefined' || $scope.InvoiceTax.taxinfo != null) {
                    $scope.InvoiceTax.taxid = $scope.InvoiceTax.taxinfo.id;
                }
            }

            if ($scope.InvoiceTax.type == undefined || $scope.InvoiceTax.type == null) {
                $rootScope.Toaster.Error("Please select type of entry");
                return;
            }

            if ($scope.InvoiceTax.type == 0 &&
                ($scope.InvoiceTax.taxid == undefined || $scope.InvoiceTax.taxid == null || $scope.InvoiceTax.taxid == 0)) {
                $rootScope.Toaster.Error("Please select the tax");
                return;
            }

            $scope.CalculateTotal(true);
            if (typeof ($scope.InvoiceItem.Lrinvoicetaxitems) == 'undefined' || $scope.InvoiceItem.Lrinvoicetaxitems == null)
                $scope.InvoiceItem.Lrinvoicetaxitems = [];
            $scope.InvoiceItem.Lrinvoicetaxitems.push($scope.InvoiceTax);
            $scope.InvoiceTax = (JSON.parse(JSON.stringify($scope.InvoiceTaxTemplate)));
            $scope.setFocusentrytype();
            $scope.ComputeLRExpense();
            //$scope.InvoiceItem.Lrinvoicetaxitems[$scope.InvoiceItem.Lrinvoicetaxitems.length - 1].id = Math.abs($scope.InvoiceItem.Lrinvoicetaxitems[$scope.InvoiceItem.Lrinvoicetaxitems.length - 1].id);
        }

        $scope.SetPONo = function () {
            var poitem = $scope.POList.find(function (item) {
                return item.id == $scope.InvoiceItem.purchaseorderid;
            });

            if (poitem && poitem != null) {
                $scope.InvoiceItem.pono = poitem.name;
            }
        }

        $scope.AddInvoice = function () {

            if (typeof ($scope.InvoiceItem.companyid) == 'undefined' || $scope.InvoiceItem.companyid == null || $scope.InvoiceItem.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Error("Please select buying company name");
                return;
            }

            if (typeof ($scope.Invoice.fixedcompanyid) != 'undefined' &&
                $scope.Invoice.fixedcompanyid > 0 &&
                $scope.InvoiceItem.companyid != $scope.Invoice.fixedcompanyid) {
                $rootScope.Toaster.Error("Retail location belongs to different company, please select right company to the retail location.");
            }

            if ($scope.InvoiceItem.suppliercompanyid == null || $scope.InvoiceItem.suppliercompanyid == 0) {
                $scope['setFocussuppliercompanyid']();
                $rootScope.Toaster.Error("Please select supplier company");
                return;
            }
            if ($scope.InvoiceItem.invoiceno == null || $scope.InvoiceItem.invoiceno == 0) {
                $scope['setFocusinvoiceno']();
                $rootScope.Toaster.Error("Please select Invoice No");
                return;
            }
            if ($scope.InvoiceItem.invoicedate == null || $scope.InvoiceItem.invoicedate == "") {
                $scope['setFocusinvoicedate']();
                $rootScope.Toaster.Error("Please select Invoice Date");
                return;
            }

            if ($scope.InvoiceItem.entrydate == null || $scope.InvoiceItem.entrydate == "") {
                $scope['setFocusentrydate']();
                $rootScope.Toaster.Error("Please select Entry Date");
                return;
            }

            var entrydate = new Date($scope.InvoiceItem.entrydate);
            var invoiedate = new Date($scope.InvoiceItem.invoicedate);

            entrydate = new Date(entrydate.getFullYear(), entrydate.getMonth(), entrydate.getDate());
            invoiedate = new Date(invoiedate.getFullYear(), invoiedate.getMonth(), invoiedate.getDate());

            if (entrydate < invoiedate) {
                $rootScope.Toaster.Error("Entry Date should be greater than Invoice date");
                return;
            }


            if ($scope.InvoiceItem.billvalue == null || $scope.InvoiceItem.billvalue == 0) {
                $scope['setFocusbillvalue']();
                $rootScope.Toaster.Error("Please enter Bill value");
                return;
            }

            //if ($scope.companygst == undefined || $scope.companygst == null || $scope.companygst == '') {
            //    $rootScope.Toaster.Error("Please update your company gst no");
            //    return;
            //}

            if (($scope.suppliergst == undefined || $scope.suppliergst == null || $scope.suppliergst == '') &&
                $scope.InvoiceItem.isinterstatesale) {
                $rootScope.Toaster.Error("Please update gst no for the supplier before make interstate purchase");
                return;
            }

            if ($scope.companygst != '' &&
                $scope.suppliergst != '') {
                var isigst = $scope.suppliergst.substring(0, 2) != $scope.companygst.substring(0, 2);
                if ($scope.InvoiceItem.isinterstatesale != isigst) {
                    if (isigst) {
                        $rootScope.Toaster.Error("Supplier from other state, please mark as interstate sale");
                    }
                    else {
                        $rootScope.Toaster.Error("Supplier from local state, please remove interstate sale mark");
                    }
                    return;
                }
            }

            //if ($scope.InvoiceItem.noofbundle == null || $scope.InvoiceItem.noofbundle == 0) {
            //    $rootScope.Toaster.Error("Please Enter NoofBundle");
            //    return;
            //}

            //if ($scope.InvoiceItem.noofpieces == null || $scope.InvoiceItem.noofpieces == 0) {
            //    $rootScope.Toaster.Error("Please Enter  NoOfPices");
            //    return;
            //}

            //if (typeof ($scope.Invoice.Items) == 'undefined') $scope.Invoice.Items = [];
            //if (typeof ($scope.InvoiceItem.expensepercentage) == 'undefined' || $scope.InvoiceItem.expensepercentage == '')
            //    $scope.InvoiceItem.expensepercentage = 0;

            //$scope.InvoiceItem.expensepercentage = $scope.GetNumber($scope.InvoiceItem.expensepercentage);
            //$scope.Invoice.lrexpense = $scope.GetNumber($scope.Invoice.lrexpense);

            if ($scope.int_finance) {
                $.each($scope.InvoiceItem.Lrinvoicetaxitems, function (k, v) {
                    v._idx = k;
                })
                var amountwithtaxable = $scope.InvoiceItem.Lrinvoicetaxitems.find(function (item) {
                    return item.taxid > 0 && item.type == 0;
                });
                if ($scope.InvoiceItem.Lrinvoicetaxitems.length == 1) {
                    if (amountwithtaxable == undefined || amountwithtaxable == null) {
                        $rootScope.Toaster.Error("Nontaxable purchase not allowed, please select zero tax");
                        return false;
                    }
                }
                var amountwithouttaxable = $scope.InvoiceItem.Lrinvoicetaxitems.find(function (item) {
                    return item.taxid <= 0 && item.type == 0;
                });
                var taxable = $scope.InvoiceItem.Lrinvoicetaxitems.find(function (item) {
                    return item.taxid > 0 && item.type == 1;
                });
                var lasttaxidx = -1;
                if (amountwithouttaxable && amountwithouttaxable != null) {
                    lasttaxidx = amountwithouttaxable._idx;
                }
                if (taxable && taxable != null && taxable._idx > lasttaxidx) {
                    lasttaxidx = taxable._idx;
                }
                var discountconflict = $scope.InvoiceItem.Lrinvoicetaxitems.find(function (item) {
                    return item._idx > lasttaxidx && (item.type == 2 || item.type == 3);
                });
                if (discountconflict && discountconflict != null) {
                    $rootScope.Toaster.Error("Discount/OnDiscount Can't be added at end of the invoice items");
                    return false;
                }
            }


            var supcomp = $scope.SupplierCompany.find(function (item) {
                return item.id == $scope.InvoiceItem.suppliercompanyid;
            });

            if (supcomp && supcomp != null) {
                var supplier = $scope.Suppliers.find(function (item) {
                    return item.id == supcomp.supplierid;
                });

                if (supplier && supplier != null && supplier.minpurdisctpercentage &&
                    supplier.minpurdisctpercentage > 0) {
                    var discount = parseFloat($scope.InvoiceItem.discount);
                    var baseamount = parseFloat($scope.InvoiceItem.baseamount);
                    if (isNaN(discount)) discount = 0;
                    if (isNaN(baseamount)) baseamount = 0;
                    var ediscount = (($scope.InvoiceItem.baseamount * supplier.minpurdisctpercentage / 100).round(2) - 1);
                    if (ediscount > discount) {
                        $rootScope.Confirm('Discount', 'Supplier minimum discount not given, do you want to continue?', function () {
                            $timeout(function () {
                                if ($scope.Invoice.Items.indexOf($scope.InvoiceItem) < 0)
                                    $scope.Invoice.Items.push($scope.InvoiceItem);
                                $scope.InvoiceItemTemplate.invoicedate = $scope.InvoiceItem.invoicedate;
                                $scope.InvoiceItem = (JSON.parse(JSON.stringify($scope.InvoiceItemTemplate)));
                            }, 10);
                        });
                        return;
                    }
                }
            }

            if ($scope.Invoice.Items.indexOf($scope.InvoiceItem) < 0) {
                if ($scope.Invoice.Items.length > 1) {
                    var exist = $scope.Invoice.Items.find(function (item) {
                        item.invoiceno == $scope.Invoice.invoiceno;
                    });
                    if (exist && exist != null) {
                        $rootScope.Toaster.Error("Invoice No already added.");
                        return;
                    }
                }
                $scope.Invoice.Items.push($scope.InvoiceItem);
            }
            $scope.InvoiceItemTemplate.invoicedate = $scope.InvoiceItem.invoicedate;
            $scope.InvoiceItem = (JSON.parse(JSON.stringify($scope.InvoiceItemTemplate)));
            //$scope.ComputeExpense();
        }


        $scope.ComputeLRExpense = function () {
            var lrexpense = 0;
            //if (typeof ($scope.Invoice.Items) == 'undefined' || $scope.Invoice.Items.indexOf($scope.Invoice.Items) < 0) {

            $.each($scope.InvoiceItem.Lrinvoicetaxitems, function (k, v) {
                var rowtype = $scope.EntryTypes.find(function (item) {
                    return item.id == v.type;
                });
                if (rowtype && rowtype.lrexpense) {
                    lrexpense += v.netamount;
                }
            });

            //}
            //if (typeof ($scope.Invoice.Items) != 'undefined') {
            //    $.each($scope.Invoice.Items, function (kk, vv) {

            //        $.each(vv.Lrinvoicetaxitems, function (k, v) {
            //            var rowtype = $scope.EntryTypes.find(function (item) {
            //                return id == v.type;
            //            });
            //            if (rowtype && rowtype.lrexpense) {
            //                lrexpense += v.netamount;
            //            }
            //        });

            //    });
            //}

            //$scope.Invoice.lrexpense = lrexpense;
            $scope.ComputeExpense();
        }

        $scope.ComputeExpense = function () {
            var totalbillvalue = 0;
            $.each($scope.Invoice.Items, function (k, v) {
                totalbillvalue += $scope.GetNumber(v.billvalue);
            });

            if (typeof ($scope.Invoice.Items) == 'undefined' || $scope.Invoice.Items.indexOf($scope.InvoiceItem) < 0) {
                totalbillvalue += $scope.InvoiceItem.totalamount;
            }

            $.each($scope.Invoice.Items, function (k, v) {
                var billpercentage = totalbillvalue / $scope.GetNumber(v.billvalue) * 100;
                var billexpensevalue = $scope.Invoice.lrexpense * billpercentage / 100;
                if (billexpensevalue > 0)
                    v.expensepercentage = $scope.GetNumber($scope.GetNumber($scope.Invoice.agentcommision) +
                        Math.ceil(billexpensevalue / $scope.GetNumber(v.billvalue) * 100 / 0.5) * 0.5);
                else
                    v.expensepercentage = 0;
            });


            if (typeof ($scope.Invoice.Items) == 'undefined' || $scope.Invoice.Items.indexOf($scope.InvoiceItem) < 0) {
                var billpercentage = totalbillvalue / $scope.InvoiceItem.totalamount * 100;
                var billexpensevalue = $scope.Invoice.lrexpense * billpercentage / 100;
                if (billexpensevalue > 0)
                    $scope.InvoiceItem.expenseperpiece = $scope.GetNumber($scope.GetNumber($scope.Invoice.agentcommision) +
                        Math.ceil(billexpensevalue / $scope.GetNumber(totalbillvalue) * 100 / 0.5) * 0.5);
                else
                    $scope.InvoiceItem.expenseperpiece = 0;
            }
        }

        //$scope.amountChanged = function () {
        //    if ($scope.AutoCalculation)
        //        $scope.taxAmountChanged();
        //};

        //$scope.taxAmountChanged = function () {
        //    if ($scope.AutoCalculation) {
        //        $scope.InvoiceTax.discount = (((parseFloat($scope.InvoiceTax.amounton)) * (parseFloat($scope.InvoiceTax.discountpercentage))) / 100).round(2);
        //        if (isNaN($scope.InvoiceTax.discount))
        //            $scope.InvoiceTax.discount = 0;
        //    }

        //    $scope.InvoiceTax.taxamount = ((parseFloat($scope.InvoiceTax.amounton) - $scope.InvoiceTax.discount) * $scope.InvoiceTax.percentage / 100).round(2);
        //    if (isNaN($scope.InvoiceTax.taxamount))
        //        $scope.InvoiceTax.taxamount = 0;


        //    $scope.InvoiceTax.netamount = parseFloat($scope.InvoiceTax.amounton) - $scope.InvoiceTax.discount + $scope.InvoiceTax.taxamount;
        //    if (isNaN($scope.InvoiceTax.netamount))
        //        $scope.InvoiceTax.netamount = 0;
        //    else
        //        $scope.InvoiceTax.netamount = $scope.InvoiceTax.netamount.round(2);
        //    $scope.Recalculate(true);

        //};

        //$scope.discountChanged = function () {
        //    var discountalue = parseFloat($scope.InvoiceTax.discount);
        //    if (isNaN(discountalue))
        //        discountalue = 0;

        //    $scope.InvoiceTax.discountpercentage = Math.round(discountalue / parseFloat($scope.InvoiceTax.amounton) * 100, 2);

        //    $scope.taxAmountChanged();
        //}

        //$scope.DiscountPercentageChanged = function () {
        //    $scope.InvoiceTax.discount = (((parseFloat($scope.InvoiceTax.amounton)) * (parseFloat($scope.InvoiceTax.discountpercentage))) / 100).round(2);
        //    if (isNaN($scope.InvoiceTax.discount))
        //        $scope.InvoiceTax.discount = 0;
        //};

        //$scope.Recalculate = function (isrecursive) {
        //    $scope.DiscountPercentageChanged();
        //    if (!isrecursive)
        //        $scope.taxAmountChanged();
        //}

        $scope.typeChanged = function () {

            $scope.InvoiceTax.typeinfo = {};
            if ($scope.InvoiceTax.type != null) {
                var taxitem = $scope.EntryTypes.find(function (item) {
                    return (item.id == $scope.InvoiceTax.type);
                });
                if (taxitem != null) {
                    $scope.InvoiceTax.typeinfo = taxitem;
                };
            };
            $scope.Compute('type');
        };

        $scope.taxChanged = function () {
            $scope.InvoiceTax.taxinfo = {};
            if ($scope.InvoiceTax.taxid != null) {
                var taxitem = $scope.purchaseTax.find(function (item) {
                    return (item.code == $scope.InvoiceTax.taxid);
                });
                if (taxitem != null) {
                    $scope.InvoiceTax.taxinfo = taxitem;
                    $scope.InvoiceTax.percentage = taxitem.taxpercentage;
                };
            };
            $scope.Compute('tax');
        };


        $scope.Clear = function () {
            $scope.ErrorMessage = "";
            $scope.ErrorStyle = "";
            $scope.Invoice = (JSON.parse(JSON.stringify($scope.InvoiceTemplate)));
            $scope.InvoiceItem = (JSON.parse(JSON.stringify($scope.InvoiceItemTemplate)));
        };

        $scope.ErrorMessage = "";

        $scope.Save = function (movenext) {
            $scope.SaveRequest = true;

            if ($scope.Invoice.Items == 'undefined' || $scope.Invoice.Items == null || $scope.Invoice.Items.length == 0) {
                $rootScope.Toaster.Error("Invoice item missing.");
                $scope.SaveRequest = false;
                return;
            }

            var request = {
                "Item": $scope.Invoice
            }

            $http({
                method: 'POST',
                url: "LrinvoiceService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.lrid > 0) {

                    if (result.Items[0].id == 0) {
                        $scope.ErrorMessage = result.ErrorMessage;
                        $rootScope.Toaster.Error(result.ErrorMessage);
                        $scope.SaveRequest = false;
                        return;
                    }
                    var invoiceids = '';
                    $.each(result.Items, function (k, v) {
                        if (invoiceids.length == 0)
                            invoiceids += v.id;
                        else
                            invoiceids += ',' + v.id;
                    })
                    if (movenext) {
                        $location.url('/LRInvoiceItems?invoiceid=' + invoiceids);
                        $rootScope.Toaster.Info("Saved Successfully.!");
                    }
                    else {
                        $rootScope.Toaster.Info("Saved Successfully.!");
                        $scope.Invoice = (JSON.parse(JSON.stringify($scope.InvoiceTemplate)));
                        $scope.InvoiceItem = (JSON.parse(JSON.stringify($scope.InvoiceItemTemplate)));
                    }
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }
                //if (result.id > 0) {
                //    $scope.SearchResult.push(result);
                //}
                //else {
                //    $scope.ErrorMessage = result.ErrorMessage;
                //}

            }).error(function (result) {
                $scope.ErrorMessage = result;
                $scope.SaveRequest = false;
            });
        }

        $scope.SearchCondition = {
            lrentryno: "",
            lrno: "",
            supplierid: 0,
            fromdate: "",
            todate: "",
            entryfromdate: "",
            entrytodate: ""
        }
        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.InvoiceConfig();
            }).error(function (result) {
            });
        }

        $scope.InvoiceConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddReferenceColumn('suppliercompanyid', 'Supplier', 200, $scope.SupplierCompany);
            igrid.AddSimpleColumn('suppliercompany', 'Supplier company', 120);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('barcodedon', 'Barcoded On', 100, 'dd-MM-yyyy');            
            igrid.AddFormatedColumn('baseamount', 'Base Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalamount', 'TotalAmount', 110, 'N2', 2).total = true;
            igrid.AddFormatedColumn('noofbundle', 'Bundle', 80, 'N0', 0).total = true;
            igrid.AddCustomColumn('hastds', 'TDS', 60, function (item, col) {
                return item.hastds == true ? "YES" : "NO";
            });
            igrid.AddCustomColumn('hastcs', 'TCS', 60, function (item, col) {
                return item.hastcs == true ? "YES" : "NO";
            });
            igrid.AddFormatedColumn('tcsamount', 'TCS Amount', 80, 'N2', 2).total = true;

            igrid.AddSimpleColumn('createduser', 'Created By', 80);
            igrid.AddFormatedColumn('createdon', 'Created On', 100, 'dd-MM-yyyy hh:mm');
            igrid.AddSimpleColumn('modifieduser', 'Modified By', 80);
            igrid.AddFormatedColumn('modifiedon', 'Modified On', 100, 'dd-MM-yyyy hh:mm');

            if ($scope.supportapproval) {
                igrid.AddCustomColumn('hasapprove', 'Approved', 60, function (item, col) {
                    return item.hasapprove == true ? "YES" : "NO";
                });
            }

            var commands = "";
            if ($rootScope.UserInfo.p_LRINVOICE.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_LRINVOICE.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_LRINVOICE.modify || $rootScope.UserInfo.p_LRINVOICE.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 130, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageInvoiceTypeItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageInvoiceTypeItem('Delete', args.item);
                    });
                });
            }

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                if (item.hasapprove) {
                    item.style = 'igridbigrowswithgreen';
                    return { cssClasses: 'igridbigrowswithgreen' };
                } else if(item.settled){
                    item.style = 'igridbigrowswithred';
                    return { cssClasses: 'igridbigrowswithred' };
                }
            };
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.selectedcount = 0;
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }
        $scope.selectedcount = 0;
        $scope.ChangeStatus = function (approved) {
            var ids = [];
            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                ids.push(v.id);
            });
            var request = {
                "Item": {
                    'hasapprove': approved,
                    'ids': ids.join(',')
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/ChangeStatus",
                data: srequest
            }).success(function (result) {
                $scope.igrid.SetSelectedRows([]);
            });
        }

        $scope.ChangeSettled = function (settled) {
            var ids = [];
            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                ids.push(v.id);
            });
            var request = {
                "Item": {
                    'changetosettled': settled,
                    'ids': ids
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/ChangeSettled",
                data: srequest
            }).success(function (result) {
                $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                    v.settled = settled;
                });
                $scope.igrid.SetSelectedRows([]);
            });
        }

        $scope.ImportInvoice = function () {
            var dialog = ngDialog.open({
                template: 'CloneInvoice',
                data: [$scope],
                width: '700px',
                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService',
                    function ($scope, $timeout, $http, hotkeys, $rootScope, restService) {
                        $scope.ParentScop = $scope.ngDialogData[0];
                        $scope.HasInvoice = false;
                        $scope.Invoice = {};
                        $scope.ProductList = [];
                        $scope.igrid = null;

                        restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
                            $scope.Products = results;
                        });

                        $scope.ConfigProductGrid = function () {
                            if ($scope.igrid != null)
                                $scope.igrid.destroy();

                            var igrid = new InfyGrid('grdinvoicedetail', '270px', '100%', false, false, false);
                            igrid.options.showHeaderRow = true;
                            igrid.EnableSerial($scope.ProductList);
                            igrid.AddReferenceColumn('productid', 'Product', 150, $scope.Products);
                            igrid.AddFormatedColumn('buyingprice', 'Cost', 80, 'N2', 2);
                            igrid.AddFormatedColumn('buyingqty', 'Qty', 80, 'N2', 2).total = true;
                            igrid.AddFormatedColumn('purchasediscount', 'Discount', 80, 'N2', 2);
                            igrid.AddFormatedColumn('sellingprice', 'Price', 80, 'N2', 2);
                            igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N0', 0);
                            igrid.AddFormatedColumn('amount', 'Amount', 80, 'N2', 2).total = true;
                            igrid.EnableTotal();
                            igrid.Initialize();
                            $scope.igrid = igrid;
                            $scope.igrid.UpdateData($scope.ProductList);
                        }

                        $scope.Clone = function () {
                            if ($scope.Invoice.invoiceno == $scope.oldinvoiceno) {
                                $rootScope.Toaster.Error("Please enter the new invoice no");
                                return;
                            }
                            $scope.Invoice.id = 0;
                            $scope.Invoice.lrid = 0;
                            $scope.closeThisDialog($scope.Invoice);
                        };

                        $scope.SearchInvoice = function () {
                            request = {
                                "Item": {
                                    'invoiceno': $scope.invoiceno,
                                    'supplierid': $scope.ParentScop.Invoice.supplierid,
                                    'showitems': true
                                },
                                "SecurityToken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "LrinvoiceService/Search",
                                data: srequest
                            }).success(function (result) {
                                if (result && result.length == 1) {
                                    $scope.HasInvoice = true;
                                    $scope.oldinvoicedate = result[0].invoicedate;
                                    $scope.oldinvoiceno = result[0].invoiceno;
                                    $scope.Invoice = result[0];
                                    $scope.ProductList = [];

                                    if ($scope.Invoice.Lrinvoicetaxitems &&
                                        $scope.Invoice.Lrinvoicetaxitems != null) {
                                        $.each($scope.Invoice.Lrinvoicetaxitems, function (k, v) {
                                            v.id = 0;
                                        });
                                    }

                                    if ($scope.Invoice.Lrinvoiceitems &&
                                        $scope.Invoice.Lrinvoiceitems != null) {
                                        $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                                            v.id = 0;
                                            var exist = $scope.ProductList.find(function (item) {
                                                return item.productid == v.productid &&
                                                    item.buyingprice == v.buyingprice &&
                                                    item.purchasediscount == v.purchasediscount;
                                            });

                                            if (exist == undefined || exist == null) {
                                                exist = {
                                                    'productid': v.productid, 'buyingprice': v.buyingprice, 'purchasediscount': v.purchasediscount,
                                                    'sellingprice': v.sellingprice, 'mrp': v.mrp, 'items': [], 'buyingqty': 0, 'purchasetotaldiscount': 0,
                                                    'amount': 0, 'purchasetax': 0
                                                };
                                                $scope.ProductList.push(exist);
                                            }
                                            exist.items.push(v);
                                            exist.purchasetotaldiscount += v.purchasetotaldiscount;
                                            exist.purchasetax += v.purchasetax;
                                            exist.buyingqty += v.buyingqty;
                                            exist.amount += v.amount;
                                        });
                                    }
                                    $scope.ConfigProductGrid();
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

                        if ($scope.Invoice.Items == undefined ||
                            $scope.Invoice.Items == null) {
                            $scope.Invoice.Items = [];
                        }
                        $scope.Invoice.Items.push(data.value);
                        $scope.Save(false);
                    }
                }
            });
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == "lrinvoice")
                subtitle = 'INVOICE ENTRY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');

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

        $scope.ManageInvoiceTypeItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete invoice - no : ' + item.invoiceno + "?",
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
                                    url: "LrinvoiceService/Delete",
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
                $scope.LRInfo.lrid = item.lrid;
                $scope.LRInfo.id = item.id;
                $scope.InitializeInvoiceList();
                $scope.Navigation.SetMode('EDIT');
                // $scope.ShowSearch = false;
                // $scope.InvoiceItem = item;

                //  $scope.Invoice.Items.splice($scope.Invoice.Items.indexOf(item), 1);

            } else if (action == "EditInvoice") {
                $scope.ShowSearch = false;
                $scope.InvoiceItem = item;
                $scope.FilterCompany();
                $scope.FilterSupplierCompany();
                $scope.ComputeLRExpense();
            }
            else if (action == "DeleteInvoice") {
                if (item.hasitems) {
                    $rootScope.Toaster.Error("Invoice which are having purchase details can't be deleted.");
                    return;
                }
                $scope.Invoice.Items.splice($scope.Invoice.Items.indexOf(item), 1);
            }
            else if (action == "deletetax") {
                $scope.InvoiceItem.Lrinvoicetaxitems.splice($scope.InvoiceItem.Lrinvoicetaxitems.indexOf(item), 1);
                if ($scope.InvoiceItem.Lrinvoicetaxitems.length == 0) {
                    $scope.InvoiceItem.taxamount = 0;
                    $scope.InvoiceItem.baseamount = 0;
                    $scope.InvoiceItem.discount = 0;
                }
                else {
                    $scope.InvoiceItem.taxamount -= $scope.GetNumber(item.taxamount);
                    $scope.InvoiceItem.baseamount -= $scope.GetNumber(item.amounton);
                    $scope.InvoiceItem.discount -= $scope.GetNumber(item.discount);
                }
                $scope.CalculateTotal(false);
                $scope.ComputeLRExpense();
            }
            else if (action == "edittax") {
                $scope.InvoiceItem.Lrinvoicetaxitems.splice($scope.InvoiceItem.Lrinvoicetaxitems.indexOf(item), 1);
                if ($scope.InvoiceItem.Lrinvoicetaxitems.length == 0) {
                    $scope.InvoiceItem.taxamount = 0;
                    $scope.InvoiceItem.baseamount = 0;
                    $scope.InvoiceItem.discount = 0;
                }
                else {
                    $scope.InvoiceItem.taxamount -= $scope.GetNumber(item.taxamount);
                    $scope.InvoiceItem.baseamount -= $scope.GetNumber(item.amounton);
                    $scope.InvoiceItem.discount -= $scope.GetNumber(item.discount);
                }
                $scope.CalculateTotal(false);
                $scope.ComputeLRExpense();

                $scope.InvoiceTax.type = item.type;
                $scope.InvoiceTax.amounton = item.amounton;
                $scope.InvoiceTax.discountpercentage = item.discountpercentage;
                $scope.InvoiceTax.discount = item.discount;
                $scope.InvoiceTax.taxid = item.taxid;
                $scope.InvoiceTax.taxamount = item.taxamount;
                $scope.InvoiceTax.netamount = item.netamount;
                $scope.typeChanged();
            };
        }

        $scope.ComputeTDS = function () {
            if ($scope.InvoiceItem == undefined || $scope.InvoiceItem == null) {
                return;
            }
            if ($scope.Invoice.tdsrule == undefined ||
                $scope.Invoice.tdsrule == null ||
                $scope.Invoice.tdsrule.tdsledgerid == undefined ||
                $scope.Invoice.tdsrule.tdsledgerid == null ||
                $scope.Invoice.tdsrule.tdsledgerid <= 0)
                return;
            $scope.InvoiceItem.hastds = false;
            var taxable = $scope.InvoiceItem.baseamount - $scope.InvoiceItem.discount;

            if ($scope.Invoice.tdsrule.singletransaction > 0 &&
                $scope.Invoice.tdsrule.singletransaction <= taxable) {
                $scope.InvoiceItem.hastds = true;
            }
            else if ($scope.Invoice.tdsrule.totaltransaction > 0 &&
                $scope.Invoice.tdsrule.totaltransaction <=
                $scope.Invoice.tdsrule.balanceamount + taxable) {
                $scope.InvoiceItem.hastds = true;
                if ($scope.Invoice.tdsrule.balanceamount < $scope.Invoice.tdsrule.totaltransaction)
                    taxable = ($scope.Invoice.tdsrule.balanceamount + taxable) - $scope.Invoice.tdsrule.totaltransaction;
            }

            if ($scope.InvoiceItem.hastds) {
                $scope.InvoiceItem.tdsledgerid = $scope.Invoice.tdsrule.tdsledgerid;
                $scope.InvoiceItem.tdspercentage = $scope.Invoice.tdsrule.taxpercentage;
                if ($scope.InvoiceItem.tdspercentage > 0) {
                    $scope.InvoiceItem.tdsamount = (taxable * $scope.InvoiceItem.tdspercentage / 100).round(2);
                }
            }
        }

        $scope.SearchPO = function () {
            var request = {
                "Item": {
                    'supplierid': $scope.Invoice.supplierid,
                    'statusid': 1
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PurchaseorderService/SearchMyPendingPO",
                data: srequest
            }).success(function (result) {
                var poitems = [];
                $.each(result, function (k, v) {
                    poitems.push({ 'id': v.id, 'name': v.pono });
                })
                $scope.POList = poitems;
            });
        };

        $scope.CreateNew = function () {
            $scope.InvoiceItem.InvoiceDate = new Date();
        }();
        $timeout(function () {
            $scope.LoadAppSetting();
            $scope.setFocusfirst();
            $scope.Navigation.SetMode('SEARCH');
        }, 10);
    }]);

