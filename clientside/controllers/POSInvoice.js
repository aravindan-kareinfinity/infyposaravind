var app = angular.module('InfinityLab');

app.controller('POSInvoiceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope',
    '$window', 'restService', '$filter', '$timeout', 'localStorageService', '$interval', 'hotkeys', '$stateParams', 'ngDialog', '$sce', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, $timeout, localStorageService,
        $interval, hotkeys, $stateParams, ngDialog, $sce, cntxService) {
        $scope.ShowSearch = false;
        $scope.ShowImage = false;
        $scope.SearchModeEnabled = false;
        $scope.BeginGridTransaction = false;
        $scope.currentimageid = '';
        $scope.SearchResult = [];
        $scope.lastBills = [];
        $scope.SearchCondition = {};
        $scope.igrid = null;
        $scope.IsDiscount = false;
        $scope.IsDaylight = false;
        $scope.dateStyle = '';
        $scope.isoffline = false;
        $scope.AllowSMChange = false;
        $scope.Billingmode = 0;
        $scope.SelectedCustomerid = 0;
        $scope.DefaultPrintingTemplateID = 0;
        $scope.Billingmodename = '';
        $scope.CuttingBarcodeParser = null;
        $scope.WeighingBarcodeParser = null;
        $scope.SupportMarkDOWN = false;
        $scope.system_b2bposfields = ''
        $scope.system_multiplesalemen = false;
        $scope.system_hidecost = true;
        $scope.system_sales_so_mandatory = false;
        $scope.system_systemrole = false;
        $scope.system_sales_so_partial = false;
        $scope.system_poscutstock = false;
        $scope.system_poscutstockauto = false;
        $scope.system_restrictuniquebarcode = false;
        $scope.system_disableuniquebarcode = false;
        $scope.system_supportcostedit = false;
        $scope.system_supportpriceedit = false;
        $scope.system_supportmanualprice = false;
        $scope.system_supportstarpricematch = false;
        $scope.system_showexclusivetaxonpos = false;
        $scope.system_maxbarcodelength = 0;
        $scope.system_posdisablerefund = false;
        $scope.system_salesrounding = 0;
        $scope.system_saleautomobileno = false;
        $scope.system_salesroundingOnBarcode = false;
        $scope.sale_requestpasscodetosms = false;
        $scope.system_salecustomermandatory = false;
        $scope.system_salesmanmandatory = false;
        $scope.system_crsettlementonbill = false;
        $scope.system_multiplesaleqtypopup = false;
        $scope.system_multipleinvoiceqty = false;
        $scope.system_supportpricelist = false;
        $scope.system_supportbrandprice = false;
        $scope.system_adonfinalprice = false;
        $scope.system_creditlimit = false;
        $scope.system_b2bexclusivetax = false;
        $scope.system_retailpriceasb2bexclusive = false;
        $scope.system_marginbasedb2b = false;
        $scope.system_salecollectrefemp = false;
        $scope.system_supportshipto = false;
        $scope.system_multiplesaleqty = 10;
        $scope.system_supportbilledit = false;
        $scope.system_ismfgapplication = false;
        $scope.system_possizelist = '';
        $scope.system_sizegrouplayout = false;
        $scope.system_supportshipto = false;

        $scope.intray_b2bcredit = false;
        $scope.rounding_rate = 2;
        $scope.rounding_tax = 4;
        $scope.rounding_discount = 2;
        $scope.rounding_amount = 2;

        $scope.LocalContext = cntxService.CreatePageContext('POSInvoice');
        $scope.CurrentBillConfig = {
            'type': 'list',
            'items': [],
            'internallist': [],
            'stockqty': function () {
                var list = [];
                $.each(this.internallist, function (k, v) {
                    list.push({ 'id': v.salablegoodsid, 'qty': v.qty });
                });
                return list;
            },
            'groupList': function () {
                if (this.type == 'list') {
                    this.items = this.internallist;
                }
                else {

                    var glist = [];
                    var dlist = [];
                    $.each(this.internallist, function (k, v) {

                        if (typeof (v.attributes.packing) == 'undefined') v.attributes.packing = 0;
                        if (v.qty != 0) {
                            var exist = glist.find(function (item) {
                                return item.key.salerate == v.rate &&
                                    item.key.designid == v.attributes.designid &&
                                    item.key.colourcode == v.attributes.colourcode &&
                                    item.key.packing == v.attributes.packing;
                            });

                            if (typeof (exist) == 'undefined' || exist == null) {
                                var item = { 'key': {}, 'list': [] };
                                item.key.groupkey = v.attributes.groupkey;
                                item.key.groupname = v.attributes.groupname;
                                item.key.designid = v.attributes.designid;
                                item.key.colourcode = v.attributes.colourcode;
                                if (v.attributes.designid && v.attributes.designid != null &&
                                    v.attributes.colourcode && v.attributes.colourcode != null) {
                                    item.key.groupname = v.attributes.designid + '/' + v.attributes.colourcode;
                                }
                                item.key.salerate = v.rate;
                                item.key.etprice = v.etprice;
                                item.key.qty = v.qty;
                                item.key.cost = v.attributes.cost;
                                item.key.packing = v.attributes.packing;
                                item.key.designid = v.attributes.designid;
                                item.key.colourcode = v.attributes.colourcode;
                                item.list.push(v);
                                glist.push(item);

                            }
                            else {
                                exist.qty += v.qty;
                                exist.list.push(v);
                            }
                        }
                        else {
                            dlist.push(v);
                        }
                    });

                    if (dlist.length > 0) {
                        var il = this.internallist;
                        $.each(dlist, function (k, v) {
                            il.splice(il.indexOf(v), 1);
                        })
                    }

                    var items = [];
                    this.items = items;
                    $.each(glist, function (k, v) {
                        var item = {
                            'scheme': '',
                            'barcode': v.list[0].printingname,
                            'displaycode': v.list[0].printingname,
                            'printingname': v.key.groupname,
                            'designitemcode': v.key.groupkey,
                            'attributes': {
                                'cost': v.list[0].attributes ? v.list[0].attributes.cost : 0,
                                'packing': v.list[0].attributes ? v.list[0].attributes.packing : 0,
                                'mrp': v.list[0].attributes ? v.list[0].attributes.mrp : 0,
                                'somissing': false,
                                'somismatch': false
                            },
                            'etdiscount': 0,
                            'taxid': v.list[0].taxid,
                            'taxInfo': v.list[0].taxInfo,
                            'cost': v.key.cost,
                            'packing': v.key.packing,
                            'salerate': v.list[0].rate,
                            'etprice': v.list[0].etprice,
                            'itemtax': v.list[0].itemtax,
                            'taxpercentage': v.list[0].taxpercentage,
                            'qty': 0,
                            'discount': 0,
                            'discountpercentage': 0,
                            'schemediscountpercentage': 0,
                            'additionaldiscountpercentage': 0,
                            'additionaldiscount': 0,
                            'schemediscount': 0,
                            'receivable': 0,
                            'soqty': 0,
                            'list': v.list
                        };

                        $.each(v.list, function (kk, vv) {
                            if (vv.attributes && vv.attributes.somissing) {
                                item.attributes.somissing = true;
                                item.attributes.somismatch = true;
                            }
                            else if (vv.qty != vv.attributes.soqty) {
                                if (!item.attributes.somissing)
                                    item.attributes.somissing = vv.qty > vv.attributes.soqty;
                                item.attributes.somismatch = true;
                            }
                            item.qty += vv.qty;
                            item.discount += vv.discount;
                            item.etdiscount += (vv.etdiscount * vv.qty);
                            item.additionaldiscount += vv.additionaldiscount;
                            item.schemediscount += vv.schemediscount;
                            item.receivable += vv.receivable;
                            item.discountpercentage += vv.discountpercentage;
                            item.schemediscountpercentage += vv.schemediscountpercentage;
                            item.additionaldiscountpercentage += vv.additionaldiscountpercentage;
                        });

                        if (item.discountpercentage > 0)
                            item.discountpercentage = item.discountpercentage / v.list.length;

                        if (item.schemediscountpercentage > 0)
                            item.schemediscountpercentage = item.schemediscountpercentage / v.list.length;

                        if (item.additionaldiscountpercentage > 0)
                            item.additionaldiscountpercentage = item.additionaldiscountpercentage / v.list.length;

                        items.push(item);
                    });

                    $.each(items, function (k, v) {
                        v.receivable = v.receivable.round($scope.rounding_amount);
                        v.style = v.list.find(function (item) {
                            return item.style && item.style != null;
                        });
                    });

                }
                return this.items;
            }
        };

        $scope.BillNoConfig = {
            'multiple': false, 'prefix': '', 'bulkbillprefix': '', 'bulkbillprefixlist': [], 'prefixlist': [],
            'isbillno': function (key) {
                if (key.startsWith('#'))
                    return true;

                if (!this.multiple)
                    return (this.prefix != null && this.prefix != '' && key.startsWith(this.offlineprefix + this.prefix));

                for (var i = 0; i < this.prefixlist.length; i++) {
                    if (this.prefixlist[i] != null && this.prefixlist[i] != '' && key.startsWith(this.offlineprefix + this.prefixlist[i]))
                        return true;
                }

                if (!this.multiple)
                    return (this.returnprefix != null && this.returnprefix != '' && key.startsWith(this.offlineprefix + this.returnprefix));

                for (var i = 0; i < this.returnprefixlist.length; i++) {
                    if (this.returnprefixlist[i] != null && this.returnprefixlist[i] != '' && key.startsWith(this.offlineprefix + this.returnprefixlist[i]))
                        return true;
                }

                return false;
            },
            'isbillmaster': function (key) {
                return (this.masterprefix != null && this.masterprefix != '' && key.startsWith(this.masterprefix));
            },
            'isbulkbillno': function (key) {
                if (key.startsWith('#'))
                    return true;

                if (!this.multiple)
                    return (this.bulkbillprefix != null && this.bulkbillprefix != '' && key.startsWith(this.offlineprefix + this.bulkbillprefix));

                if (this.bulkbillprefixlist && this.bulkbillprefixlist != null) {
                    for (var i = 0; i < this.bulkbillprefixlist.length; i++) {
                        if (this.bulkbillprefixlist[i] != null && this.bulkbillprefixlist[i] != '' && key.startsWith(this.offlineprefix + this.bulkbillprefixlist[i]))
                            return true;
                    }
                }
                return false;
            },
            'init': function (ismultiple) {

                this.multiple = typeof (ismultiple) != 'undefined' || ismultiple;
                this.offlineprefix = typeof ($scope.ApplicationSettings.appsettings.system_offlineprefix) == 'undefined' || $scope.ApplicationSettings.appsettings.system_offlineprefix == null ? '' :
                    $scope.ApplicationSettings.appsettings.system_offlineprefix;

                if (this.offlineprefix == '')
                    this.offlineprefix = typeof ($scope.ApplicationSettings.appsettings.system_onlineprefix) == 'undefined' || $scope.ApplicationSettings.appsettings.system_onlineprefix == null ? '' :
                        $scope.ApplicationSettings.appsettings.system_onlineprefix;

                $scope.SearchModeEnabled = false || $scope.ApplicationSettings.appsettings.system_ismfgapplication;
                if ($scope.SearchModeEnabled && $scope.ApplicationSettings.appsettings.mfg_mode >= 2)
                    $scope.CurrentBillConfig.type = 'group';

                if ($scope.ApplicationSettings.appsettings.system_possizelist == undefined ||
                    $scope.ApplicationSettings.appsettings.system_possizelist == null ||
                    $scope.ApplicationSettings.appsettings.system_possizelist == '') {
                    if ($scope.ApplicationSettings.appsettings.system_posscrtemplate == 3 ||
                        $scope.ApplicationSettings.appsettings.system_posscrtemplate == 4) {
                        $scope.CurrentBillConfig.type = 'list';
                    }
                }
                $scope.system_b2brounding = false || $scope.ApplicationSettings.appsettings.system_b2brounding;
                if ($scope.system_b2brounding) {
                    $scope.rounding_rate = 8;
                    $scope.rounding_tax = 8;
                    $scope.rounding_discount = 8;
                    $scope.rounding_amount = 2;
                }

                $scope.IsUANBased = $scope.ApplicationSettings.appsettings.mfg_mode == 5;
                $scope.system_saleorder = $scope.ApplicationSettings.appsettings.saleorder;
                $scope.system_systemrole = $scope.ApplicationSettings.appsettings.system_systemrole;
                $scope.system_sales_so_mandatory = false || $scope.ApplicationSettings.appsettings.sales_so_mandatory;
                $scope.system_multiplesalemen = false || $scope.ApplicationSettings.appsettings.system_multiplesalemen;
                $scope.system_sales_so_partial = false || $scope.ApplicationSettings.appsettings.sales_so_partial;
                $scope.system_posscrtemplate = $scope.ApplicationSettings.appsettings.system_posscrtemplate;
                $scope.system_supportshipto = false || $scope.ApplicationSettings.appsettings.system_salecustomermandatory;
                $scope.system_salecustomermandatory = false || $scope.ApplicationSettings.appsettings.system_salecustomermandatory;
                $scope.system_salesmanmandatory = false || $scope.ApplicationSettings.appsettings.system_salesmanmandatory;
                $scope.system_crsettlementonbill = false || $scope.ApplicationSettings.appsettings.system_crsettlementonbill;
                $scope.system_multiplesaleqtypopup = false || $scope.ApplicationSettings.appsettings.system_multiplesaleqtypopup;
                $scope.system_supportcostedit = false || $scope.ApplicationSettings.appsettings.system_supportcostedit;
                $scope.system_poscutstock = false || $scope.ApplicationSettings.appsettings.system_poscutstock;
                $scope.system_poscutstockauto = false || $scope.ApplicationSettings.appsettings.system_poscutstockauto;
                $scope.system_restrictuniquebarcode = false || $scope.ApplicationSettings.appsettings.system_restrictuniquebarcode;
                $scope.system_disableuniquebarcode = false || $scope.ApplicationSettings.appsettings.system_disableuniquebarcode;
                $scope.system_supportpriceedit = false || $scope.ApplicationSettings.appsettings.system_supportpriceedit;
                $scope.system_multipleinvoiceqty = false || $scope.ApplicationSettings.appsettings.system_multipleinvoiceqty;
                $scope.system_b2bexclusivetax = false || $scope.ApplicationSettings.appsettings.system_b2bexclusivetax;
                $scope.system_adonfinalprice = false || $scope.ApplicationSettings.appsettings.system_adonfinalprice;
                $scope.system_supportpricelist = false || $scope.ApplicationSettings.appsettings.system_supportpricelist;
                $scope.system_supportbrandprice = false || $scope.ApplicationSettings.appsettings.system_supportbrandprice;
                $scope.system_creditlimit = false || $scope.ApplicationSettings.appsettings.system_creditlimit;
                $scope.system_supportmanualprice = false || $scope.ApplicationSettings.appsettings.system_supportmanualprice;
                $scope.system_supportstarpricematch = false || $scope.ApplicationSettings.appsettings.system_supportstarpricematch;
                $scope.system_showexclusivetaxonpos = false || $scope.ApplicationSettings.appsettings.system_showexclusivetaxonpos;
                $scope.system_salecollectrefemp = false || $scope.ApplicationSettings.appsettings.system_salecollectrefemp;
                $scope.system_supportshipto = false || $scope.ApplicationSettings.appsettings.system_supportshipto;
                $scope.system_supportbilledit = false || $scope.ApplicationSettings.appsettings.system_supportbilledit;
                $scope.intray_b2bcredit = false || $scope.ApplicationSettings.appsettings.intray_b2bcredit;
                $scope.system_saleautomobileno = false || $scope.ApplicationSettings.appsettings.system_saleautomobileno;
                $scope.system_b2bposfields = $scope.ApplicationSettings.appsettings.system_b2bposfields;
                $scope.system_salepricemrpdifference = $scope.ApplicationSettings.appsettings.system_salepricemrpdifference || false;
                $scope.system_multiplesaleqty = $scope.ApplicationSettings.appsettings.system_multiplesaleqty &&
                    $scope.ApplicationSettings.appsettings.system_multiplesaleqty > 0 ? $scope.ApplicationSettings.appsettings.system_multiplesaleqty : 10;
                $scope.system_hidecost = UserDetails.Info.p_SHOWVALUE && UserDetails.Info.p_SHOWVALUE.view;
                $scope.system_possizelist = $scope.ApplicationSettings.appsettings.system_possizelist;
                $scope.system_sizegrouplayout = $scope.ApplicationSettings.appsettings.system_sizegrouplayout;
                $scope.system_b2bprintdisable = false || $scope.ApplicationSettings.appsettings.system_b2bprintdisable;
                
                if ($scope.system_sizegrouplayout) {
                    restService.Get('ReferencelistService/SIZE', true).then(function (sizeresults) {
                        $scope.SizeList = sizeresults;
                        restService.Get('ReferencelistService/FULL-SIZEGROUP', true).then(function (results) {
                            $scope.SizeGroupList = [];
                            $.each(results, function (k, v) {
                                if (v.additionalinfo &&
                                    v.additionalinfo.sizegroups &&
                                    v.additionalinfo.sizegroups.length > 0) {
                                    var newsizegroup = {
                                        'id': v.id, 'name': v.name, 'possizelist': [],
                                        'list': []
                                    };

                                    $.each(v.additionalinfo.sizegroups, function (kk, vv) {
                                        var size = $scope.SizeList.find(function (item) {
                                            return item.id == vv
                                        });
                                        if (size && size != null) {
                                            newsizegroup.list.push(size);
                                            newsizegroup.possizelist.push(size.name);
                                        }
                                    });

                                    newsizegroup.possizelist = newsizegroup.possizelist.join(',');
                                    $scope.SizeGroupList.push(newsizegroup);
                                }
                            })
                        });
                    });
                }
                $scope.system_ismfgapplication = $scope.ApplicationSettings.appsettings.system_ismfgapplication;
                if (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.billconfig) != 'undefined' &&
                    $rootScope.LocationInfo.billconfig != null) {

                    if ($rootScope.LocationInfo.location.additionalinfo &&
                        $rootScope.LocationInfo.location.additionalinfo != null &&
                        $rootScope.LocationInfo.location.additionalinfo.parentid &&
                        $rootScope.LocationInfo.location.additionalinfo.parentid > 0) {

                        if ($scope.BuyingCompany &&
                            $scope.BuyingCompany != null) {
                            var company = $scope.BuyingCompany.find(function (item) {
                                return item.id == $rootScope.LocationInfo.location.additionalinfo.parentid
                            });

                            if (company && company != null && company.gstno &&
                                company.gstno != null) {
                                $scope.CompanyGSTNO = company.gstno;
                            }
                        }
                    }

                    this.prefix = $rootScope.LocationInfo.billconfig.prefix;
                    this.prefixlist = $rootScope.LocationInfo.billconfig.prefixlist;

                    this.returnprefix = $rootScope.LocationInfo.billreturnconfig.prefix;
                    this.returnprefixlist = $rootScope.LocationInfo.billreturnconfig.prefixlist;

                    if (typeof ($rootScope.LocationInfo.bulkbillconfig) != 'undefined' && $rootScope.LocationInfo.bulkbillconfig != null) {
                        this.bulkbillprefix = $rootScope.LocationInfo.bulkbillconfig.prefix;
                        this.bulkbillprefixlist = $rootScope.LocationInfo.bulkbillconfig.prefixlist;
                    }

                    this.masterprefix = $rootScope.LocationInfo.billmasterconfig.prefix;

                    if (typeof (this.prefix) == 'undefined' || this.prefix == '')
                        this.prefix = null;

                    if (typeof (this.bulkbillprefix) == 'undefined' || this.bulkbillprefix == '')
                        this.bulkbillprefix = null;

                    if (typeof (this.bulkbillprefixlist) == 'undefined' || this.bulkbillprefixlist == '')
                        this.bulkbillprefixlist = null;
                    if (typeof (this.masterprefix) == 'undefined' || this.masterprefix == '')
                        this.masterprefix = null;
                }

                if ($scope.system_systemrole) {
                    var authData = localStorageService.get('authorizationData');
                    if (authData.systemrole != 'ALL' &&
                        authData.systemrole != 'BILLING' &&
                        authData.systemrole != 'BILLSETTLE') {
                        $rootScope.ShowAlert('System Violation', 'This system not alloted for billing');
                        $timeout(function () {
                            $(location).attr('href', 'index.html');
                        }, 1000);
                        return;
                    }
                }

                $scope.system_supportpostaxtype = false || $scope.ApplicationSettings.appsettings.system_supportpostaxtype;
                if ($scope.system_supportpricelist) {
                    restService.Get('CustomerpriceService/SearchAll', true).then(function (results) {
                        $scope.priceList = new PriceListManager(results);
                    });
                }

                if ($scope.system_supportpostaxtype) {
                    $scope.ShowTaxTypeBox();
                }
                else {
                    $scope.GridConfig();
                }
            }
        };
        $scope.PromotionEnabled = false;
        $scope.AdditionalDiscountEnabled = false;
        $scope.EMIEnabled = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.DiscountQueue = false;
        $scope.IsSaleReturn = $stateParams.type == 'SALERETURN';
        $scope.IsBulkSale = $stateParams.type == 'BULKSALE';
        $scope.Title = $scope.IsSaleReturn ? 'Dealer Sales Return' : ($scope.IsBulkSale ? 'Dealer Sales On Approval' : 'Dealer Sales');
        $scope.leftPanStyle = $scope.IsSaleReturn ? '#f7dcdc' : ($scope.IsBulkSale ? '#cff7db' : 'white');
        $scope.IsWarehouse = false;
        $scope.CANAPPROVEDISCOUNT = false;
        $scope.SearchAPI = $scope.IsSaleReturn ? 'SalablegoodsService/ReturnBarcode/' : 'SalablegoodsService/Barcode/';

        $scope.HasLocationSelected = false;
        $scope.CompanyGSTNO = null;
        $scope.CompanyStateID = 0;
        $rootScope.OnLocationInfoReady(function () {
            $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                typeof ($rootScope.LocationInfo.location) != 'undefined');
            if ($scope.HasLocationSelected) {
                $scope.CompanyGSTNO = $rootScope.LocationInfo.company.gstno;
                $scope.CompanyStateID = $rootScope.LocationInfo.company.stateid;
                $scope.CompanyCountryID = $rootScope.LocationInfo.company.countryid;
            }
        });

        $scope.StateList = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.StateList = results;
        });

        $scope.BillKeys = restService.Get('ReferencelistService/BILLKEY', true).then(function (results) {
            $scope.BillKeys = results;
        });

        $scope.Promotions = restService.Get('ReferencelistService/PROMOTIONS', true).then(function (results) {
            $scope.Promotions = results;
        });

        $scope.ChangeTitle = function () {
            $scope.Title = $scope.IsSaleReturn ? 'Dealer Sales Return' : 'Dealer Sales';
            if ($scope.IsDiscount)
                $scope.Title += ' (Discount)';
        }
        $scope.ChangeTitle();


        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
            $scope.CounterName = ($scope.HasLocationSelected) ? $scope.SearchRef($rootScope.LocationInfo.counter.id, results) : 'Select Counter';
            if ($scope.HasLocationSelected && !$scope.AutoSettlement) {
                var counter = results.find(function (item) { return item.id == $rootScope.LocationInfo.counter.id; });
                $scope.AutoSettlement = typeof (counter) != 'undefined' && counter.autosettlement;
                $scope.OnlineCounter = typeof (counter) != 'undefined' && counter.online;

                if ($scope.AutoSettlement) {
                    $rootScope.Toaster.Notice("Single window settlement enabled..");
                    $scope.ValidateCounter();
                }
                if ($scope.OnlineCounter) {
                    if ($scope.CurrentBill && $scope.CurrentBill.attributes)
                        $scope.CurrentBill.billattributes.online = $scope.OnlineCounter;
                    $rootScope.Toaster.Notice("Online Counter..");
                }
            }
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
            $scope.floorname = ($scope.HasLocationSelected) ? $scope.SearchRef($rootScope.LocationInfo.floorid, results) : 'Select Floor';
        });

        $scope.Locations = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Locations = results;
            if (!$scope.HasLocationSelected) {
                $scope.LocationName = 'Select Location';
                return;
            }
            $scope.ShowLocationInfo();
        });

        $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.Customers = results;
        });
        $scope.EmployeeList = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-', 'code': '-' });
            }
            $scope.EmployeeList = results;
        });

        $scope.ShowLocationInfo = function () {
            $scope.CounterName = ($scope.HasLocationSelected) ? $scope.SearchRef($rootScope.LocationInfo.counter.id, $scope.Counters) : 'Select Counter';
            $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, $scope.Locations);
            $.each($scope.Locations, function (k, v) {
                if (v.id == $rootScope.LocationInfo.location.id) {
                    var additionalinfo = v.additionalinfo;
                    if (additionalinfo != null)
                        $scope.IsWarehouse = additionalinfo.iswarehouse;
                    if ($scope.IsWarehouse)
                        $rootScope.Toaster.Error("Sales will not be allowed in Warehouse, Please check the sales location.");
                }
            });
        }

        //$scope.SupplierProducts = null;
        //$scope.SupplierProducts = restService.Get('ReferencelistService/SUPPLIERPRODUCT', true).then(function (results) {
        //    $scope.SupplierProducts = results;
        //});
        //$scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
        //    $scope.Supplier = results;
        //});

        $scope.Brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brands = results;
        });
        $scope.SalesTax = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.SalesTax = results;
        });
        $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
            $scope.AdditionalChargeList = results;
        });
        restService.Get('ReferencelistService/FULLCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.FilteredProducts = [];
        restService.Get('ReferencelistService/ALLPRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.SupplierChanged = function () {
            if ($scope.SupplierProducts == null || typeof ($scope.SearchCondition.SupplierID) == 'undefined' ||
                $scope.SearchCondition.SupplierID == null || $scope.SearchCondition.SupplierID == 0) {
                $scope.FilteredProducts.length = 0;
                $.each($scope.Products, function (k, v) {
                    $scope.FilteredProducts.push(v);
                });
            }
            else {
                var sp = $scope.SupplierProducts.find(function (item) {
                    return item.id == $scope.SearchCondition.SupplierID;
                });

                var productlist = typeof (sp) == 'undefined' ? [] : sp.child;
                $scope.FilteredProducts.length = 0;
                if (productlist == null || productlist.length == 0) {
                    $.each($scope.Products, function (k, v) {
                        $scope.FilteredProducts.push(v);
                    });
                }
                else {
                    $.each(productlist, function (k, v) {
                        var prod = $scope.Products.find(function (item) {
                            return item.id == v;
                        });
                        $scope.FilteredProducts.push(prod);
                    });
                }
            }
        }
        $scope.GetRoundingValue = function () {
            var receivable = $scope.CurrentBill.receivable;
            if ($scope.CurrentBill.additionalcharges &&
                $scope.CurrentBill.additionalcharges != null &&
                $scope.CurrentBill.additionalcharges > 0) {
                receivable += $scope.CurrentBill.additionalcharges;
            }
            if ($scope.system_salesrounding > 0 && $scope.system_salesroundingOnBarcode) {
                return $rootScope.RoundRupee(receivable.round(0), $scope.system_salesrounding) - receivable.round(2);
            }
            else {
                return (receivable.round(0) - receivable.round(2)).round(2);
            }
        }
        $scope.ReturnReason = restService.Get('ReferencelistService/RETURNREASON', true).then(function (results) {
            $scope.ReturnReason = results;
        });
        $scope.BulkSaleReason = restService.Get('ReferencelistService/BULKSALEREASON', true).then(function (results) {
            $scope.BulkSaleReason = results;
        });
        $scope.lastBills = restService.Get('BillmasterService/LastBills', true).then(function (results) {
            $scope.lastBills = results;
        });

        $scope.BillModes = [{ id: 1, name: 'Inclusive Tax (STICKER)' }, { id: 2, name: 'Exclusive Tax (RETAIL)' },
        { id: 3, name: 'Exclusive Tax (B2B)' }, { id: 4, name: 'Inclusive Tax (MARGIN)' },
        { id: 5, name: 'Exclusive Tax (MARGIN)' }];

        $scope.BillType = [{ id: 1, name: 'Cash' }, { id: 2, name: 'Card' }, { id: 3, name: 'Credit' }, { id: 4, name: 'Redemption' }];

        $scope.TemplateSalesDetail = {
            barcode: '',
            printingname: '',
            productid: 0,
            sellingmode: 0,
            price: '',
            invoiceitemid: '',
            id: '',
            qty: '',
            isactive: true,
            qtyeditable: false,
            priceeditable: false,
            salesmanid: '',
            salesmanname: '',
            salesmancode: '',
            version: 1
        };

        $scope.TemplateCurrentBill = {
            Billitems: [],
            additionalchargeslist: [],
            additionalcharges: 0,
            billattributes: {},
            billTax: [],
            billdate: "",
            billtime: "",
            tax: 0.0,
            id: 0,
            isactive: true,
            iscreditbill: true,
            isbulkreturn: false,
            discount: 0.0,
            receivable: 0.0,
            additionaldiscount: 0.0,
            customerid: 0,
            phoneno: '',
            customername: 'Cash',
            billtype: 0,
            rounding: 0,
            totalqty: 0,
            totalpiece: 0,
            additionaldiscount: 0,
            total: 0,
            totaldiscount: 0,
            rounding: 0,
            schemediscount: 0,
            schemereward: 0,
            schemediscountcoupen: 0,
            HasPendingPromotion: false,
            HasPromotionApplied: false,
            HasCouponApplied: false,
            areaid: 0,
            salereasonid: 0,
            seasonid: 0,
            returnbillid: 0
        };

        $scope.UploadedMissingBarcodeMissing = null;
        $scope.UploadedMissingBarcodeMissing_Shortage = null;
        $scope.UploadedMissingBarcodeMissing_Missing = null;
        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid <= 0) {
                $rootScope.Toaster.Error("Please select the customer before upload the barcodes");
                return;
            }

            var barcodelist = [];
            var barcodelistgroup = [];
            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                var price = 0;
                var qty = 1;
                if (barcode.toLowerCase().indexOf('barcode') >= 0) return;

                if (v.values.length > 1 && v.values[1] != null && v.values[1] != '')
                    qty = $scope.GetNumber(v.values[1]);

                if (v.values.length > 2 && v.values[2] != null && v.values[2] != '')
                    price = $scope.GetNumber(v.values[2]);

                if (barcode != null && barcode != '' && barcode.length > 0) {

                    for (var i = 0; i < qty; i++)
                        barcodelist.push(barcode);

                    var grpexist = barcodelistgroup.find(function (item) {
                        return item.barcode == barcode;
                    });
                    if (typeof (grpexist) == 'undefined' || grpexist == null)
                        barcodelistgroup.push({ 'barcode': barcode, 'qty': qty, 'price': price });
                    else
                        grpexist.qty = grpexist.qty + qty;
                }
            });

            $scope.UploadedMissingBarcodeMissing = null;
            $scope.UploadedMissingBarcodeMissing_Shortage = null;
            $scope.UploadedMissingBarcodeMissing_Missing = null;

            if (barcodelist.length > 0) {
                var request =
                {
                    Item:
                    {
                        'customerid': $scope.CurrentBill.customerid,
                        'locationid': $rootScope.LocationInfo.location.id,
                        'barcodes': barcodelist,
                        'returngoods': $scope.IsSaleReturn,
                        'includesold': $scope.IsSaleReturn,
                        'ccmarginmrp': $scope.CurrentBill.ccmarginmrp,
                        'ccmarginprice': $scope.CurrentBill.ccmarginprice,
                        'cccomputemode': $scope.CurrentBill.cccomputemode,
                        'usedstock': $scope.CurrentBillConfig.stockqty()
                    }
                };
                if ($scope.SearchModeEnabled) {
                    request =
                    {
                        Item:
                        {
                            'customerid': $scope.CurrentBill.customerid,
                            'locationid': $rootScope.LocationInfo.location.id,
                            'shipcustomerid': ($scope.CurrentBill.billattributes && $scope.CurrentBill.billattributes.hasshiptto) ? $scope.CurrentBill.billattributes.shiptoid : 0,
                            'code': $scope.autoCoding.code,
                            'qty': $scope.autoCoding.qty,
                            'barcodes': barcodelist,
                            'sono': $scope.CurrentBill.billattributes.orderrefid,
                            'usedstock': $scope.CurrentBillConfig.stockqty(),
                            'includesold': $scope.IsSaleReturn,
                            'returngoods': $scope.IsSaleReturn,
                            'ccmarginmrp': $scope.CurrentBill.ccmarginmrp,
                            'ccmarginprice': $scope.CurrentBill.ccmarginprice,
                            'cccomputemode': $scope.CurrentBill.cccomputemode,
                            'source': typeof (source) != 'undefined' ? source : 'UAN'
                        }
                    };
                    if ($scope.IsUANBased)
                        request.Item.source = '';
                }

                $http({
                    method: 'POST',
                    url: "SalablegoodsService/SearchBarcode",
                    data: JSON.stringify(request)
                }).success(function (availableitems) {
                    var missinguanbarcodes = [];
                    if (availableitems.length > 0) {
                        var baseSalesDetail = (JSON.parse(JSON.stringify($scope.SalesDetail)));
                        baseSalesDetail.isuploadqty = true;
                        $scope.BeginGridTransaction = true;
                        for (var i = 0; i < availableitems.length; i++) {
                            var results = availableitems[i];
                            if (results.rqty > 0) {
                                missinguanbarcodes.push({ 'barcode': results.serialno, 'qty': results.rqty, 'rqty': results.qty });
                                continue;
                            }
                            //if (!$scope.IsSaleReturn)
                            {
                                var grpexist = barcodelistgroup.find(function (item) {
                                    return (item.barcode == results.barcode || item.barcode == results.serialno)
                                        && item.qty > 0;
                                });

                                if (typeof (grpexist) != 'undefined' && grpexist != null) {

                                    if (!$scope.IsSaleReturn) {
                                        if (grpexist.barcode != results.serialno &&
                                            grpexist.qty >= results.iqty - (results.oqty + results.dqty + results.jqty)) {
                                            results.qty = results.iqty - (results.oqty + results.dqty + results.jqty);
                                        } else {
                                            results.qty = grpexist.qty;
                                        }
                                        grpexist.qty = grpexist.qty - results.qty;
                                        if (grpexist.qty <= 0)
                                            barcodelistgroup.splice(barcodelistgroup.indexOf(grpexist), 1);
                                    }
                                    else {
                                        if (grpexist.barcode == results.serialno &&
                                            grpexist.qty >= results.oqty) {
                                            results.qty = results.oqty;
                                        } else {
                                            results.qty = grpexist.qty;
                                        }
                                        grpexist.qty = grpexist.qty - results.qty;
                                        if (grpexist.qty <= 0)
                                            barcodelistgroup.splice(barcodelistgroup.indexOf(grpexist), 1);
                                        results.qty *= -1;
                                    }
                                }
                                else {
                                    continue;
                                }

                                if (!$scope.IsSaleReturn && results.qty <= 0) continue;
                            }

                            results.buyingprice = results.buyingprice.round(2);
                            $scope.SalesDetail = (JSON.parse(JSON.stringify(baseSalesDetail)));
                            $scope.SalesDetail.barcode = results.barcode;
                            $scope.SalesDetail.qty = results.qty;
                            $scope.SalesDetail.stock = results.retailqty;
                            $scope.SalesDetail.hsncode = results.hsncode;
                            $scope.SalesDetail.attributes = {
                                'serialno': results.serialno,
                                'groupkey': results.groupkey,
                                'groupname': results.groupname,
                                'designid': results.designid,
                                'sizegroup': results.sizegroup,
                                'colourname': results.colourname,
                                'colourcode': results.colourcode,
                                'size': results.sizename,
                                'mrp': results.mrp,
                                'so': results.saleorderitems,
                                'brand': results.brandname,
                                'cost': results.buyingprice,
                                'typename': results.typename,
                                'materialname': results.materialname,
                                'stylename': results.stylename,
                                'patternname': results.patternname
                            };

                            results.priceextax = results.priceextax;
                            if ($scope.CurrentBill.pricesource == 1) {
                                results.price = results.wsrate;
                                results.priceextax = results.wsrateextax;

                                results.discount = 0;
                                results.discountpercentage = 0;
                                results.additionaldiscount = 0;
                                results.additionaldiscountpercentage = 0;
                            }
                            if ($scope.CurrentBill.pricesource == 2) {
                                results.price = results.dealerprice;
                                results.priceextax = results.dealerpriceextax;

                                results.discount = 0;
                                results.discountpercentage = 0;
                                results.additionaldiscount = 0;
                                results.additionaldiscountpercentage = 0;
                            }
                            if (typeof (grpexist) != 'undefined' && grpexist != null && grpexist.price > 0) {
                                results.price = grpexist.price;
                                results.priceextax = grpexist.price;
                                results.discount = 0;
                                results.discountpercentage = 0;
                                results.additionaldiscount = 0;
                                results.additionaldiscountpercentage = 0;
                                $scope.SalesDetail.qty = grpexist.qty;
                            }
                            $scope.SalesDetail.pricewithouttax = results.priceextax;
                            $scope.SalesDetail.pricewithtax = results.price;

                            if (!$scope.IsUANBased && $scope.CurrentBill.ccmarginmode) {
                                if ($scope.CurrentBill.ccmarginmrp > 0) {
                                    var mrpmargin = (results.mrp * $scope.CurrentBill.ccmarginmrp / 100).round($scope.rounding_rate);
                                    var mrpcost = results.mrp - mrpmargin;
                                    if (results.taxInfo != null && results.taxInfo.taxsplit != null) {

                                        if (results.taxInfo.taxsplit.findTax == undefined || results.taxInfo.taxsplit.findTax == null)
                                            results.taxInfo.taxsplit.findTax = $scope.findTax;

                                        var taxitem = results.taxInfo.taxsplit.findTax(results.mrp, false);
                                        var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                                        var beforetax = (results.mrp / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                                        mrpcost -= beforetax;
                                    }
                                    results.price = mrpcost;
                                    results.baseprice = results.price;
                                }
                                else if ($scope.CurrentBill.ccmarginprice > 0) {
                                    var mrpmargin = (results.price * $scope.CurrentBill.ccmarginprice / 100).round($scope.rounding_rate);
                                    var mrpcost = results.price - mrpmargin;
                                    if (results.taxInfo != null && results.taxInfo.taxsplit != null) {

                                        if (results.taxInfo.taxsplit.findTax == undefined || results.taxInfo.taxsplit.findTax == null)
                                            results.taxInfo.taxsplit.findTax = $scope.findTax;

                                        var taxitem = results.taxInfo.taxsplit.findTax(results.price, false);
                                        var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                                        var beforetax = (results.price / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                                        mrpcost -= beforetax;
                                    }
                                    results.price = mrpcost;
                                    results.baseprice = results.price;
                                }
                                else {
                                    results.price = (results.buyingprice + (results.buyingprice * $scope.CurrentBill.ccmargin / 100)).round($scope.rounding_rate);
                                    results.baseprice = results.price;
                                }
                                $scope.SalesDetail.pricewithouttax = results.priceextax;
                                $scope.SalesDetail.pricewithtax = results.price;
                            }

                            $scope.AddBarcode(results, false);

                            var product = $scope.Products.find(function (item) {
                                return item.id == results.productid;
                            });
                            /*var brand = $scope.Brands.find(function (item) {
                                return item.id == results.brandid;
                            });*/
                            $scope.SalesDetail.sellingmode = product.sellingmode;
                            $scope.AddBillItem(false);
                        }
                        $scope.BeginGridTransaction = false;
                        $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                        $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                        $scope.setFocusautocoding();
                    }

                    if ($scope.IsSaleReturn) {
                        $scope.UploadedMissingBarcodeMissing = null;
                        $scope.UploadedMissingBarcodeMissing_Shortage = null;
                        $scope.UploadedMissingBarcodeMissing_Missing = null;
                    }
                    else {
                        if (barcodelistgroup.length > 0) {
                            if ($scope.UploadedMissingBarcodeMissing == null)
                                $scope.UploadedMissingBarcodeMissing = [];
                            $.each(barcodelistgroup, function (k, v) {
                                var exist = missinguanbarcodes.find(function (item) {
                                    return item.barcode == v.barcode;
                                });
                                if (typeof (exist) == 'undefined' || exist == null)
                                    $scope.UploadedMissingBarcodeMissing.push({ 'barcode': v.barcode, 'qty': 0, 'rqty': v.qty });
                            })
                        }

                        if (missinguanbarcodes.length > 0) {
                            if ($scope.UploadedMissingBarcodeMissing == null)
                                $scope.UploadedMissingBarcodeMissing = [];
                            $.each(missinguanbarcodes, function (k, v) {
                                $scope.UploadedMissingBarcodeMissing.push({ 'barcode': v.barcode, 'qty': v.rqty - v.qty, 'rqty': v.rqty });
                            });
                        }

                        $scope.UploadedMissingBarcodeMissing_Shortage = 0;
                        $scope.UploadedMissingBarcodeMissing_Missing = 0;
                        if ($scope.UploadedMissingBarcodeMissing && $scope.UploadedMissingBarcodeMissing != null) {
                            $.each($scope.UploadedMissingBarcodeMissing, function (k, v) {
                                if (v.qty > 0)
                                    $scope.UploadedMissingBarcodeMissing_Shortage += v.rqty - v.qty;
                                else
                                    $scope.UploadedMissingBarcodeMissing_Missing += v.rqty;
                            });
                        }
                    }

                }).error(function (result) {
                    $scope.BeginGridTransaction = false;
                    $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                    $scope.setFocusautocoding();
                });
            }
        }



        $scope.ApplicationSettings = function () {

            if (!$scope.HasLocationSelected) {
                $rootScope.Triggers.SelectLocation('', function () {
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                        typeof ($rootScope.LocationInfo.location) != 'undefined')
                    $scope.ShowLocationInfo();
                    $scope.ApplicationSettings();
                });
                return;
            }

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

                    $scope.HSNLimit.init($scope.ApplicationSettings.appsettings.sale_b2bhsnlimit);
                    $scope.system_posdisablerefund = $scope.ApplicationSettings.appsettings.system_posdisablerefund;
                    $scope.system_maxbarcodelength = $scope.ApplicationSettings.appsettings.system_maxbarcodelength;

                    if ($scope.ApplicationSettings.appsettings.system_barcodeparser &&
                        $scope.ApplicationSettings.appsettings.system_barcodeparser != null &&
                        $scope.ApplicationSettings.appsettings.system_barcodeparser != '') {
                        try {
                            var barcodeparser = (JSON.parse($scope.ApplicationSettings.appsettings.system_barcodeparser));
                            if (barcodeparser.cutting && barcodeparser.cutting != null) {
                                function CuttingBarcodeParser(config) {
                                    this.config = config;
                                    this.valid = function (barcode) {
                                        if (barcode == undefined || barcode == undefined || barcode == '')
                                            return false;
                                        if (this.config.QApp && barcode.startsWith('~'))
                                            return true;

                                        if ((this.config.barcodelength + this.config.meterlength) == barcode.length) {
                                            return true;
                                        }
                                        if (this.config.prefix && this.config.prefix != null &&
                                            this.config.prefix.length > 0 && barcode.startsWith(this.config.prefix)) {
                                            return true;
                                        }
                                        return false;
                                    }
                                    this.parse = function (barcode) {
                                        if (this.config.QApp) {
                                            var barcodeparts = barcode.split('~');
                                            return {
                                                'valid': barcodeparts.length == 3 || barcodeparts.length == 4,
                                                'barcode': barcodeparts[1],
                                                'qty': barcodeparts[2],
                                                'ecode': barcodeparts[3]
                                            };
                                        }
                                        else {
                                            if ((this.config.barcodelength + this.config.meterlength) == barcode.length) {
                                                var qty = barcode.substring(this.config.barcodelength,
                                                    this.config.barcodelength + this.config.meterlength);
                                                var intpart = qty.substring(0, qty.length - this.config.meterdecimal);
                                                var decpart = qty.substring(intpart.length);
                                                return {
                                                    'barcode': barcode.substring(0, this.config.barcodelength),
                                                    'valid':true,
                                                    'qty': parseFloat(intpart + '.' + decpart)
                                                };
                                            }
                                            if (this.config.prefix && this.config.prefix != null &&
                                                this.config.prefix.length > 0 && barcode.startsWith(this.config.prefix)) {
                                                var parts = barcode.substring(this.config.prefix.length).split(this.config.separator);
                                                return { 'barcode': parts[0], 'qty': parseFloat(parts[1]) };
                                            }
                                            return { 'barcode': barcode, 'valid': true, 'qty': 1 };
                                        }
                                    }
                                }
                                $scope.CuttingBarcodeParser = new CuttingBarcodeParser(barcodeparser.cutting);
                            }


                            if (barcodeparser.weighing && barcodeparser.weighing != null) {
                                function WeighingBarcodeParser(config) {
                                    this.config = config;
                                    this.valid = function (barcode) {
                                        if (barcode == undefined || barcode == undefined || barcode == '')
                                            return false;
                                        if (barcode.startsWith(this.config.prefix) && barcode.length == this.config.length)
                                            return true;
                                        return false;
                                    }
                                    this.parse = function (barcode) {
                                        var wbarcode = barcode.substring(this.config.barcodestart, this.config.barcodelength);
                                        var wqty = 0;
                                        var kgpart = parseInt(barcode.substring(this.config.kgfrom, this.config.kglength));
                                        var gmpart = parseInt(barcode.substring(this.config.grmfrom, this.config.grmlength));
                                        if (isNaN(kgpart)) kgpart = 0;
                                        if (isNaN(gmpart)) gmpart = 0;
                                        if (gmpart != 0) {
                                            wqty = parseFloat(kgpart.toFixed(0) + '.' + gmpart.toFixed(3));
                                        }
                                        else {
                                            wqty = kgpart;
                                        }
                                        return {
                                            'barcode': wbarcode,
                                            'qty': wqty
                                        };
                                    }
                                }
                                $scope.WeighingBarcodeParser = new WeighingBarcodeParser(barcodeparser.weighing);
                            }
                        } catch (x) {

                        }
                    }

                    $scope.sale_requestpasscodetosms = $scope.ApplicationSettings.appsettings.sale_requestpasscodetosms;
                    $scope.system_salesrounding = $scope.ApplicationSettings.appsettings.system_salesrounding;
                    $scope.system_salesroundingOnBarcode = typeof ($scope.ApplicationSettings.appsettings.system_salesroundingOnBarcode) != 'undefined' &&
                        $scope.ApplicationSettings.appsettings.system_salesroundingOnBarcode;

                    if ($scope.ApplicationSettings.appsettings.system_datechange &&
                        $scope.ApplicationSettings.nextdaycount > 0 &&
                        $filter('date')($scope.ApplicationSettings.currentday, 'dd-MM-yyyy') == $filter('date')(new Date(), 'dd-MM-yyyy')) {
                        $scope.IsDaylight = true;
                        $scope.DaylightCount = $scope.ApplicationSettings.nextdaycount;
                        $scope.dateStyle = '#f7dcdc';
                        $scope.CurrentBill.billdate = $scope.GetBillDate();
                    }

                    $scope.BillNoConfig.init($scope.ApplicationSettings.appsettings.system_multicompany);
                    //$scope.PromotionEnabled = $rootScope.License.promotions && $scope.ApplicationSettings.appsettings.sale_promotion;
                    $scope.AdditionalDiscountEnabled = $scope.ApplicationSettings.appsettings.sale_additionaldiscount;
                    $scope.EMIEnabled = $scope.ApplicationSettings.appsettings.sale_emi;
                    $scope.ExchangeEnabled = $scope.ApplicationSettings.appsettings.sale_exchange;
                    $scope.DiscountQueue = $scope.ApplicationSettings.appsettings.intray_discount;
                    $scope.IsDiscount = $scope.PromotionEnabled;
                    $scope.ManualRewards = $scope.ApplicationSettings.appsettings.sale_rewardmode == 1;
                    $scope.TemplateCurrentBill.areaid = $scope.ApplicationSettings.appsettings.sale_area;
                    $scope.TemplateCurrentBill.salereasonid = $scope.ApplicationSettings.appsettings.sale_salereason;
                    $scope.TemplateCurrentBill.seasonid = $scope.ApplicationSettings.appsettings.sale_season;
                    $scope.SOManager.Initialize($scope);
                    if ($scope.ApplicationSettings.appsettings.system_alwayscaps)
                        $scope.OnKeyPress = function (value) { return value.toUpperCase(); };
                }
                else {
                    $rootScope.Toaster.Error("Please save the application setting for the current location");
                    $rootScope.Triggers.SelectLocation('', function () {
                        $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                            typeof ($rootScope.LocationInfo.location) != 'undefined')
                        $scope.ShowLocationInfo();
                        $scope.ApplicationSettings();
                    });
                }
            }).error(function (result) {
                $rootScope.Toaster.Error("Please save the application setting for the current location");
                $rootScope.Triggers.SelectLocation('', function () {
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                        typeof ($rootScope.LocationInfo.location) != 'undefined')
                    $scope.ShowLocationInfo();
                    $scope.ApplicationSettings();
                });
            });
        }();

        $scope.GetBillDate = function () {
            if ($scope.IsDaylight == true) {
                var billdate = new Date();
                return billdate.setDate(billdate.getDate() + $scope.DaylightCount);
            }
            return new Date();
        }

        $scope.OnKeyPress = function (value) { return value; };
        $scope.AutoSettlement = false;

        if (!$scope.IsSaleReturn &&
            !$scope.IsBulkSale &&
            $rootScope.LocationInfo != null &&
            $rootScope.LocationInfo.location != null &&
            $rootScope.LocationInfo.location.additionalinfo != null &&
            $rootScope.LocationInfo.location.additionalinfo.autosettlement) {
            $scope.AutoSettlement = true;
            $rootScope.Toaster.Notice("Single window settlement enabled..");
        };

        if (!$scope.IsSaleReturn &&
            !$scope.IsBulkSale &&
            $rootScope.LocationInfo != null &&
            $rootScope.LocationInfo.counter != null &&
            $rootScope.LocationInfo.counter.additionalinfo != null &&
            $rootScope.LocationInfo.counter.additionalinfo.autosettlement) {
            $scope.AutoSettlement = true;
            $rootScope.Toaster.Notice("Single window settlement enabled..");
        };

        $scope.CreateNew = function () {
            $scope.UploadedMissingBarcodeMissing = null;
            $scope.UploadedMissingBarcodeMissing_Shortage = null;
            $scope.UploadedMissingBarcodeMissing_Missing = null;
            if ($scope.CurrentBill == undefined)
                $scope.CurrentBill = { 'isb2b': true,'billtype':1,'iscreditbill':true};

            var isb2b = $scope.CurrentBill.isb2b;
            var billtype = $scope.CurrentBill.billtype;
            var iscreditbill = $scope.CurrentBill.iscreditbill;

            $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
            $scope.CurrentBill = (JSON.parse(JSON.stringify($scope.TemplateCurrentBill)));
            $scope.CurrentBill.billdate = $scope.GetBillDate();
            $scope.CurrentBill.billdatedisplay = $filter('date')($scope.CurrentBill.billdate, 'dd-MM-yyyy');
            $scope.CurrentBill.billtimedisplay = $filter('date')($scope.CurrentBill.billdate, 'hh:mm a');
            $scope.CurrentBill.billattributes.packslipid = '';
            $scope.CurrentBill.isb2b = isb2b;
            $scope.CurrentBill.billtype = billtype;
            $scope.CurrentBill.iscreditbill = iscreditbill;
            $scope.additionaldiscountamountrequested = 0;
            $scope.additionaldiscountpercentrequested = 0;
            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.lastbit = "";
            $scope.AllowAdditionalDiscount = false;
            $scope.SearchCondition.isbulkreturn = false;
            $scope.SchemeReturnBillItems = [];
            $scope.ShowImage = false;
            $scope.SelectedCustomerid = 0;
            $scope.CurrentBillConfig.internallist = $scope.CurrentBill.Billitems;
            $scope.CheckPOSLayout(true);
            $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
            if ($scope.igrid != null && $scope.CurrentBillConfig.items != null)
                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');

            if ($scope.agrid != null && $scope.CurrentBill.additionalchargeslist != null)
                $scope.agrid.UpdateData($scope.CurrentBill.additionalchargeslist);

        };

        $scope.DeleteGroupItem = function (action, gitem) {

            $scope.CurrentBillConfig.items.splice($scope.CurrentBillConfig.items.indexOf(gitem), 1);
            $.each(gitem.list, function (k, item) {

                $scope.SOManager.RemoveItem(item);
                $scope.CurrentBill.Billitems.splice($scope.CurrentBill.Billitems.indexOf(item), 1);
                $scope.CurrentBill.tax = $scope.CurrentBill.tax - item.tax;
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round($scope.rounding_discount);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                if (item.qty > 0 || $scope.IsSaleReturn) {
                    $scope.CurrentBill.totalqty -= (item.qty).round(2);
                    $scope.CurrentBill.totalpiece -= (item.piece).round(2);
                }
                $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                $scope.CurrentBill.schemediscount = $scope.CurrentBill.schemediscount - item.schemediscount;
                $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty);

                var existingValue = null;
                angular.forEach($scope.CurrentBill.billTax, function (value, key) {
                    if (value.taxid == item.taxid) {
                        existingValue = value;
                    }
                });
                if (existingValue != null) {
                    existingValue.amount -= ((item.price - item.additionaldiscount) - item.tax);
                    existingValue.tax -= item.tax;
                }
            });
            if ($scope.CurrentBill.Billitems.length == 0) {
                $scope.CurrentBill.isbulkreturn = false;
                if (!$scope.BeginGridTransaction) {
                    $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                }
            }
            else {
                if (!$scope.BeginGridTransaction) {
                    $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                }
                $scope.UpdateOffer();
            }
            $scope.setFocusautocoding();
            return true;
        }

        $scope.ManageBillItem = function (action, item) {

            if (action == "Delete") {

                if (item.schemeid > 0 && $scope.IsSaleReturn) {
                    var totaldiscount = 0;
                    var totalreceivable = 0;
                    var schemeitems = [];
                    $.each($scope.SchemeReturnBillItems, function (k, v) {
                        if (v.schemeid == item.schemeid) {
                            totaldiscount += (v.schemediscount);
                            totalreceivable += (v.receivable + v.schemediscount);
                            schemeitems.push(v);
                        }
                    });

                    var percentage = (totaldiscount / totalreceivable * 100);
                    if (schemeitems.length > 1) {
                        var index = 0;
                        $.each(schemeitems, function (k, v) {
                            index++;
                            v.schemeid = 0;
                            if (schemeitems.length == index) {
                                v.receivable = (v.receivable + v.schemediscount) - totaldiscount;
                                v.schemediscount = totaldiscount;
                            }
                            else {
                                var ndiscount = ((v.receivable + v.schemediscount) * percentage / 100).round(0);
                                v.receivable = (v.receivable + v.schemediscount) - ndiscount;
                                v.schemediscount = ndiscount;
                                totaldiscount -= v.schemediscount;
                            }
                        });
                    }
                }

                if (!$scope.IsSaleReturn && typeof ($scope.CurrentBill.returnbillid) != 'undefined' && item.receivable < 0) {
                    $rootScope.Toaster.Notice("Can't delete return bill items, use * to clear!.");
                    return false;
                }

                $scope.SOManager.RemoveItem(item);

                $scope.CurrentBill.Billitems.splice($scope.CurrentBill.Billitems.indexOf(item), 1);
                $scope.CurrentBill.tax = $scope.CurrentBill.tax - item.tax;
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round($scope.rounding_discount);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                if (item.qty > 0 || $scope.IsSaleReturn) {
                    $scope.CurrentBill.totalqty -= (item.qty).round(2);
                    $scope.CurrentBill.totalpiece -= (item.piece).round(2);
                }
                $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                $scope.CurrentBill.schemediscount = $scope.CurrentBill.schemediscount - item.schemediscount;
                $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty);

                var existingValue = null;
                angular.forEach($scope.CurrentBill.billTax, function (value, key) {
                    if (value.taxid == item.taxid) {
                        existingValue = value;
                    }
                });

                if (existingValue != null) {
                    existingValue.amount -= ((item.price - item.additionaldiscount) - item.tax);
                    existingValue.tax -= item.tax;
                }
                if ($scope.CurrentBill.Billitems.length == 0) {
                    $scope.CurrentBill.isbulkreturn = false;
                }
                else
                    $scope.UpdateOffer();
                $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                $scope.setFocusautocoding();
                return true;
            }


            if (action == "SMChange") {
                $rootScope.Prompt('Salesman Change', 'Code', function (code) {
                    code = code.toString();
                    if (typeof (code) != 'undefined' && code.length > 0) {
                        restService.Get('EmployeeService/EmployeeCode/' + $scope.UserInfo.OrganizationID + '/' + $rootScope.LocationInfo.location.id + '/' + code.replace('.', ''), true).then(function (results) {
                            if ($scope.CurrentBillConfig.type != 'list' && item.list) {
                                $.each(item.list, function (k, v) {
                                    v.salesmancode = results.employeecode;
                                    v.salesmanname = results.name;
                                    v.salesmannameLast = results.name;
                                    v.salesmanid = results.id;
                                })
                            }
                            else {
                                item.salesmancode = results.employeecode;
                                item.salesmanname = results.name;
                                item.salesmannameLast = results.name;
                                item.salesmanid = results.id;
                            }
                            $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                        });
                    }
                })
            }


            if (action == "SMMChange") {

                if (item.attributes == undefined || item.attributes == null) item.attributes = {};
                if (item.attributes.smcodes == undefined || item.attributes.smcodes == null)
                    item.attributes.smcodes = [];
                $scope.SelectMultipleSalesmen(item);
                return;
            }

            if (action == "Edit") {

                if ($scope.IsSaleReturn) {
                    $rootScope.Prompt('Edit', 'Qty', function (count) {
                        var totalcount = $rootScope.GetNumber(count);

                        if (item.schemeid > 0 && $scope.IsSaleReturn) {
                            var totaldiscount = 0;
                            var totalreceivable = 0;
                            var schemeitems = [];
                            $.each($scope.SchemeReturnBillItems, function (k, v) {
                                if (v.schemeid == item.schemeid) {
                                    totaldiscount += v.discount;
                                    totalreceivable += (v.receivable + v.discount);
                                    schemeitems.push(v);
                                }
                            });

                            if (schemeitems.length > 1) {
                                var percentage = (totaldiscount / totalreceivable * 100) / item.qty * totalcount;
                                var index = 0;
                                $.each(schemeitems, function (k, v) {
                                    index++;
                                    v.schemeid = 0;
                                    if (schemeitems.length == index) {
                                        v.receivable = (v.receivable + v.discount) - totaldiscount;
                                        v.discount = totaldiscount;
                                    }
                                    else {
                                        var ndiscount = ((v.receivable + v.discount) * percentage / 100).round($scope.rounding_discount);
                                        v.receivable = (v.receivable + v.discount) - ndiscount;
                                        v.discount = ndiscount;
                                        totaldiscount -= v.discount;
                                    }
                                });
                            }
                        }

                        if (totalcount > 0) {

                            $timeout(function () {

                                if (item.qty > 0 || $scope.IsSaleReturn) {
                                    $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - item.qty).round(2);
                                    $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - item.piece).round(2);
                                }
                                $scope.CurrentBill.tax = ($scope.CurrentBill.tax - item.tax).round(5);
                                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
                                $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - item.schemediscount).round(2);
                                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                                $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty)).round(2);

                                item.discount = (item.discount / item.qty) * totalcount * -1;
                                item.schemediscount = (item.schemediscount / item.qty) * totalcount * -1;
                                item.qty = totalcount * -1;
                                if (item.sellingmode == 3)
                                    item.piece = -1;
                                else
                                    item.piece = totalcount * -1;

                                $scope.CurrentBill.schemediscount += item.schemediscount;
                                if (item.qty > 0 || $scope.IsSaleReturn) {
                                    $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + item.piece).round(2);
                                    $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + item.qty).round(2);
                                }
                                $scope.UpdateItemInfo(item, false);
                                $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                $scope.$digest();
                            }, 10);

                        }

                    }, item.qty * -1);
                }
                else {

                    $rootScope.Prompt('Edit', 'Qty', function (count) {
                        var totalcount = $rootScope.GetNumber(count);

                        if (totalcount > 0 || $scope.IsSaleReturn) {

                            $timeout(function () {

                                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - item.qty).round(2);
                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - item.piece).round(2);

                                $scope.CurrentBill.tax = ($scope.CurrentBill.tax - item.tax).round(5);
                                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
                                $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - item.schemediscount).round(2);
                                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                                $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty)).round(2);

                                item.discount = (item.discount / item.qty) * totalcount;
                                item.schemediscount = (item.schemediscount / item.qty) * totalcount;
                                item.qty = totalcount;
                                if (item.sellingmode == 3)
                                    item.piece = 1;
                                else
                                    item.piece = totalcount;

                                $scope.CurrentBill.schemediscount += item.schemediscount;
                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + item.piece).round(2);
                                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + item.qty).round(2);
                                $scope.UpdateItemInfo(item, false);
                                $scope.UpdateOffer();
                                $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                $scope.$digest();
                            }, 10);
                        }
                    }, item.qty);
                }
                return true;
            }
        };



        $scope.UpdateItemInfo = function (saledetail, recompute) {

            if (typeof (recompute) == 'undefined')
                recompute = false;


            saledetail.qty = $scope.GetNumber(saledetail.qty);
            saledetail.piece = $scope.GetNumber(saledetail.piece);

            if (recompute) {
                $scope.CurrentBill.tax = ($scope.CurrentBill.tax - saledetail.tax).round($scope.rounding_tax);
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - saledetail.discount).round($scope.rounding_discount);
                $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - saledetail.schemediscount).round($scope.rounding_discount);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - saledetail.receivable).round(2);
                $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (saledetail.additionaldiscount * saledetail.qty)).round($scope.rounding_discount);
            }

            if (typeof (saledetail.barcodeDetail) == 'undefined')
                saledetail.barcodeDetail = {
                    'taxid': saledetail.taxid,
                    'discount': (saledetail.discount / saledetail.qty),
                    'additionaldiscount': saledetail.additionaldiscount,
                    'stockid': saledetail.stockid,
                    'id': saledetail.salablegoodsid,
                    'companyid': saledetail.companyid
                };

            //+ saledetail.barcodeDetail.additionaldiscount - customer discount verfication.
            saledetail.discount = (saledetail.qty * (saledetail.barcodeDetail.discount)).round($scope.rounding_discount);

            if (typeof (saledetail.additionaldiscount) == 'undefined' || isNaN(saledetail.additionaldiscount))
                saledetail.additionaldiscount = 0;

            if (typeof (saledetail.schemediscount) == 'undefined' || isNaN(saledetail.schemediscount))
                saledetail.schemediscount = 0;


            if ($scope.system_b2bexclusivetax) {

                saledetail.baseprice = (saledetail.etprice - saledetail.etdiscount).round($scope.rounding_discount);
                if (isNaN(saledetail.baseprice))
                    saledetail.baseprice = saledetail.etprice;
                if (!$scope.CurrentBill.isinterstatesale) {
                    var halfTax = (saledetail.baseprice * saledetail.taxpercentage / 2 / 100).round($scope.rounding_tax);
                    saledetail.itemtax = (halfTax * 2).round($scope.rounding_tax);
                    saledetail.tax = (saledetail.qty * saledetail.itemtax).round($scope.rounding_tax);
                }
                else {
                    saledetail.itemtax = (saledetail.baseprice * saledetail.taxpercentage / 100).round($scope.rounding_tax)
                    saledetail.tax = (saledetail.qty * saledetail.baseprice * saledetail.taxpercentage / 100).round($scope.rounding_tax);
                }
                saledetail.baseprice = (saledetail.etprice - saledetail.etdiscount + saledetail.itemtax).round($scope.rounding_tax);
            }
            else {
                saledetail.baseprice = saledetail.price - saledetail.additionaldiscount - (saledetail.schemediscount / saledetail.qty).round($scope.rounding_tax);
                if (!$scope.CurrentBill.isinterstatesale) {
                    var basePrice = (saledetail.baseprice / (100 + (saledetail.taxpercentage)) * 100);
                    var halfTax = (basePrice * (saledetail.taxpercentage / 2) / 100).round($scope.rounding_tax);
                    saledetail.itemtax = (halfTax * 2).round($scope.rounding_tax);
                }
                else {
                    saledetail.itemtax = ((saledetail.baseprice / (100 + saledetail.taxpercentage)) * saledetail.taxpercentage).round($scope.rounding_tax);
                }
            }

            saledetail.tax = (saledetail.itemtax * saledetail.qty).round($scope.rounding_tax);
            saledetail.rate = (saledetail.baseprice - saledetail.itemtax).round($scope.rounding_rate);
            saledetail.receivable = (saledetail.baseprice * saledetail.qty).round($scope.rounding_amount);
            saledetail.taxid = saledetail.barcodeDetail.taxid;
            saledetail.stockid = saledetail.barcodeDetail.stockid;
            saledetail.salablegoodsid = saledetail.barcodeDetail.id;
            saledetail.companyid = saledetail.barcodeDetail.companyid;

            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + saledetail.tax).round($scope.rounding_tax);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + saledetail.discount).round($scope.rounding_discount);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount + saledetail.schemediscount).round($scope.rounding_discount);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + saledetail.receivable).round(2);
            $scope.CurrentBill.rounding = $scope.GetRoundingValue();
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + (saledetail.additionaldiscount * saledetail.qty)).round($scope.rounding_discount);
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

        $scope.ApplyCustomerDiscount = function () {

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid <= 0)
                return;

            if ((typeof ($scope.CurrentBill.customerdiscount) == 'undefined' || $scope.CurrentBill.customerdiscount <= 0) &&
                (typeof ($scope.CurrentBill.ccmargin) == 'undefined' || !$scope.CurrentBill.ccmarginmode))
                return;

            if ($scope.CurrentBill.discountotp && !$scope.CurrentBill.discountotpconfirmed) return;

            $.each($scope.CurrentBill.Billitems, function (k, item) {

                $scope.CurrentBill.tax = $scope.CurrentBill.tax - item.tax;
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round($scope.rounding_discount);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty);

                if (!$scope.IsUANBased && $scope.CurrentBill.ccmarginmode) {
                    if ($scope.CurrentBill.ccmarginmrp > 0) {
                        var mrpmargin = (item.barcodeDetail.mrp * $scope.CurrentBill.ccmarginmrp / 100).round($scope.rounding_rate);
                        var mrpcost = item.barcodeDetail.mrp - mrpmargin;
                        if (item.barcodeDetail.taxInfo != null && item.barcodeDetail.taxInfo.taxsplit != null) {

                            if (item.barcodeDetail.taxInfo.taxsplit.findTax == undefined || item.barcodeDetail.taxInfo.taxsplit.findTax == null)
                                item.barcodeDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                            var taxitem = item.barcodeDetail.taxInfo.taxsplit.findTax(item.barcodeDetail.mrp, false);
                            var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                            var beforetax = (item.barcodeDetail.mrp / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                            mrpcost -= beforetax;

                            //var taxitem = item.barcodeDetail.taxInfo.taxsplit.findTax(mrpcost, true);
                            //var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                            //var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                            //mrpcost += aftertax;
                        }
                        item.barcodeDetail.price = mrpcost;
                        item.barcodeDetail.baseprice = item.barcodeDetail.price;
                    }
                    else if ($scope.CurrentBill.ccmarginprice > 0) {
                        var mrpmargin = (item.barcodeDetail.price * $scope.CurrentBill.ccmarginprice / 100).round($scope.rounding_rate);
                        var mrpcost = item.barcodeDetail.price - mrpmargin;
                        if (item.barcodeDetail.taxInfo != null && item.barcodeDetail.taxInfo.taxsplit != null) {

                            if (item.barcodeDetail.taxInfo.taxsplit.findTax == undefined || item.barcodeDetail.taxInfo.taxsplit.findTax == null)
                                item.barcodeDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                            var taxitem = item.barcodeDetail.taxInfo.taxsplit.findTax(item.barcodeDetail.price, false);
                            var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                            var beforetax = (item.barcodeDetail.price / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                            mrpcost -= beforetax;

                            //var taxitem = item.barcodeDetail.taxInfo.taxsplit.findTax(mrpcost, true);
                            //var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                            //var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                            //mrpcost += aftertax;
                        }
                        item.barcodeDetail.price = mrpcost;
                        item.barcodeDetail.baseprice = item.barcodeDetail.price;
                    }
                    else if (item.barcodeDetail && item.barcodeDetail != null) {
                        item.barcodeDetail.price = (item.barcodeDetail.buyingprice + (item.barcodeDetail.buyingprice * $scope.CurrentBill.ccmargin / 100)).round($scope.rounding_rate);
                    }

                    //if ($scope.CurrentBill.ccmarginmrp > 0) {
                    //    var mrpmargin = (item.barcodeDetail.mrp * $scope.CurrentBill.ccmarginmrp / 100).round($scope.rounding_rate);
                    //    var mrpcost = item.barcodeDetail.mrp - mrpmargin;
                    //    if (item.taxInfo != null && item.taxInfo.taxsplit != null) {
                    //        var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(item.barcodeDetail.mrp, false);
                    //        var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                    //        var beforetax = (item.barcodeDetail.mrp / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                    //        mrpcost -= beforetax;

                    //        var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(mrpcost, true);
                    //        var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                    //        var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                    //        mrpcost += aftertax;
                    //    }
                    //    item.price = item.barcodeDetail.mrp - mrpdiscount;
                    //    item.baseprice = item.price;
                    //}
                    //else if ($scope.CurrentBill.ccmarginprice > 0) {
                    //    var mrpmargin = (item.barcodeDetail.price * $scope.CurrentBill.ccmarginprice / 100).round($scope.rounding_rate);
                    //    var mrpcost = item.barcodeDetail.price - mrpmargin;
                    //    if (item.taxInfo != null && item.taxInfo.taxsplit != null) {
                    //        var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(item.barcodeDetail.price, false);
                    //        var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                    //        var beforetax = (item.barcodeDetail.price / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                    //        mrpcost -= beforetax;

                    //        var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(mrpcost, true);
                    //        var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                    //        var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                    //        mrpcost += aftertax;
                    //    }
                    //    item.price = item.barcodeDetail.mrp - mrpdiscount;
                    //    item.baseprice = item.price;

                    //}
                    //else if (item.barcodeDetail.buyingprice > 0) {
                    //    var newprice = item.barcodeDetail.buyingprice + (item.barcodeDetail.buyingprice * $scope.CurrentBill.ccmargin / 100).round(0);
                    //    if (item.taxInfo != null && item.taxInfo.taxsplit != null) {
                    //        var taxitem = item.taxInfo.taxsplit.findTax(newprice, true);
                    //        var taxpercentage = taxitem.cgst + taxitem.sgst;
                    //        newprice += (newprice * taxpercentage / 100).round($scope.rounding_tax);
                    //    }
                    //    item.price = newprice;
                    //    item.baseprice = newprice;
                    //}

                }
                else {
                    if (typeof (item.attributes) == 'undefined' || item.attributes == null)
                        item.attributes = { 'customerdiscount': 0 };

                    var existingDiscountPercentage = (item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount) / item.barcodeDetail.price * 100;

                    if ($scope.CurrentBill.billdiscountinclude) {
                        if (existingDiscountPercentage < $scope.CurrentBill.customerdiscount) {
                            var addremaindiscount = $scope.CurrentBill.customerdiscount - existingDiscountPercentage;
                            item.additionaldiscount = (item.barcodeDetail.price * addremaindiscount / 100).round($scope.rounding_discount);
                            item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round($scope.rounding_discount);
                            item.attributes.customerdiscount = item.additionaldiscount;
                        }
                    }
                    else {
                        var addremaindiscount = $scope.CurrentBill.customerdiscount;
                        item.additionaldiscount = (item.barcodeDetail.price * addremaindiscount / 100).round($scope.rounding_discount);
                        item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round($scope.rounding_discount);
                        item.attributes.customerdiscount = item.additionaldiscount;
                    }

                    item.discount = (item.qty * (item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount)).round($scope.rounding_discount);

                    if (typeof (item.additionaldiscount) == 'undefined' || isNaN(item.additionaldiscount))
                        item.additionaldiscount = 0;

                    if (typeof (item.schemediscount) == 'undefined' || isNaN(item.schemediscount))
                        item.schemediscount = 0;

                    item.baseprice = (item.price - item.additionaldiscount - (item.schemediscount / item.qty).round($scope.rounding_discount));
                }

                if (typeof (item.taxInfo) != 'undefined' && item.taxInfo != null && item.taxInfo.taxsplit != null) {
                    if (item.taxInfo.taxsplit.findTax == undefined || item.taxInfo.taxsplit.findTax == null)
                        item.taxInfo.taxsplit.findTax = $scope.findTax;

                    var taxitem = item.taxInfo.taxsplit.findTax(item.baseprice);
                    item.taxpercentage = taxitem.cgst + taxitem.sgst;
                }

                if (item.baseprice == undefined || item.baseprice == null) {
                    if (typeof (item.additionaldiscount) == 'undefined' || isNaN(item.additionaldiscount))
                        item.additionaldiscount = 0;

                    if (typeof (item.schemediscount) == 'undefined' || isNaN(item.schemediscount))
                        item.schemediscount = 0;
                    item.baseprice = (item.price - item.additionaldiscount - (item.schemediscount / item.qty).round($scope.rounding_discount));
                }
                if (!$scope.CurrentBill.isinterstatesale) {
                    var basePrice = (item.baseprice / (100 + (item.taxpercentage)) * 100);
                    var halfTax = (basePrice * (item.taxpercentage / 2) / 100).round($scope.rounding_tax);
                    item.itemtax = (halfTax * 2).round($scope.rounding_tax);
                }
                else {
                    item.itemtax = ((item.baseprice / (100 + item.taxpercentage)) * item.taxpercentage).round($scope.rounding_tax);
                }

                item.tax = (item.itemtax * item.qty).round($scope.rounding_tax);
                item.taxid = item.taxid;
                item.rate = (item.baseprice - item.itemtax).round($scope.rounding_rate);
                item.receivable = (item.baseprice * item.qty).round($scope.rounding_amount);
                item.saleamount = item.qty * item.rate;

                $scope.CurrentBill.tax = $scope.CurrentBill.tax + item.tax;
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount + item.discount).round($scope.rounding_discount);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + item.receivable).round(2);
                $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount + (item.additionaldiscount * item.qty);

            });
        }

        $scope.ApplyExclusiveTax = function (price, isb2b, dogroup) {
            if (price <= 0)
                price = $scope.SalesDetail.price;

            if (typeof ($scope.SalesDetail.etprice) == 'undefined' || $scope.SalesDetail.etprice == 0) {
                var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(price, true);
                $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                $scope.SalesDetail.price = price * (100 + $scope.SalesDetail.taxpercentage) / 100;
            }
            $scope.AddBillItemInternal(isb2b, dogroup);
        }

        $scope.AddBillItem = function (isb2b, dogroup) {

            if ($scope.CurrentBill.creditlimit_disable) {
                $rootScope.Toaster.Error('Selected customer have crossed the credit limit!');
                return;
            }

            if ($scope.system_b2bexclusivetax || $scope.system_ismfgapplication) {
                if ($scope.system_supportpricelist) {
                    if ($scope.SalesDetail.attributes.designid &&
                        $scope.SalesDetail.attributes.designid != null) {
                        $scope.priceList.GetPrice($scope.CurrentBill.customerid,
                            $scope.CurrentBill.customercategoryid,
                            $scope.SalesDetail.attributes.designid,
                            function (price) {
                                $scope.ApplyExclusiveTax(price, isb2b, dogroup);
                            });
                    }
                    else {
                        $scope.ApplyExclusiveTax(0, isb2b, dogroup);
                    }
                } else {
                    $scope.ApplyExclusiveTax(0, isb2b, dogroup);
                }
            }
            else {
                $scope.AddBillItemInternal(isb2b, dogroup);
            }
        }

        $scope.AddBillItemInternal = function (isb2b, dogroup) {

            dogroup = typeof (dogroup) == 'undefined' || dogroup;
            if (typeof (isb2b) == 'undefined') {
                $scope.SalesDetail.additionaldiscount = 0;
                $scope.SalesDetail.additionaldiscountpercentage = 0;
            }

            if (typeof ($scope.SalesDetail.journalqty) == 'undefined')
                $scope.SalesDetail.journalqty = 0;

            if ($scope.SearchModeEnabled && typeof ($scope.SalesDetail.pricewithouttax) != 'undefined') {
                var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.pricewithouttax, true);
                $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
            }

            if ($scope.SalesDetail.qty > 0 && $scope.IsSaleReturn)
                $scope.SalesDetail.qty = $scope.GetNumber($scope.SalesDetail.qty) * -1;



            $scope.SalesDetail.discount = ($scope.SalesDetail.qty * ($scope.SalesDetail.barcodeDetail.discount + $scope.SalesDetail.barcodeDetail.additionaldiscount)).round($scope.rounding_discount);

            if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount)) {
                $scope.SalesDetail.additionaldiscount = 0;
                $scope.SalesDetail.additionaldiscountpercentage = 0;
            }

            if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount)) {
                $scope.SalesDetail.schemediscount = 0;
                $scope.SalesDetail.schemediscountpercentage = 0;
            }

            $scope.SalesDetail.baseprice = ($scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount - ($scope.SalesDetail.schemediscount / $scope.SalesDetail.qty).round($scope.rounding_discount));

            if (!$scope.IsUANBased) {
                if (typeof ($scope.CurrentBill.customerid) != 'undefined' && $scope.CurrentBill.customerid > 0 &&
                    typeof ($scope.CurrentBill.ccmarginmode) != 'undefined' && $scope.CurrentBill.ccmarginmode) {

                    if (!$scope.CurrentBill.discountotp || $scope.CurrentBill.discountotpconfirmed) {

                        if (typeof ($scope.SalesDetail.attributes) == 'undefined' || $scope.SalesDetail.attributes == null)
                            $scope.SalesDetail.attributes = { 'customerdiscount': 0 };

                        $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                            return arritem.id == $scope.SalesDetail.taxid;
                        });

                        if ($scope.SalesDetail.taxInfo != null) {
                            if ($scope.SalesDetail.taxInfo.taxsplit == null) {
                                $scope.SalesDetail.taxInfo.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                            }
                            $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                        }

                        if ($scope.CurrentBill.ccmarginmode) {
                            if ($scope.CurrentBill.ccmarginmrp > 0) {
                                var mrpmargin = ($scope.SalesDetail.barcodeDetail.mrp * $scope.CurrentBill.ccmarginmrp / 100).round($scope.rounding_rate);
                                var mrpcost = $scope.SalesDetail.barcodeDetail.mrp - mrpmargin;
                                if ($scope.SalesDetail.taxInfo != null && $scope.SalesDetail.taxInfo.taxsplit != null) {
                                    if ($scope.SalesDetail.taxInfo.taxsplit.findTax == undefined || $scope.SalesDetail.taxInfo.taxsplit.findTax == null)
                                        $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                                    if ($scope.CurrentBill.cccomputemode != 1 && 
                                        $scope.CurrentBill.cccomputemode != 5) {
                                        var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(mrpcost, false);
                                        var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                                        var beforetax = (mrpcost / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                                        mrpcost -= beforetax;
                                    }
                                    //var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(mrpcost, true);
                                    //var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                                    //var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                                    //mrpcost += aftertax;
                                }
                                $scope.SalesDetail.price = mrpcost.round($scope.rounding_rate);
                                $scope.SalesDetail.baseprice = $scope.SalesDetail.price;
                            }
                            else if ($scope.CurrentBill.ccmarginprice > 0) {
                                var mrpmargin = ($scope.SalesDetail.barcodeDetail.price * $scope.CurrentBill.ccmarginprice / 100).round($scope.rounding_rate);
                                var mrpcost = $scope.SalesDetail.barcodeDetail.price - mrpmargin;
                                if ($scope.SalesDetail.taxInfo != null && $scope.SalesDetail.taxInfo.taxsplit != null) {
                                    if ($scope.SalesDetail.taxInfo.taxsplit.findTax == undefined || $scope.SalesDetail.taxInfo.taxsplit.findTax == null)
                                        $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                                    var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(mrpcost, false);
                                    var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                                    var beforetax = (mrpcost / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                                    mrpcost -= beforetax;

                                    //var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(mrpcost, true);
                                    //var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                                    //var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                                    //mrpcost += aftertax;
                                }
                                $scope.SalesDetail.price = mrpcost;
                                $scope.SalesDetail.baseprice = $scope.SalesDetail.price;
                            }
                            else {
                                $scope.SalesDetail.cccostmode = true;
                                $scope.SalesDetail.price = ($scope.SalesDetail.barcodeDetail.buyingprice + ($scope.SalesDetail.barcodeDetail.buyingprice * $scope.CurrentBill.ccmargin / 100)).round($scope.rounding_rate);
                                $scope.SalesDetail.baseprice = ($scope.SalesDetail.barcodeDetail.buyingprice + ($scope.SalesDetail.barcodeDetail.buyingprice * $scope.CurrentBill.ccmargin / 100)).round($scope.rounding_rate)
                            }
                            $scope.SalesDetail.etprice = $scope.SalesDetail.price;
                        }

                        if ($scope.SalesDetail.taxInfo != null && $scope.SalesDetail.taxInfo.taxsplit != null) {
                            if ($scope.SalesDetail.taxInfo.taxsplit.findTax == undefined || $scope.SalesDetail.taxInfo.taxsplit.findTax == null)
                                $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                            var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.price, true);
                            $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                        }

                        if (!$scope.CurrentBill.isinterstatesale) {
                            var halfTax = ($scope.SalesDetail.price * $scope.SalesDetail.taxpercentage / 2 / 100).round($scope.rounding_tax);
                            $scope.SalesDetail.itemtax = (halfTax * 2).round($scope.rounding_tax);
                        }
                        else {
                            $scope.SalesDetail.itemtax = ($scope.SalesDetail.price * $scope.SalesDetail.taxpercentage / 100).round($scope.rounding_tax);
                        }
                        if ($scope.SalesDetail.cccostmode) {
                            $scope.SalesDetail.price = ($scope.SalesDetail.price + $scope.SalesDetail.itemtax).round(2);
                        }
                        else {
                            $scope.SalesDetail.price = ($scope.SalesDetail.price + $scope.SalesDetail.itemtax).round(0);
                        }
                        $scope.SalesDetail.baseprice = $scope.SalesDetail.price;
                    }
                }
                else if (typeof ($scope.CurrentBill.customerid) != 'undefined' && $scope.CurrentBill.customerid > 0 &&
                    typeof ($scope.CurrentBill.customerdiscount) != 'undefined' && $scope.CurrentBill.customerdiscount > 0) {

                    if (!$scope.CurrentBill.discountotp || $scope.CurrentBill.discountotpconfirmed) {

                        if (typeof ($scope.SalesDetail.attributes) == 'undefined' || $scope.SalesDetail.attributes == null)
                            $scope.SalesDetail.attributes = { 'customerdiscount': 0 };

                        var existingDiscountPercentage = ($scope.SalesDetail.barcodeDetail.discount + $scope.SalesDetail.barcodeDetail.additionaldiscount) / $scope.SalesDetail.barcodeDetail.price * 100;
                        if ($scope.system_b2bexclusivetax) {
                            if (existingDiscountPercentage < $scope.CurrentBill.customerdiscount) {
                                var addremaindiscount = $scope.CurrentBill.customerdiscount - existingDiscountPercentage;
                                $scope.SalesDetail.etdiscount = ($scope.SalesDetail.etprice * addremaindiscount / 100).round($scope.rounding_discount);
                                $scope.SalesDetail.additionaldiscountpercentage = existingDiscountPercentage + addremaindiscount;
                                $scope.SalesDetail.attributes.customerdiscount = $scope.SalesDetail.etdiscount;
                            }
                        }
                        else {
                            if ($scope.CurrentBill.billdiscountinclude) {
                                if (existingDiscountPercentage < $scope.CurrentBill.customerdiscount) {
                                    var addremaindiscount = $scope.CurrentBill.customerdiscount - existingDiscountPercentage;
                                    $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.barcodeDetail.price * addremaindiscount / 100).round($scope.rounding_discount);
                                    $scope.SalesDetail.additionaldiscountpercentage = ($scope.SalesDetail.additionaldiscount / $scope.SalesDetail.barcodeDetail.price * 100).round($scope.rounding_discount);
                                    $scope.SalesDetail.attributes.customerdiscount = $scope.SalesDetail.additionaldiscount;
                                }
                            }
                            else {
                                var addremaindiscount = $scope.CurrentBill.customerdiscount;
                                $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.barcodeDetail.price * addremaindiscount / 100).round($scope.rounding_discount);
                                $scope.SalesDetail.additionaldiscountpercentage = ($scope.SalesDetail.additionaldiscount / $scope.SalesDetail.barcodeDetail.price * 100).round($scope.rounding_discount);
                                $scope.SalesDetail.attributes.customerdiscount = $scope.SalesDetail.additionaldiscount;
                            }
                            $scope.SalesDetail.baseprice = ($scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount - ($scope.SalesDetail.schemediscount / $scope.SalesDetail.qty).round($scope.rounding_discount));
                        }
                        $scope.UpdateTax($scope.SalesDetail);
                    }
                }

                if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null &&
                    $scope.SalesDetail.taxInfo.taxsplit != null) {
                    if ($scope.SalesDetail.etprice && $scope.SalesDetail.etprice != null &&
                        $scope.SalesDetail.etprice > 0) {
                        if ($scope.SalesDetail.etdiscount == undefined || $scope.SalesDetail.etdiscount == null)
                            $scope.SalesDetail.etdiscount = 0;

                        if ($scope.SalesDetail.taxInfo.taxsplit.findTax == undefined || $scope.SalesDetail.taxInfo.taxsplit.findTax == null)
                            $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                        var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.etprice - $scope.SalesDetail.etdiscount, true);
                        $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                    }
                    else {
                        var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.baseprice, false);
                        $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                    }
                }


                if (!$scope.CurrentBill.isinterstatesale) {
                    var basePrice = ($scope.SalesDetail.baseprice / (100 + ($scope.SalesDetail.taxpercentage)) * 100);
                    var halfTax = (basePrice * ($scope.SalesDetail.taxpercentage / 2) / 100).round($scope.rounding_tax);
                    $scope.SalesDetail.itemtax = (halfTax * 2).round($scope.rounding_tax);
                }
                else {
                    $scope.SalesDetail.itemtax = (($scope.SalesDetail.baseprice / (100 + $scope.SalesDetail.taxpercentage)) * $scope.SalesDetail.taxpercentage).round($scope.rounding_tax);
                }
            }

            $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round($scope.rounding_tax);
            $scope.SalesDetail.taxid = $scope.SalesDetail.barcodeDetail.taxid;
            $scope.SalesDetail.rate = ($scope.SalesDetail.baseprice - $scope.SalesDetail.itemtax).round($scope.rounding_rate);



            $scope.SalesDetail.salerate = ($scope.SalesDetail.rate + $scope.SalesDetail.itemtax).round($scope.rounding_rate);
            $scope.SalesDetail.receivable = ($scope.SalesDetail.baseprice * $scope.SalesDetail.qty).round($scope.rounding_amount);
            $scope.SalesDetail.stockid = $scope.SalesDetail.barcodeDetail.stockid;
            $scope.SalesDetail.salablegoodsid = $scope.SalesDetail.barcodeDetail.id;
            $scope.SalesDetail.companyid = $scope.SalesDetail.barcodeDetail.companyid;
            $scope.SalesDetail.qty = parseFloat($scope.SalesDetail.qty);
            $scope.SalesDetail.schemediscount = 0;
            $scope.SalesDetail.schemereward = 0;
            $scope.SalesDetail.schemediscountcoupen = 0;

            if (isNaN($scope.SalesDetail.qty))
                $scope.SalesDetail.qty = 0;

            if ($scope.SalesDetail.sellingmode == 3)
                $scope.SalesDetail.piece = $scope.IsSaleReturn ? -1 : 1;
            else
                $scope.SalesDetail.piece = parseInt($scope.SalesDetail.qty);

            $scope.SalesDetail.saleamount = $scope.SalesDetail.qty * $scope.SalesDetail.rate;

            if ($scope.SalesDetail.qty == 0) {
                $rootScope.Toaster.Error("Zero Qty Can't be added");
                return;
            }

            if (!$scope.HSNLimit.validate($scope.CurrentBill.Billitems, $scope.SalesDetail)) {
                $rootScope.Toaster.Error("Can't add more than " + $scope.HSNLimit.max + ' hsn codes in single bill');
                return;
            }

            if ($scope.SalesDetail.etprice == undefined ||
                $scope.SalesDetail.etprice == null)
                $scope.SalesDetail.etprice = 0;

            if ($scope.SalesDetail.additionaldiscount == undefined ||
                $scope.SalesDetail.additionaldiscount == null)
                $scope.SalesDetail.additionaldiscount = 0;

            if ($scope.SalesDetail.additionaldiscountpercentage == undefined ||
                $scope.SalesDetail.additionaldiscountpercentage == null)
                $scope.SalesDetail.additionaldiscountpercentage = 0;

            //$scope.CurrentBill.Billitems.push($scope.SalesDetail);

            if (typeof ($scope.SalesDetail.attributes) == 'undefined' || $scope.SalesDetail.attributes == null)
                $scope.SalesDetail.attributes = {};

            $scope.SalesDetail.attributes.cost = $scope.SalesDetail.cost;
            $scope.SalesDetail.attributes.margin = $scope.SalesDetail.margin;

            if ($scope.SalesDetail.additionaldiscountpercentage > 0) {
                $scope.SalesDetail.etdiscount = ($scope.SalesDetail.etprice * $scope.SalesDetail.additionaldiscountpercentage / 100).round($scope.rounding_discount);
                $scope.SalesDetail.etdiscountpercentage = (($scope.SalesDetail.etdiscount / $scope.SalesDetail.etprice) * 100).round($scope.rounding_discount);
            }
            else {
                $scope.SalesDetail.etdiscount = 0;
                $scope.SalesDetail.etdiscountpercentage = 0;
            }

            if ($scope.Billingmode != 0) {
                if (typeof ($scope.SalesDetail.etprice) == 'undefined' ||
                    $scope.SalesDetail.etprice == null ||
                    $scope.SalesDetail.etprice == 0) {
                    $rootScope.Toaster.Error("Exclusive taxable price is missing");
                    return;
                }

                if (typeof ($scope.SalesDetail.etdiscountpercentage) == 'undefined' ||
                    $scope.SalesDetail.etdiscountpercentage == null) {
                    $rootScope.Toaster.Error("Exclusive discount percentage is missing");
                    return;
                }

                if (typeof ($scope.SalesDetail.etdiscount) == 'undefined' ||
                    $scope.SalesDetail.etdiscount == null) {
                    $rootScope.Toaster.Error("Exclusive discount value is missing");
                    return;
                }

                if ($scope.SalesDetail.etdiscount == 0 && ($scope.SalesDetail.additionaldiscount > 0)) {
                    $rootScope.Toaster.Error("Exclusive discount value is not matching with additionaldiscount");
                    return;
                }
            }

            if (typeof ($scope.SalesDetail.etprice) == 'undefined' ||
                $scope.SalesDetail.etprice == null ||
                $scope.SalesDetail.etprice == 0) {
                $scope.SalesDetail.etprice = $scope.SalesDetail.rate;
            }

            $scope.CalcualteTotal($scope.SalesDetail);
            $scope.CurrentBill.Billitems.splice(0, 0, $scope.SalesDetail);
            $scope.SOManager.UpdateItem($scope.SalesDetail, function () {
                $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
                $scope.UpdateOffer();
                $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                if (!$scope.BeginGridTransaction) {
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                }
                $scope.LocalContext.Post('$scope.CurrentBill', $scope.CurrentBill, false);
            });
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.CurrentBill', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.CurrentBill = value;
            $scope.CurrentBillConfig.internallist = $scope.CurrentBill.Billitems;
            $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
            $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
            if ($scope.CurrentBill.additionalchargeslist == null)
                $scope.CurrentBill.additionalchargeslist = [];
            $scope.agrid.UpdateData($scope.CurrentBill.additionalchargeslist);
        }

        $scope.UpdateOffertimeout = null;
        $scope.UpdateOffer = function () {
            if ($scope.IsSaleReturn || !$scope.IsDiscount) return;
            if ($scope.UpdateOffertimeout != null)
                $timeout.cancel($scope.UpdateOffertimeout);
            $scope.UpdateOffertimeout = $timeout(function () {
                $scope.FindOffers();
            }, 100);
        }

        $scope.FindOffers = function (callback) {

            $scope.CurrentBill.locationid = $rootScope.LocationInfo.location.id;
            $scope.CurrentBill.counterid = $rootScope.LocationInfo.counter.id;
            var request = {
                "Item": $scope.CurrentBill
            }
            $http({
                method: 'POST',
                url: "BillService/FindOffers",
                data: JSON.stringify(request)
            }).success(function (result) {
                if ($scope.CurrentBill.HasPendingPromotion || result.HasPendingPromotion) {
                    if ($scope.CurrentBill.Billitems.length == result.Billitems.length) {
                        $.each($scope.CurrentBill.Billitems, function (k, v) {

                            //var offerbillitem = result.Billitems.find(function(arritem) {
                            //    return arritem.barcode == v.barcode && arritem.salesmanid == v.salesmanid;
                            //});

                            offerbillitem = result.Billitems[k];
                            var lastdiscount = v.schemediscount;
                            if (typeof (offerbillitem) != 'undefined' && offerbillitem.schemediscount > 0) {
                                v.schemediscount = offerbillitem.schemediscount;
                                v.schemereward = offerbillitem.schemereward;
                                v.schemediscountcoupen = offerbillitem.schemediscountcoupen;
                                v.schemeid = offerbillitem.schemeid;
                                v.style = { "background-color": "lightgrey" };
                                v.schemediscountpercentage = ((v.schemediscount / v.qty) / (v.price + (v.discount / v.qty)) * 100).round(0);
                                if (typeof ($scope.Promotions) != 'undefined' && $scope.Promotions.length > 0) {
                                    var pinfo = $scope.Promotions.find(function (item) {
                                        return item.id == v.schemeid;
                                    });
                                    if (typeof (pinfo) != 'undefined' && pinfo != null)
                                        v.scheme = pinfo.name;
                                }
                            }
                            else {
                                v.schemediscount = 0;
                                v.schemereward = 0;
                                v.schemeid = 0;
                                v.schemediscountcoupen = 0;
                                v.scheme = '';
                                v.style = {};
                                v.schemediscountpercentage = 0;
                            }
                            if (lastdiscount != v.schemediscount) {
                                $scope.UpdateTax(v);
                                $scope.UpdateItemInfo(v, true);
                            }
                        });
                    }
                    $scope.CurrentBill.schemediscount = result.schemediscount;
                    $scope.CurrentBill.schemereward = result.schemereward;
                    $scope.CurrentBill.schemediscountcoupen = result.schemediscountcoupen;
                    $scope.CurrentBill.HasPendingPromotion = result.HasPendingPromotion;
                    $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                    $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                }
                else {
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                }
                if (typeof (callback) != 'undefined')
                    callback();
            });
        }

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.CalcualteTotal = function (saleitem) {
            if (saleitem.qty > 0 || $scope.IsSaleReturn) {
                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + saleitem.qty).round(2);
                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + saleitem.piece).round(2);
            }
            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + saleitem.tax).round($scope.rounding_tax);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + saleitem.discount).round($scope.rounding_discount);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount + saleitem.schemediscount).round($scope.rounding_discount);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + saleitem.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + (saleitem.additionaldiscount * saleitem.qty)).round($scope.rounding_discount);
            $scope.CurrentBill.rounding = $scope.GetRoundingValue();
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);

            var existingValue = null;

            if (typeof (saleitem.barcodeDetail) == 'undefined')
                saleitem.barcodeDetail = {
                    'taxid': saleitem.taxid,
                    'discount': (saleitem.discount / saleitem.qty),
                    'additionaldiscount': saleitem.additionaldiscount,
                    'stockid': saleitem.stockid,
                    'id': saleitem.salablegoodsid,
                    'companyid': saleitem.companyid
                };

            angular.forEach($scope.CurrentBill.billTax, function (value, key) {
                if (existingValue == null && value.taxid == saleitem.barcodeDetail.taxid) {
                    existingValue = value;
                }
            });

            if (existingValue == null) {
                existingValue = {
                    taxid: saleitem.barcodeDetail.taxid,
                    taxName: saleitem.taxname,
                    amount: saleitem.price - saleitem.tax,
                    tax: saleitem.tax,
                    taxpercentage: saleitem.taxpercentage
                }
                $scope.CurrentBill.billTax.push(existingValue)
            }
            else {
                existingValue.amount += (saleitem.price - saleitem.tax);
                existingValue.tax += saleitem.tax;
            }
            if (!$scope.BeginGridTransaction)
                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
        };

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            salesmanid: "",
            billmastercode: "",
            billno: null
        }

        $scope.SaveRequest = false;

        $scope.ValidateBill = function () {


            if ($scope.IsWarehouse) {
                $rootScope.Toaster.Error("Please check the sales location.");
                return false;
            }

            if ($scope.system_posdisablerefund && !$scope.IsSaleReturn) {
                if ($scope.CurrentBill.receivable < 0) {
                    $rootScope.Toaster.Error("Refund disabled, please contact your manager.");
                    return false;
                }
            }

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                $rootScope.Toaster.Notice('Please select the customer');
                return false;
            }

            if ($scope.CompanyGSTNO &&
                $scope.CompanyGSTNO != null &&
                $scope.CompanyGSTNO != '' &&
                $scope.CurrentBill.customergst &&
                $scope.CurrentBill.customergst != null &&
                $scope.CurrentBill.customergst != '') {
                if ($scope.CurrentBill.customergst == $scope.CompanyGSTNO) {
                    $rootScope.Toaster.Notice('Customer GST and Billing Company GST is same');
                    return false;
                }
            }

            if ($scope.system_salesmanmandatory) {
                var hassalesman = true;
                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    if (typeof (v.salesmanid) == 'undefined' || v.salesmanid <= 0)
                        hassalesman = false;
                });
                if (!hassalesman) {
                    $rootScope.Toaster.Notice('Please select the salesman code for all items.');
                    $scope.setFocusautocoding();
                    return false;
                }
            }


            if ($scope.IsSaleReturn) {
                $scope.CurrentBill.billtype = 8;
            }
            else if ($scope.IsBulkSale) {
                $scope.CurrentBill.billtype = 10;
                $scope.CurrentBill.isbulkreturn = true;
                if (
                    typeof ($scope.CurrentBill.billattributes) == 'undefined' ||
                    $scope.CurrentBill.billattributes == null ||
                    typeof ($scope.CurrentBill.billattributes.approvedby) == 'undefined' ||
                    $scope.CurrentBill.billattributes.approvedby == null ||
                    $scope.CurrentBill.billattributes.approvedby <= 0) {
                    $rootScope.Toaster.Notice('Please select approver');
                    $scope.setFocusbulkapprovedby();
                    return false;
                }

                if ($scope.BulkSaleReason.length > 0 &&
                    (typeof ($scope.CurrentBill.returnreasonid) == 'undefined' ||
                        $scope.CurrentBill.returnreasonid == null ||
                        $scope.CurrentBill.returnreasonid <= 0)) {
                    $rootScope.Toaster.Notice('Please select reason');
                    return false;
                }


                //if (!$rootScope.ShowDeliveryChallan) {
                if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                    $rootScope.Toaster.Notice('Please select the customer');
                    $scope.ShowCustomerDialog();
                    return false;
                }
                //}
            }
            else {
                if ($scope.CurrentBill.iscreditbill && (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0)) {
                    $rootScope.Toaster.Notice('Please select the customer');
                    $scope.ShowCustomerDialog();
                    return false;
                }
                $scope.CurrentBill.billtype = $scope.CurrentBill.iscreditbill ? 1 : 0;
            }

            if ($scope.system_sales_so_mandatory) {
                if (typeof ($scope.CurrentBill.billattributes.orderrefid) == 'undefined' ||
                    $scope.CurrentBill.billattributes.orderrefid == null ||
                    $scope.CurrentBill.billattributes.orderrefid == '') {
                    $rootScope.Toaster.Error('Sales order number mandatory');
                    return false;
                }
            }

            if ($scope.system_sales_so_partial) {
                var failedmessages = [];
                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    if (v.attributes && v.attributes.somissing) {
                        if (v.attributes.groupkey && v.attributes.size)
                            failedmessages.push(v.attributes.groupkey + '(' + v.attributes.size + ')');
                        else if (v.attributes.serialno)
                            failedmessages.push(v.attributes.serialno);
                        else
                            failedmessages.push(v.printingname);
                    }
                });
                //    if (failedmessages.length > 0) {
                //        $rootScope.Toaster.Error('Cant save partial/missing sales order - ' + failedmessages.join('<br>'));
                //        return false;
                //    }
            }


            $scope.CurrentBill.isexport = false;
            if ($scope.CurrentBill.additionalchargeslist &&
                $scope.CurrentBill.additionalchargeslist != null &&
                $scope.CurrentBill.additionalchargeslist.length > 0) {
                var exportgstitem = $scope.CurrentBill.additionalchargeslist.find(function (item) {
                    return item.chargetype == 'EXPORTGST';
                });
                $scope.CurrentBill.isexport = exportgstitem && exportgstitem != null;
                refreshtax = typeof (exportgstitem) == 'undefined' ||
                    exportgstitem == null || exportgstitem.Tax <= 0;
            }


            if ($scope.CurrentBill.isexport) {
                if ($scope.CurrentBill.isinterstatesale) {
                    $scope.CurrentBill.isinterstatesale = false;
                    $rootScope.Toaster.Notice('Export bill cannot be a interstate bill');
                    return false;
                }

                if (
                    $scope.CompanyCountryID != 0 && $scope.CurrentBill.customercountryid != 0 &&
                    $scope.CurrentBill.customercountryid == $scope.CompanyCountryID) {
                    $rootScope.Toaster.Error('Customer country and current company are same cant allow to create Export bill');
                    return false;
                }

                if ($scope.CurrentBill.customerid != null && $scope.CurrentBill.customerid > 0) {
                    if ($scope.CurrentBill.customergst != null &&
                        $scope.CurrentBill.customergst != '') {
                        $rootScope.Toaster.Error('Export customer cant have GST number');
                        return false;
                    }
                }
            }
            else if ($scope.CurrentBill.isinterstatesale) {
                if ($scope.CurrentBill.customerid == null || $scope.CurrentBill.customerid <= 0) {
                    $rootScope.Toaster.Error('Please select the customer for IGST bill');
                    $scope.CurrentBill.isinterstatesale = false;
                    return false;
                }

                if ($scope.CompanyGSTNO == null || $scope.CompanyGSTNO == '') {
                    $rootScope.Toaster.Error('Please update company gst number for IGST bill');
                    $scope.CurrentBill.isinterstatesale = false;
                    return false;
                }

                if ($scope.CompanyStateID == undefined ||
                    $scope.CompanyStateID == null || $scope.CompanyStateID <= 0) {
                    $rootScope.Toaster.Error('Please update company state for IGST bill');
                    $scope.CurrentBill.isinterstatesale = false;
                    return false;
                }

                if ($scope.CurrentBill.customerstateid == undefined ||
                    $scope.CurrentBill.customerstateid == null ||
                    $scope.CurrentBill.customerstateid <= 0) {
                    $rootScope.Toaster.Error('Customer state cant be null for IGST bill');
                    return false;
                }

                if ($scope.CurrentBill.customerstateid > 0 && $scope.CompanyStateID > 0 &&
                    $scope.CurrentBill.customerstateid == $scope.CompanyStateID) {
                    $rootScope.Toaster.Error('Customer state and current company state are same cant allow to create IGST bill');
                    return false;
                }

                if ($scope.CompanyCountryID != 0 && $scope.CurrentBill.customercountryid != 0 &&
                    $scope.CurrentBill.customercountryid != $scope.CompanyCountryID) {
                    $rootScope.Toaster.Error('Customer country and current company are not same cant allow to create IGST bill');
                    return false;
                }

                if ($scope.CurrentBill.customergst != null && $scope.CurrentBill.customergst != '') {
                    if ($scope.CurrentBill.customergst.substring(0, 2) == $scope.CompanyGSTNO.substring(0, 2)) {
                        $rootScope.Toaster.Error('Customer gst and current company gst are belongs to same state cant allow to create IGST bill');
                        return;
                    }
                }

                if ($scope.CurrentBill.customergst == null || $scope.CurrentBill.customergst == undefined || $scope.CurrentBill.customergst == '') {
                    if ($scope.CurrentBill.customerstateid == undefined ||
                        $scope.CurrentBill.customerstateid == null) {
                        $rootScope.Toaster.Error('Please select the state of customer to create IGST bill');
                        return false;
                    }
                }
            }
            else {
                if ($scope.CurrentBill.customerid != null && $scope.CurrentBill.customerid > 0) {
                    if ($scope.CurrentBill.customerstateid > 0 &&
                        $scope.CurrentBill.customerstateid != $scope.CompanyStateID) {
                        $rootScope.Toaster.Error('Customer state and current company state are different cant allow to create non IGST bill');
                        return;
                    }

                    if ($scope.CurrentBill.customergst != null &&
                        $scope.CurrentBill.customergst != '') {
                        if ($scope.CurrentBill.customergst.substring(0, 2) != $scope.CompanyGSTNO.substring(0, 2)) {
                            $rootScope.Toaster.Error('Customer gst and current company gst are not beloongs to same state cant allow to create non IGST bill');
                            return;
                        }
                    }
                }
            }

            if (!$scope.IsSaleReturn && !$scope.ValidateCredit())
                return false;

            if ($scope.CurrentBill.billattributes.packslipid &&
                $scope.CurrentBill.billattributes.packslipid != null &&
                $scope.CurrentBill.billattributes.packsliplist &&
                $scope.CurrentBill.billattributes.packsliplist != null &&
                $scope.CurrentBill.billattributes.packsliplist.length > 0) {
                var allpackslips = $scope.CurrentBill.billattributes.packslipid.split(',');
                var missingpackslips = [];
                $.each($scope.CurrentBill.billattributes.packsliplist, function (k, v) {
                    if (v != null && v != '' &&
                        allpackslips.indexOf(v) < 0) {
                        missingpackslips.push(v);
                    }
                })
                if (missingpackslips.length > 0) {
                    $.alert('Please scan pending packing slips ' + missingpackslips.join(','));
                    return false;
                }
            }

            $scope.CurrentBill.locationid = $rootScope.LocationInfo.location.id;
            $scope.CurrentBill.counterid = $rootScope.LocationInfo.counter.id;
            $scope.CurrentBill.countername = $scope.CounterName;
            $scope.CurrentBill.floorname = $scope.floorname;
            $scope.CurrentBill.floorid = $rootScope.LocationInfo.floorid;
            var refreshtax = true;

            if ($scope.system_supportpostaxtype) {
                if ($scope.CurrentBill.id &&
                    $scope.CurrentBill.id > 0 &&
                    $scope.CurrentBill.billattributes.billmode &&
                    $scope.CurrentBill.billattributes.billmode > 0) {
                    if ($scope.CurrentBill.billattributes.billmode != $scope.Billingmode) {
                        $rootScope.Toaster.Error('Bill already saved on ' + $scope.BillModes[$scope.CurrentBill.billattributes.billmode - 1].name + ' Mode \r\n' +
                            'Cant update with new mode ' + $scope.Billingmodename);
                        return false;
                    }
                }

                if ($scope.CurrentBill.billattributes == undefined ||
                    $scope.CurrentBill.billattributes == null) {
                    $scope.CurrentBill.billattributes = {};
                }
                $scope.CurrentBill.billattributes.billmode = $scope.Billingmode;
                $scope.CurrentBill.billattributes.billtemplateid = $scope.DefaultPrintingTemplateID
            }
            return $scope.Retotal(refreshtax);
        }

        $scope.Retotal = function (refreshtax) {
            var current = $scope.CurrentBill.total;
            if (refreshtax && !$scope.CurrentBill.isexport) {
                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    $scope.UpdateTax(v);
                    $scope.UpdateItemInfo(v, true);
                });
            }
            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;
            $.each($scope.CurrentBill.Billitems, function (k, v) {
                $scope.CurrentBill.tax += v.tax;
                $scope.CurrentBill.discount += v.discount;
                $scope.CurrentBill.receivable += v.receivable;
                $scope.CurrentBill.schemediscount += v.schemediscount;
                $scope.CurrentBill.additionaldiscount += (v.additionaldiscount * v.qty);
            });
            $scope.CurrentBill.rounding = $scope.GetRoundingValue();
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
            if ($scope.CurrentBill.total != current && refreshtax) {
                $.alert('Some data mismatch adjusted, please verify and save again or contact support team');
                return false;
            }
            return true;
        }
        $scope.Save = function (requestapproval, print) {

            if (!$scope.ValidateBill())
                return;

            if ($scope.CurrentBill.id > 0) {


                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    if (typeof (v.barcodeDetail) == 'undefined') {
                        v.discount = v.discount - v.schemediscount;
                        $scope.CurrentBill.discount = $scope.CurrentBill.discount - v.schemediscount;
                        v.barcodeDetail = {
                            'discount': v.discount / v.qty, 'additionaldiscount': v.additionaldiscount,
                            'taxid': v.taxid, 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid,
                            'companyid': v.companyid
                        };
                    }
                });

                if (!$scope.CurrentBill.HasPendingPromotion) {
                    var schemeditem = $scope.CurrentBill.Billitems.find(function (item) {
                        return item.schemediscount > 0 || item.schemeid > 0;
                    });
                    if (typeof (schemeditem) != 'undefined' && schemeditem != null) {
                        $scope.CurrentBill.HasPendingPromotion = true;
                        $rootScope.Toaster.Notice('Wait for offer updations.');
                        $scope.FindOffers(function () {
                            $rootScope.Toaster.Info('You can save now.');
                        });
                        return;
                    }
                }

                if ($scope.CurrentBill.id == undefined || $scope.CurrentBill.id == null ||
                    $scope.CurrentBill.id == '')
                    $scope.CurrentBill.id = 0;


                if (($scope.IsSaleReturn && $scope.CurrentBill.id == 0) || !$scope.system_supportbilledit ||
                    ($scope.CurrentBill.isbulkreturn && $scope.CurrentBill.id == 0)) {

                    if ($scope.CurrentBill.billno != '') {
                        $scope.CurrentBill.returnbillno = $scope.CurrentBill.billno;
                        $scope.CurrentBill.returnbillid = $scope.CurrentBill.id;
                    }

                    $scope.CurrentBill.id = 0;
                    $scope.CurrentBill.billno = '';
                }
            }

            //if ($scope.AutoSettlement && (!$scope.CurrentBill.iscreditbill || $scope.system_crsettlementonbill)) {
            //    $scope.ShowSettlement($scope.CurrentBill);
            //    return;
            //};

            if (typeof ($scope.CurrentBill.Billitems) == 'undefined' ||
                $scope.CurrentBill.Billitems == null ||
                $scope.CurrentBill.Billitems.length == 0) {
                return;
            }

            $scope.CurrentBill.isb2b = $scope.CurrentBill.iscreditbill ? true : false;
            //$scope.CurrentBill.billtype = 1;
            var request = {
                "Item": $scope.CurrentBill
            }

            if ($scope.SaveRequest) return;
            request.Item.printtemplateid = $scope.DefaultPrintingTemplateID;
            request.Item.papersize = $scope.PrinterData.PaperSize;
            request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
            $scope.SaveRequest = true;



            $http({
                method: 'POST',
                url: "BillService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {

                if (result.id > 0) {
                    $scope.ErrorMessage = "Successfully saved";
                    //if (print) {
                    //    $scope.Print(result.Bills[0]);
                    //}

                    if (print && !$scope.system_b2bprintdisable) {
                        if (result.printdocument == null || result.printdocument == '') {
                            $.each(result.Bills, function (k, v) {
                                if (v.printdocument != null && v.printdocument != '')
                                    $scope.Print(v);
                            });
                        }
                        else {
                            $scope.Print(result);
                        }
                    }

                    if ($scope.DiscountQueue && !$scope.discounttimerenabled && $scope.CurrentBill.additionaldiscount > 0) {
                        $scope.discounttimerenabled = true;
                        $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                    }

                    $scope.AddBillHistory(result, false, true)
                    $scope.CreateNew();

                    if ($scope.DiscountQueue) {
                        $scope.AllowAdditionalDiscount = false;
                    }
                    $scope.SaveRequest = false;

                }
                else {
                    $scope.ErrorMessage = result.errormessage;
                    if (typeof (result.errormessage) != 'undefined' &&
                        result.errormessage != null && result.errormessage != '')
                        $rootScope.Toaster.Error(result.errormessage);
                    $scope.SaveRequest = false;
                }

            }).error(function (result) {
                $scope.ErrorMessage = result;
                $scope.SaveRequest = false;
            });
        }

        $scope.SearchCustomer = function () {
            if ($scope.CurrentBill.phoneno == null || $scope.CurrentBill.phoneno == "") {
                $scope.CurrentBill.customername = "Cash";
                $scope.setFocuscreditbill();
                return;
            }
            restService.Get('CustomerService/SearchByPhoneNumber?id=' + $scope.CurrentBill.phoneno + '&lid=' + $rootScope.LocationInfo.location.id, true).then(function (results) {

                if (results.id > 0) {
                    var existingCustomer = $scope.Customers.find(function (item) {
                        return item.id == results.id;
                    });
                    if (typeof (existingCustomer) == 'undefined' || existingCustomer == null)
                        $scope.Customers.push(results);

                    $scope.CurrentBill.customerid = results.id;
                    $scope.CustomerChanged();
                }
                //$scope.ApplyCustomerInfo(results);
            });
        }

        $scope.CostChanged = function (item, newValue, oldValue) {

            if (typeof (item.qty) == 'undefined' || isNaN(item.qty)) item.qty = 0;
            if (typeof (item.piece) == 'undefined' || isNaN(item.piece)) item.piece = 0;
            if (typeof (item.tax) == 'undefined' || isNaN(item.tax)) item.tax = 0;
            if (typeof (item.discount) == 'undefined' || isNaN(item.discount)) item.discount = 0;
            if (typeof (item.schemediscount) == 'undefined' || isNaN(item.schemediscount)) item.schemediscount = 0;
            if (typeof (item.receivable) == 'undefined' || isNaN(item.receivable)) item.receivable = 0;
            if (typeof (item.additionaldiscount) == 'undefined' || isNaN(item.additionaldiscount)) item.additionaldiscount = 0;

            if (item.qty > 0 || $scope.IsSaleReturn) {
                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - item.qty).round(2);
                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - item.piece).round(2);
            }
            $scope.CurrentBill.tax = ($scope.CurrentBill.tax - item.tax).round($scope.rounding_tax);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - item.schemediscount).round(2);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty)).round(2);


            var cost = $scope.GetNumber(newValue);
            item.price = cost;
            item.amount = cost * $scope.GetNumber(item.qty);
            item.rate = item.price;


            if (typeof (item.taxInfo) == 'undefined' || item.taxInfo == null) {
                item.taxInfo = $scope.SalesTax.find(function (arritem) {
                    return arritem.id == item.taxid;
                });
            }

            item.taxpercentage = item.taxInfo.taxpercentage;
            item.taxname = item.taxInfo.name;

            item.baseprice = item.price;
            if (item.taxInfo != null && item.taxInfo.taxsplit != null) {
                if (typeof (item.taxInfo.taxsplit.findTax) == 'undefined')
                    item.taxInfo.taxsplit.findTax = $scope.findTax;
                var taxitem = item.taxInfo.taxsplit.findTax(item.baseprice, true);
                item.taxpercentage = taxitem.cgst + taxitem.sgst;
            }

            if (!$scope.CurrentBill.isinterstatesale) {
                var halfTax = (item.price * (item.taxpercentage / 2) / 100).round($scope.rounding_tax);
                item.itemtax = (halfTax * 2).round($scope.rounding_tax);
            }
            else {
                item.itemtax = (item.price * item.taxpercentage / 100).round($scope.rounding_tax);
            }

            item.salerate = item.price + item.itemtax;
            item.price = (item.price + item.itemtax).round(0);
            item.tax = (item.itemtax * item.qty).round($scope.rounding_tax);
            item.receivable = (item.price * item.qty).round($scope.rounding_amount);
            if (typeof (item.list) != 'undefined' && item.list != null) {
                $.each(item.list, function (k, v) {
                    v.price = item.price;
                    v.itemtax = item.itemtax;
                    v.cost = cost;
                    v.rate = cost;
                    v.salerate = item.salerate;
                    v.saleamount = item.salerate * v.qty;
                    v.attributes.cost = cost;
                    v.taxpercentage = item.taxpercentage;
                    v.taxname = item.taxname;
                    v.baseprice = item.baseprice;
                    v.margin = 0;
                    v.tax = (v.qty * v.itemtax).round(0);
                    v.receivable = (v.qty * v.price).round($scope.rounding_amount);
                });
            }

            if (item.qty > 0 || $scope.IsSaleReturn) {
                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + item.qty).round(2);
                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + item.piece).round(2);
            }
            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + item.tax).round($scope.rounding_tax);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + item.discount).round($scope.rounding_discount);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount + item.schemediscount).round($scope.rounding_discount);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + item.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + (item.additionaldiscount * item.qty)).round($scope.rounding_discount);
            $scope.CurrentBill.rounding = $scope.GetRoundingValue();
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
        }

        $scope.MarginChanged = function (item, newvalue) {
            var margin = $scope.GetNumber(newvalue);
            var newprice = (item.attributes.margincost + (item.attributes.margincost * margin / 100)).round(2);
            $scope.PriceChanged(item, newprice, newprice);
        }

        $scope.PriceChanged = function (item, newValue, oldValue) {

            if (typeof (item.qty) == 'undefined' || isNaN(item.qty)) item.qty = 0;
            if (typeof (item.piece) == 'undefined' || isNaN(item.piece)) item.piece = 0;
            if (typeof (item.tax) == 'undefined' || isNaN(item.tax)) item.tax = 0;
            if (typeof (item.discount) == 'undefined' || isNaN(item.discount)) item.discount = 0;
            if (typeof (item.schemediscount) == 'undefined' || isNaN(item.schemediscount)) item.schemediscount = 0;
            if (typeof (item.receivable) == 'undefined' || isNaN(item.receivable)) item.receivable = 0;
            if (typeof (item.additionaldiscount) == 'undefined' || isNaN(item.additionaldiscount)) item.additionaldiscount = 0;

            if (item.qty > 0 || $scope.IsSaleReturn) {
                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - item.qty).round(2);
                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - item.piece).round(2);
            }
            $scope.CurrentBill.tax = ($scope.CurrentBill.tax - item.tax).round($scope.rounding_tax);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round($scope.rounding_discount);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - item.schemediscount).round($scope.rounding_discount);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty)).round($scope.rounding_discount);

            if (item.attributes && item.attributes.cost) {
                var cost = item.attributes.cost;
                item.attributes.lossprice = (cost && cost > $scope.GetNumber(newValue));
            }

            item.discount = 0;
            item.additionaldiscount = 0;
            item.additionaldiscountpercentage = 0;
            item.discountpercentage = 0;
            item.etdiscount = 0;
            item.etdiscountpercentage = 0;

            if (typeof (item.taxInfo) == 'undefined' || item.taxInfo == null) {
                item.taxInfo = $scope.SalesTax.find(function (arritem) {
                    return arritem.id == item.taxid;
                });
            }

            item.taxpercentage = item.taxInfo.taxpercentage;
            item.taxname = item.taxInfo.name;

            if ($scope.system_b2bexclusivetax) {
                // && $scope.system_showexclusivetaxonpos
                item.etprice = $scope.GetNumber(newValue);
                if (typeof (item.taxInfo.taxsplit.findTax) == 'undefined')
                    item.taxInfo.taxsplit.findTax = $scope.findTax;
                var taxitem = item.taxInfo.taxsplit.findTax(item.etprice, true);
                item.taxpercentage = taxitem.cgst + taxitem.sgst;
                item.price = item.etprice * (100 + item.taxpercentage) / 100;
                item.salerate = item.price;
            }
            else {
                item.salerate = $scope.GetNumber(newValue);
                item.price = $scope.GetNumber(newValue);
                if (item.taxInfo != null && item.taxInfo.taxsplit != null) {
                    if (typeof (item.taxInfo.taxsplit.findTax) == 'undefined')
                        item.taxInfo.taxsplit.findTax = $scope.findTax;
                    var taxitem = item.taxInfo.taxsplit.findTax(item.price, false);
                    item.taxpercentage = taxitem.cgst + taxitem.sgst;
                }
            }

            item.baseprice = item.price / (100 + item.taxpercentage) * 100;
            item.etprice = item.baseprice;

            if (!$scope.CurrentBill.isinterstatesale) {
                var halfTax = (item.baseprice * (item.taxpercentage / 2) / 100).round($scope.rounding_tax);
                item.itemtax = (halfTax * 2).round($scope.rounding_tax);
            }
            else {
                item.itemtax = (item.baseprice * item.taxpercentage / 100).round($scope.rounding_tax);
            }

            item.rate = item.price - item.itemtax;
            item.saleamount = item.salerate * $scope.GetNumber(item.qty);
            item.amount = item.rate * $scope.GetNumber(item.qty);
            item.baseprice = item.rate;
            item.price = item.salerate;
            item.tax = (item.itemtax * item.qty).round($scope.rounding_tax);
            item.receivable = (item.price * item.qty).round($scope.rounding_amount);
            if (typeof (item.list) != 'undefined' && item.list != null) {
                var additionaldiscountpercentage = 0;
                var discountpercentage = 0;
                $.each(item.list, function (k, v) {
                    v.price = item.price;
                    v.etprice = item.etprice;
                    v.itemtax = item.itemtax;
                    v.salerate = item.salerate;
                    v.saleamount = item.salerate * v.qty;
                    v.taxpercentage = item.taxpercentage;
                    v.taxname = item.taxname;
                    v.baseprice = item.baseprice;
                    v.margin = 0;
                    if (v.additionaldiscount > 0)
                        v.additionaldiscountpercentage = (v.additionaldiscount / v.price * 100).round($scope.rounding_discount)
                    if (v.discount > 0)
                        v.discountpercentage = (v.discount / v.price * 100).round($scope.rounding_discount)
                    additionaldiscountpercentage += v.additionaldiscountpercentage;
                    discountpercentage += v.discountpercentage;
                    v.tax = (v.qty * v.itemtax).round($scope.rounding_tax);
                    v.receivable = (v.qty * v.price).round($scope.rounding_amount);
                });
                if (additionaldiscountpercentage > 0)
                    item.additionaldiscountpercentage = additionaldiscountpercentage / item.list.length;
                if (discountpercentage > 0)
                    item.discountpercentage = discountpercentage / item.list.length;
            }

            if (item.qty > 0 || $scope.IsSaleReturn) {
                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + item.qty).round(2);
                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + item.piece).round(2);
            }
            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + item.tax).round($scope.rounding_tax);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + item.discount).round($scope.rounding_discount);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount + item.schemediscount).round($scope.rounding_discount);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + item.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + (item.additionaldiscount * item.qty)).round($scope.rounding_discount);
            $scope.CurrentBill.rounding = $scope.GetRoundingValue();
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable.round(2)).round(2);

        }

        $scope.LoadReturnSO = function (items) {
            var barcodes = [];
            $.each(items, function (k, v) {
                for (var i = 0; i < v.pendingqty; i++) {
                    barcodes.push(v.uid);
                }
            });
            var request =
            {
                Item:
                {
                    'customerid': $scope.CurrentBill.customerid,
                    'locationid': $rootScope.LocationInfo.location.id,
                    'barcodes': barcodes,
                    'returngoods': true,
                    'avoidprice': true,
                    'ccmarginmrp': $scope.CurrentBill.ccmarginmrp,
                    'ccmarginprice': $scope.CurrentBill.ccmarginprice,
                    'cccomputemode': $scope.CurrentBill.cccomputemode,
                    'sono': $scope.CurrentBill.billattributes.orderrefid
                }
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SalablegoodsService/SearchBarcode",
                data: srequest
            }).success(function (barcoderesults) {
                $.each(barcoderesults, function (k, v) {
                    var soitemdetail = items.find(function (soitem) {
                        return soitem.uid == v.serialno;
                    });
                    if (soitemdetail && soitemdetail != null) {
                        v.price = soitemdetail.price;
                        v.priceextax = soitemdetail.price;
                        v.etprice = soitemdetail.price;
                    }
                })
                $scope.AddGroupItem(barcoderesults);
                $scope.autoCoding.code = ''
            }).error(function (result) {
            });
        }

        $scope.CustomerChanged = function (callback) {
            if (typeof ($scope.CurrentBill.customerid) != 'undefined' && $scope.CurrentBill.customerid != null) {

                if ($scope.SelectedCustomerid == 0 && $scope.CurrentBill.Billitems.length == 0)
                    $scope.SelectedCustomerid = $scope.CurrentBill.customerid;
                else if ($scope.SelectedCustomerid > 0 && $scope.CurrentBill.customerid != $scope.SelectedCustomerid
                    && $scope.CurrentBill.Billitems.length > 0) {
                    $rootScope.Toaster.Info("Items are cleard.");
                    $scope.CreateNew();
                    return;
                }

                $scope.CurrentBill.isinterstatesale = false;

                if ($scope.SOManager && !$scope.SOManager.ValidateSO($scope.CurrentBill.customerid)) {
                    $scope.CurrentBill.billattributes.orderrefid = '';
                }

                restService.Get('CustomerService/SearchByID?id=' + $scope.CurrentBill.customerid, true).then(function (results) {
                    $scope.ApplyCustomerInfo(results);
                    if (results.gstno != null && results.gstno != '' && $scope.BuyingCompany.length == 1) {
                        var statecode = results.gstno.substring(0, 2);
                        $scope.BuyingCompany.find(function (item) {
                            if (item.gstno != null &&
                                item.gstno.length > 10 &&
                                !item.gstno.startsWith(statecode)) {
                                $scope.CurrentBill.isinterstatesale = true;
                            }
                        });
                    }

                    $scope.CurrentBill.billattributes.hasshiptto = results.customertype == 7;
                    if (typeof (callback) != 'undefined' && typeof (callback) != null)
                        callback(results);
                });
            }

            else {
                $scope.CurrentBill.customername = '';
                $scope.CurrentBill.customergst = '';
                $scope.CurrentBill.customerdiscount = 0;
                $scope.CurrentBill.ccmargin = 0;
                $scope.CurrentBill.ccmarginmode = false;
                $scope.CurrentBill.phoneno = '';
                $scope.CurrentBill.isinterstatesale = false;
                if ($scope.CurrentBill.billattributes)
                    $scope.CurrentBill.billattributes.hasshiptto = false;
            }
        }

        $scope.ApplyCustomerInfo = function (results) {

            if ($scope.system_supportpricelist)
                $scope.priceList.LoadPrice(results.id);

            $scope.CurrentBill.phoneno = results.mobileno;
            $scope.CurrentBill.customername = results.name;
            if (results.id != $scope.CurrentBill.customerid)
                $scope.CurrentBill.customerid = results.id;
            $scope.CurrentBill.customergst = results.gstno;
            $scope.CurrentBill.customerstateid = results.stateid;
            $scope.CurrentBill.customercountryid = results.countryid;
            $scope.CurrentBill.customerdiscount = results.discount;
            $scope.CurrentBill.customercategoryid = results.categoryid;
            $scope.CurrentBill.pricesource = results.pricesource;
            $scope.CurrentBill.ccmargin = results.ccmargin;
            $scope.CurrentBill.ccmarginmode = results.ccmarginmode;
            $scope.CurrentBill.ccmarginmrp = results.ccmarginmrp;
            $scope.CurrentBill.ccmarginprice = results.ccmarginprice;
            $scope.CurrentBill.cccomputemode = results.cccomputemode;
            $scope.CurrentBill.rewardunit = results.rewardunit;
            $scope.CurrentBill.billdiscountinclude = results.billdiscountinclude;
            $scope.CurrentBill.discountotp = results.discountotp;
            $scope.CurrentBill.validatecredit = results.supportcredit;
            $scope.CurrentBill.creditlimit = results.creditlimit;
            $scope.CurrentBill.creditlimit_pending = results.creditlimit_pending;
            $scope.CurrentBill.creditlimit_delay = results.creditlimit_delay;
            $scope.CurrentBill.creditlimit_approve = !results.creditlimit_disable;
            $scope.CurrentBill.creditlimit_disable = $scope.system_creditlimit && !$scope.intray_b2bcredit && results.creditlimit_disable;
            $scope.leftPanStyle = $scope.IsSaleReturn ? '#f7dcdc' : ($scope.IsBulkSale ? '#cff7db' : 'white');

            if ($scope.system_creditlimit && !$scope.IsSaleReturn && !$scope.IsBulkSale) {
                if (results.ErrorMessage && results.ErrorMessage != null && results.ErrorMessage != '') {
                    $rootScope.Toaster.Error(results.ErrorMessage);
                    $scope.leftPanStyle = $scope.IsSaleReturn ? '#f7dcdc' : ($scope.IsBulkSale ? '#cff7db' : 'white');
                }
            }

            if ($scope.CurrentBill.discountotp && ($scope.CurrentBill.ccmarginmode || $scope.CurrentBill.customerdiscount > 0)) {
                $scope.CurrentBill.discountotpconfirmed = false;
                $rootScope.Toaster.Error('Customer OTP Require for special discounts!');
                $scope.RequestCustomerOTP();
            }
            else {
                $scope.CurrentBill.discountotpconfirmed = false;
                $scope.CurrentBill.discountotp = false;
                $scope.ApplyCustomerDiscount();
            }
        }

        $scope.ValidateCredit = function () {

            if (typeof ($scope.CurrentBill.id) == 'undefined' || $scope.CurrentBill.id <= 0) {
                $scope.CurrentBill.billattributes.printrequiredapproval = '';
            }
            if (!$scope.CurrentBill.validatecredit)
                return true;
            if ($scope.CurrentBill.creditlimit_disable) {
                $rootScope.Toaster.Error('Selected customer have crossed the credit limit!');
                return false;
            }
            if (!$scope.intray_b2bcredit) {
                if ($scope.CurrentBill.creditlimit_delay > 0) {
                    $rootScope.Toaster.Error('Selected customer already have pending amount of ' + $scope.CurrentBill.creditlimit_delay);
                    return false;
                }
                if ($scope.CurrentBill.total > 0 && $scope.CurrentBill.creditlimit_pending) {
                    $rootScope.Toaster.Error('Maximum allowed credit - ' + $scope.CurrentBill.creditlimit_pending);
                    return false;
                }
                return true;
            }
            else {
                var exceptionreasons = [];
                if ($scope.CurrentBill.creditlimit_delay > 0) {
                    $rootScope.Toaster.Error('Need Approval - Selected customer already have pending amount of ' + $scope.CurrentBill.creditlimit_delay);
                    exceptionreasons.push('Selected customer already have pending amount of ' + $scope.CurrentBill.creditlimit_delay);
                }
                if ($scope.CurrentBill.total > 0 && $scope.CurrentBill.creditlimit_pending) {
                    $rootScope.Toaster.Error('Need Approval - Maximum allowed credit - ' + $scope.CurrentBill.creditlimit_pending);
                    exceptionreasons.push('Maximum allowed credit - ' + $scope.CurrentBill.creditlimit_pending);
                }
                if (exceptionreasons.length > 0 && typeof ($scope.CurrentBill.id) == 'undefined' || $scope.CurrentBill.id <= 0) {
                    $scope.CurrentBill.billattributes.printrequiredapproval = exceptionreasons.join(',');
                }
                return true;
            }
        }

        $scope.SearchSalesmen = function () {
            if ($scope.SalesDetail.salesmannameLast != null &&
                $scope.SalesDetail.salesmannameLast != "" &&
                $scope.SalesDetail.salesmannameLast == $scope.SalesDetail.salesmanname) {
                $scope.AddBillItem();
                $scope.setFocusBarcode();
                return;
            }

            if ($scope.SalesDetail.salesmanname == null || $scope.SalesDetail.salesmanname == '') {
                $scope.AddBillItem();
                $scope.setFocusBarcode();
            }
            else {
                restService.Get('EmployeeService/EmployeeCode/' + $scope.UserInfo.OrganizationID + '/' + $rootScope.LocationInfo.location.id + '/' + $scope.SalesDetail.salesmanname, true).then(function (results) {
                    $scope.SalesDetail.salesmancode = results.employeecode;
                    $scope.SalesDetail.salesmanname = results.name;
                    $scope.SalesDetail.salesmannameLast = results.name;
                    $scope.SalesDetail.salesmanid = results.id;
                });
            }
        }

        $scope.CheckLocation = function () {
            return $rootScope.LocationInfo != null;
        }

        $scope.SearchPackBarcode = function (barcode) {
            restService.Get('PackbarcodeService/SearchPack/' + $rootScope.LocationInfo.location.id + '/' + $scope.qEncodeURIComponent(barcode)).then(function (results) {

                var barcodes = [];
                $.each(results.barcodes, function (k, v) {
                    barcodes.push(v.barcode);
                });

                var request =
                {
                    Item:
                    {
                        'customerid': $scope.CurrentBill.customerid,
                        'locationid': $rootScope.LocationInfo.location.id,
                        'shipcustomerid': ($scope.CurrentBill.billattributes && $scope.CurrentBill.billattributes.hasshiptto) ? $scope.CurrentBill.billattributes.shiptoid : 0,
                        'barcodes': barcodes,
                        'qty': $scope.autoCoding.qty,
                        'sono': $scope.CurrentBill.billattributes.orderrefid,
                        'usedstock': $scope.CurrentBillConfig.stockqty(),
                        'ccmarginmrp': $scope.CurrentBill.ccmarginmrp,
                        'ccmarginprice': $scope.CurrentBill.ccmarginprice,
                        'cccomputemode': $scope.CurrentBill.cccomputemode,
                    }
                };

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SalablegoodsService/SearchBarcode",
                    data: srequest
                }).success(function (barcoderesults) {
                    $.each(barcoderesults, function (k, v) {
                        v.packingno = results.packingno;
                        var qtybarcode = results.barcodes.find(function (item) {
                            return item.barcode == v.barcode;
                        });

                        if (qtybarcode == undefined || qtybarcode == null) {
                            qtybarcode = results.barcodes.find(function (item) {
                                return item.barcode == v.obarcode;
                            });
                        }

                        if (qtybarcode && qtybarcode != null) {
                            v.soboxid = qtybarcode.soboxid;
                            v.fgboxid = qtybarcode.fgboxid;
                        }
                    })
                    $scope.AddGroupItem(barcoderesults);
                    $scope.autoCoding.code = ''

                }).error(function (result) {
                });
            });
        }

        $scope.SearchPackingSlip = function (barcode) {
            restService.Get('PackingslipService/SearchPackingNo/' + $rootScope.LocationInfo.location.id + '/' + $scope.qEncodeURIComponent(barcode)).then(function (results) {

                var skipcurrentbill = false;
                if ($scope.CurrentBill.id && $scope.CurrentBill.id > 0
                    && $scope.CurrentBill.billattributes
                    && $scope.CurrentBill.billattributes != null
                    && $scope.CurrentBill.billattributes.packslipid
                    && $scope.CurrentBill.billattributes.packslipid == results.packingno) {
                    skipcurrentbill = true;
                }

                if (!skipcurrentbill && results.billid > 0 && results.billno != '') {
                    $rootScope.Toaster.Error(barcode + " already billed (" + results.billno + ')');
                    return;
                }

                if (typeof ($scope.CurrentBill.billattributes.packslipid) != 'undefined' &&
                    $scope.CurrentBill.billattributes.packslipid != null &&
                    $scope.CurrentBill.billattributes.packslipid != '' &&
                    $scope.CurrentBill.receivable != 0) {
                    var packslips = $scope.CurrentBill.billattributes.packslipid.split(',');
                    if (packslips.indexOf(results.packingno) < 0) {
                        if ($scope.CurrentBill.Billitems && $scope.CurrentBill.Billitems != null &&
                            $scope.CurrentBill.Billitems.length > 0) {
                            if ($scope.CurrentBill.customerid != results.customerid) {
                                $rootScope.Toaster.Error("Can't merge different customer packing slip to single invoice");
                                return;
                            }
                        }
                        $scope.CurrentBill.billattributes.packslipid += ',' + results.packingno;
                        $scope.CurrentBill.billattributes.packsliplist = results.othersolist;
                    }
                    else if ($scope.CurrentBill.Billitems &&
                        $scope.CurrentBill.Billitems != null &&
                        $scope.CurrentBill.Billitems.length > 0) {

                        var existpackingitem = $scope.CurrentBill.Billitems.find(function (item) {
                            return item.attributes.packingno == results.packingno;
                        });
                        if (existpackingitem && existpackingitem != null) {
                            $rootScope.Toaster.Error(results.packingno + " already imported (" + existpackingitem.barcode + ")");
                            return;
                        }
                    }
                }
                else {
                    $scope.CurrentBill.billattributes.packslipid = results.packingno;
                    $scope.CurrentBill.billattributes.packsliplist = results.othersolist;
                }
                $scope.CurrentBill.billattributes.orderrefid = results.sono;


                if ($scope.CurrentBill.billattributes &&
                    $scope.CurrentBill.billattributes.orderrefid &&
                    $scope.CurrentBill.billattributes.orderrefid != '' &&
                    !$scope.SOManager.Enabled) {
                    $rootScope.Toaster.Error("SO Manager not enabled to link so with billing items");
                }
                if ($scope.SOManager.Enabled &&
                    $scope.CurrentBill.billattributes &&
                    $scope.CurrentBill.billattributes.orderrefid &&
                    $scope.CurrentBill.billattributes.orderrefid != '') {
                    $scope.SOManager.SearchSaleOrder($scope.CurrentBill.billattributes.orderrefid,
                        $scope.CurrentBill.Billitems, function () {

                            $scope.CurrentBill.customerid = results.customerid;
                            source = 'SOPLAN';
                            if ($scope.CurrentBill.customerid > 0) {
                                $scope.CustomerChanged(function () {
                                    $scope.LoadPackingSlip(results);
                                });
                            }
                            else {
                                $scope.LoadPackingSlip(results);
                            }

                        });
                }
                else {
                    $scope.CurrentBill.customerid = results.customerid;
                    source = 'SOPLAN';
                    if ($scope.CurrentBill.customerid > 0) {
                        $scope.CustomerChanged(function () {
                            $scope.LoadPackingSlip(results);
                        });
                    }
                    else {
                        $scope.LoadPackingSlip(results);
                    }
                }
            });
        }

        $scope.LoadPackingSlip = function (results) {

            var barcodes = [];
            $.each(results.barcodes, function (k, v) {
                for (var i = 0; i < v.qty; i++) {
                    barcodes.push(v.barcode);
                }
            });

            var request =
            {
                Item:
                {
                    'customerid': $scope.CurrentBill.customerid,
                    'locationid': $rootScope.LocationInfo.location.id,
                    'shipcustomerid': ($scope.CurrentBill.billattributes && $scope.CurrentBill.billattributes.hasshiptto) ? $scope.CurrentBill.billattributes.shiptoid : 0,
                    'barcodes': barcodes,
                    'qty': $scope.autoCoding.qty,
                    'sono': $scope.CurrentBill.billattributes.orderrefid,
                    'usedstock': $scope.CurrentBillConfig.stockqty(),
                    'source': 'UAN',
                    'ccmarginmrp': $scope.CurrentBill.ccmarginmrp,
                    'ccmarginprice': $scope.CurrentBill.ccmarginprice,
                    'cccomputemode': $scope.CurrentBill.cccomputemode,
                }
            };

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalablegoodsService/SearchBarcode",
                data: srequest
            }).success(function (barcoderesults) {

                $.each(barcoderesults, function (k, v) {
                    v.packingno = results.packingno;
                    var qtybarcode = results.barcodes.find(function (item) {
                        return item.barcode == v.barcode;
                    });

                    if (qtybarcode == undefined || qtybarcode == null) {
                        qtybarcode = results.barcodes.find(function (item) {
                            return item.barcode == v.obarcode;
                        });
                    }

                    if (qtybarcode && qtybarcode != null) {
                        v.soboxid = qtybarcode.soboxid;
                        v.fgboxid = qtybarcode.fgboxid;
                    }
                })

                $scope.AddGroupItem(barcoderesults);
                $scope.autoCoding.code = ''

            }).error(function (result) {
            });
        }

        $scope.SearchMFGBarcode = function (source) {

            if (typeof ($scope.autoCoding) != 'undefined' &&
                typeof ($scope.autoCoding.code) != 'undefined' &&
                $scope.autoCoding.code != null &&
                $scope.autoCoding.code.startsWith('SOPLAN/')) {
                var codes = $scope.autoCoding.code.split('/');
                $scope.autoCoding.code = '';
                $scope.CurrentBill.customerid = parseInt(codes[1]);
                source = 'SOPLAN';
                $scope.CustomerChanged(function () {
                    $scope.SearchMFGBarcode('SOPLAN');
                });
                return;
            }

            var request =
            {
                Item:
                {
                    'customerid': $scope.CurrentBill.customerid,
                    'locationid': $rootScope.LocationInfo.location.id,
                    'shipcustomerid': ($scope.CurrentBill.billattributes && $scope.CurrentBill.billattributes.hasshiptto) ? $scope.CurrentBill.billattributes.shiptoid : 0,
                    'code': $scope.autoCoding.code,
                    'qty': $scope.autoCoding.qty,
                    'sono': $scope.CurrentBill.billattributes.orderrefid,
                    'usedstock': $scope.CurrentBillConfig.stockqty(),
                    'includesold': $scope.IsSaleReturn,
                    'source': typeof (source) != 'undefined' ? source : 'UAN',
                    'ccmarginmrp': $scope.CurrentBill.ccmarginmrp,
                    'ccmarginprice': $scope.CurrentBill.ccmarginprice,
                    'cccomputemode': $scope.CurrentBill.cccomputemode
                }
            };

            if (request.Item.source == 'BILL NO') {
                $scope.SearchCondition.billno = $scope.autoCoding.code;
                $scope.SearchCondition.billid = 0;
                $scope.SearchBill(function () {
                    $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                })
                return;
            }

            if (request.Item.source != 'SOPLAN' &&
                $scope.autoCoding.code.length <= 3 &&
                !isNaN($scope.autoCoding.code)) {
                $scope.autoCoding.qty = $scope.autoCoding.code;
                $scope.autoCoding.code = '';
                return;
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalablegoodsService/SearchBarcode",
                data: srequest
            }).success(function (results) {
                if (typeof (results.length) != 'undefined' && results.length > 0) {
                    if (results.length == 1 && results[0].sellingmode == 3) {

                        if (results[0].stockisbit) {
                            $scope.SalesDetail.qty = results.retailqty;
                            $scope.AddGroupItem(results);
                            $scope.autoCoding.code = '';
                        }
                        else {
                            $rootScope.Prompt('Edit', 'Qty', function (count) {
                                var totalqty = $rootScope.GetNumber(count);
                                results[0].qty = totalqty;

                                $timeout(function () {
                                    $scope.AddGroupItem(results);
                                    $scope.autoCoding.code = '';
                                }, 10);

                            });
                        }

                    }
                    else {
                        $scope.AddGroupItem(results);
                        $scope.autoCoding.code = ''
                    }
                }
                else if (results.id > 0) {
                    $scope.AddGroupItem([results]);
                    $scope.autoCoding.code = ''
                }
            }).error(function (result) {
            });
        }

        $scope.qEncodeURIComponent = function (value) {
            if (value.indexOf('.') >= 0)
                value = value.replaceAll('.', '~dot~');
            if (value.indexOf('/') >= 0)
                value = value.replaceAll('/', '~sls~');
            if (value.indexOf('*') >= 0)
                value = value.replaceAll('*', '~str~');
            return encodeURIComponent(value);
        }

        $scope.SearchBarcode = function () {

            if ($scope.SalesDetail.barcode == null || $scope.SalesDetail.barcode == "") return;

            if ($scope.SearchModeEnabled) {
                $scope.SearchMFGBarcode();
                return;
            }

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                $rootScope.Toaster.Notice('Please select the customer');
                return;
            }

            if (($scope.SalesDetail.barcode == "0")
                && $scope.CurrentBill.Billitems.length > 0) {
                $scope.setFocusSave();
                return;
            }


            if (typeof ($scope.CurrentBill.isbulkreturn) != 'undefined' &&
                $scope.CurrentBill.isbulkreturn) {
                $rootScope.Toaster.Error("Approved Bill can't support additional items");
                return;
            }

            if (typeof ($scope.CurrentBill.isbulkreturngoods) != 'undefined' &&
                $scope.CurrentBill.isbulkreturngoods) {
                $rootScope.Toaster.Error("Approved Bill can't support additional items");
                return;
            }
            for (var index = 0; index < $scope.CurrentBill.Billitems.length; index++) {
                if (($scope.CurrentBill.Billitems[index].qty < 0 && $scope.IsSaleReturn) ||
                    ($scope.CurrentBill.Billitems[index].qty > 0 && !$scope.IsSaleReturn)) {

                    if (!$scope.CurrentBill.Billitems[index].qtyeditable) {
                        $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[index]);
                    }
                    $scope.SalesDetail.barcode = "";
                    return;
                }
            }

            if (!$scope.CheckLocation())
                return;

            $scope.SalesDetail.printingname = '';
            $scope.SalesDetail.productid = 0;
            $scope.SalesDetail.sellingmode = 0;
            $scope.SalesDetail.barcodeDetail = null;
            $scope.SalesDetail.qtyeditable = false;
            $scope.SalesDetail.priceeditable = false;

            if ($scope.system_maxbarcodelength > 0 && $scope.SalesDetail.barcode.length > $scope.system_maxbarcodelength) {
                var barcodeparts = $scope.SalesDetail.barcode.split(/(\d+)/);
                if (barcodeparts[0].length == 1) {
                    $scope.SalesDetail.barcode = $scope.SalesDetail.barcode.substring(0,
                        $scope.system_maxbarcodelength + barcodeparts[0].length);
                }
                else {
                    $scope.SalesDetail.barcode = $scope.SalesDetail.barcode.substring(0,
                        $scope.system_maxbarcodelength);
                }
            }


            var barcode = $scope.SalesDetail.barcode;
            if (barcode == null || barcode == '' || barcode.indexOf(' ') >= 0 ||
                barcode.indexOf('\\') >= 0) {

                if (!$scope.BillNoHandling())
                    $rootScope.Toaster.Error('Unsupported barcode format');
                return;
            }

            restService.Get($scope.SearchAPI + $rootScope.LocationInfo.location.id + '/' + $scope.qEncodeURIComponent($scope.SalesDetail.barcode)).then(function (results) {

                if (results.errormessage != null && results.errormessage != '') {
                    $rootScope.Toaster.Error(results.errormessage);
                    if (results.sellingmode == 3 &&
                        results.errormessage.indexOf('Loaded from bill no') >= 0) {
                        var allparts = results.errormessage.split('#');
                        var customername = allparts[allparts.length - 1].trim();
                    }
                }

                if (results.id != 0) {
                    results.buyingprice = results.buyingprice.round(2);
                    if (typeof (results.imageid) != 'undefined' && results.imageid != null && results.imageid.length > 0) {
                        $scope.currentimageid = results.imageid;
                        $scope.ShowImage = true;
                    }
                    else {
                        $scope.ShowImage = false;
                    }
                    $scope.SalesDetail.stock = $scope.IsSaleReturn ? results.qty : (results.iqty - (results.dqty + results.oqty));
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.SalesDetail.attributes = {
                        'serialno': results.serialno,
                        'designid': results.designid,
                        'mrp': results.mrp,
                        'size': results.sizename,
                        'brand': results.brandname,
                        'typename': results.typename,
                        'colourname': results.colourname,
                        'materialname': results.materialname,
                        'stylename': results.stylename,
                        'patternname': results.patternname
                    };
                    if (!$scope.sale_negativestock && $scope.SalesDetail.stock <= 0) {

                        $.confirm({
                            title: 'Duplicate',
                            content: 'This item already sold, do you want to sell it again?',
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
                                        $scope.SalesDetail.stock = 1;
                                        $scope.AddBarcode(results);


                                        $timeout(function () {
                                            $scope.setFocusSalesman();
                                            $scope.$digest();
                                        }, 10);


                                        return true;
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

                    } else {

                        //search product information
                        //if product.sellingmode != piece > enable qty editable

                        if (!$scope.IsUANBased && $scope.CurrentBill.ccmarginmode) {

                            results.taxInfo = $scope.SalesTax.find(function (arritem) {
                                return arritem.id == results.taxid;
                            });
                            if (results.taxInfo != null) {
                                if (results.taxInfo.taxsplit == null) {
                                    results.taxInfo.taxsplit = { 'taxpercentage': results.taxInfo.taxpercentage };
                                }
                                results.taxInfo.taxsplit.findTax = $scope.findTax;
                            }


                            if ($scope.CurrentBill.ccmarginmrp > 0) {
                                var mrpmargin = (results.barcodeDetail.mrp * $scope.CurrentBill.ccmarginmrp / 100).round($scope.rounding_rate);
                                var mrpcost = results.barcodeDetail.mrp - mrpmargin;
                                if (results.barcodeDetail.taxInfo != null && results.barcodeDetail.taxInfo.taxsplit != null) {

                                    if (results.barcodeDetail.taxInfo.taxsplit.findTax == undefined || results.barcodeDetail.taxInfo.taxsplit.findTax == null)
                                        results.barcodeDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                                    var taxitem = results.barcodeDetail.taxInfo.taxsplit.findTax(results.barcodeDetail.mrp, false);
                                    var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                                    var beforetax = (results.barcodeDetail.mrp / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                                    mrpcost -= beforetax;

                                    //var taxitem = results.barcodeDetail.taxInfo.taxsplit.findTax(mrpcost, true);
                                    //var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                                    //var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                                    //mrpcost += aftertax;
                                }
                                results.barcodeDetail.price = mrpcost;
                                results.barcodeDetail.baseprice = results.barcodeDetail.price;
                            }
                            else if ($scope.CurrentBill.ccmarginprice > 0) {
                                var mrpmargin = (results.barcodeDetail.price * $scope.CurrentBill.ccmarginprice / 100).round($scope.rounding_rate);
                                var mrpcost = results.barcodeDetail.price - mrpmargin;
                                if (results.barcodeDetail.taxInfo != null && results.barcodeDetail.taxInfo.taxsplit != null) {

                                    if (results.barcodeDetail.taxInfo.taxsplit.findTax == undefined || results.barcodeDetail.taxInfo.taxsplit.findTax == null)
                                        results.barcodeDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                                    var taxitem = results.barcodeDetail.taxInfo.taxsplit.findTax(results.barcodeDetail.price, false);
                                    var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                                    var beforetax = (results.barcodeDetail.price / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                                    mrpcost -= beforetax;

                                    //var taxitem = results.barcodeDetail.taxInfo.taxsplit.findTax(mrpcost, true);
                                    //var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                                    //var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                                    //mrpcost += aftertax;
                                }
                                results.barcodeDetail.price = mrpcost;
                                results.barcodeDetail.baseprice = results.barcodeDetail.price;
                            }
                            else {
                                results.barcodeDetail.price = (results.barcodeDetail.buyingprice + (results.barcodeDetail.buyingprice * $scope.CurrentBill.ccmargin / 100)).round($scope.rounding_rate);
                            }

                            //if ($scope.CurrentBill.ccmarginmrp > 0) {
                            //    var mrpmargin = (results.barcodeDetail.mrp * $scope.CurrentBill.ccmarginmrp / 100).round($scope.rounding_rate);
                            //    var mrpcost = results.barcodeDetail.mrp - mrpmargin;
                            //    if (item.taxInfo != null && item.taxInfo.taxsplit != null) {
                            //        var taxitem = results.taxInfo.taxsplit.findTax(results.barcodeDetail.mrp, false);
                            //        var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                            //        var beforetax = (results.barcodeDetail.mrp / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                            //        mrpcost -= beforetax;

                            //        var taxitem = results.taxInfo.taxsplit.findTax(mrpcost, true);
                            //        var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                            //        var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                            //        mrpcost += aftertax;
                            //    }
                            //    results.price = results.barcodeDetail.mrp - mrpdiscount;
                            //    results.baseprice = results.price;
                            //}
                            //else if ($scope.CurrentBill.ccmarginprice > 0) {
                            //    var mrpmargin = (results.barcodeDetail.price * $scope.CurrentBill.ccmarginprice / 100).round($scope.rounding_rate);
                            //    var mrpcost = results.barcodeDetail.price - mrpmargin;
                            //    if (item.taxInfo != null && item.taxInfo.taxsplit != null) {
                            //        var taxitem = results.taxInfo.taxsplit.findTax(results.barcodeDetail.price, false);
                            //        var beforetaxpercentage = taxitem.cgst + taxitem.sgst;
                            //        var beforetax = (results.barcodeDetail.price / (100 + beforetaxpercentage) * beforetaxpercentage).round($scope.rounding_rate)
                            //        mrpcost -= beforetax;

                            //        var taxitem = results.taxInfo.taxsplit.findTax(mrpcost, true);
                            //        var aftertaxpercentage = taxitem.cgst + taxitem.sgst;
                            //        var aftertax = (mrpcost / (100 + aftertaxpercentage) * aftertaxpercentage).round($scope.rounding_rate);
                            //        mrpcost += aftertax;
                            //    }
                            //    results.price = results.barcodeDetail.mrp - mrpdiscount;
                            //    results.baseprice = item.price;
                            //}
                            //else if (results.buyingprice > 0) {
                            //    var newprice = results.buyingprice + (results.buyingprice * $scope.CurrentBill.ccmargin / 100).round(0);
                            //    var newdiscount = (results.price - results.discount + results.additionaldiscount) - newprice;
                            //    results.additionaldiscount += newdiscount;
                            //    results.additionaldiscountpercentage = (results.additionaldiscount / results.price * 100).round($scope.rounding_discount);
                            //    results.attributes.customerdiscount = results.additionaldiscount;
                            //    results.baseprice -= newdiscount;
                            //}
                        }

                        $scope.AddBarcode(results);
                        //results.iqty <= 1 &&
                        if (results.price != 0)
                            $scope.setFocusSalesman();
                    }
                }
            });
        }

        $scope.UpdateTax = function (SalesDetail) {
            if (typeof (SalesDetail.taxInfo) != 'undefined' &&
                SalesDetail.taxInfo != null) {
                SalesDetail.taxpercentage = SalesDetail.taxInfo.taxpercentage;
                SalesDetail.taxname = SalesDetail.taxInfo.name;
                if (typeof (SalesDetail.additionaldiscount) == 'undefined' || isNaN(SalesDetail.additionaldiscount))
                    SalesDetail.additionaldiscount = 0;

                if (typeof (SalesDetail.schemediscount) == 'undefined' || isNaN(SalesDetail.schemediscount))
                    SalesDetail.schemediscount = 0;

                if ($scope.system_b2bexclusivetax) {
                    if (typeof (SalesDetail.etdiscount) == 'undefined' || isNaN(SalesDetail.etdiscount))
                        SalesDetail.etdiscount = 0;

                    if (typeof (SalesDetail.etprice) == 'undefined' || isNaN(SalesDetail.etprice)) {
                        if ($scope.system_retailpriceasb2bexclusive) {
                            SalesDetail.etprice = SalesDetail.price;
                            if (SalesDetail.taxInfo != null && SalesDetail.taxInfo.taxsplit != null) {

                                if (SalesDetail.taxInfo.taxsplit.findTax == undefined || SalesDetail.taxInfo.taxsplit.findTax == null)
                                    SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                                var taxitem = SalesDetail.taxInfo.taxsplit.findTax(SalesDetail.price, true);
                                SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                            }
                            SalesDetail.price = (SalesDetail.etprice + ((SalesDetail.price / (100 + SalesDetail.taxpercentage)) * 100)).round(2);
                        }
                        else {
                            if (SalesDetail.taxInfo != null && SalesDetail.taxInfo.taxsplit != null) {
                                if (SalesDetail.taxInfo.taxsplit.findTax == undefined || SalesDetail.taxInfo.taxsplit.findTax == null)
                                    SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                                var taxitem = SalesDetail.taxInfo.taxsplit.findTax(SalesDetail.price);
                                SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                            }
                            SalesDetail.etprice = ((SalesDetail.price / (100 + SalesDetail.taxpercentage)) * 100).round(2);
                        }
                    }

                    var baseprice = ($scope.GetNumber(SalesDetail.etprice) - $scope.GetNumber(SalesDetail.etdiscount)).round($scope.rounding_discount);
                    if (isNaN(baseprice))
                        baseprice = SalesDetail.etprice;

                    if (SalesDetail.taxInfo != null && SalesDetail.taxInfo.taxsplit != null) {
                        if (SalesDetail.taxInfo.taxsplit.findTax == undefined || SalesDetail.taxInfo.taxsplit.findTax == null)
                            SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                        var taxitem = SalesDetail.taxInfo.taxsplit.findTax(baseprice, true);
                        SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                    }

                    if (!$scope.CurrentBill.isinterstatesale) {
                        var fullTax = (baseprice * SalesDetail.taxpercentage / 100);
                        var halfTax = (fullTax / 2).round($scope.rounding_tax);
                        SalesDetail.itemtax = (halfTax * 2).round($scope.rounding_tax);
                        SalesDetail.tax = (SalesDetail.qty * SalesDetail.itemtax).round($scope.rounding_tax);
                    }
                    else {
                        SalesDetail.itemtax = (baseprice * SalesDetail.taxpercentage / 100).round($scope.rounding_tax)
                        SalesDetail.tax = (SalesDetail.qty * baseprice * SalesDetail.taxpercentage / 100).round($scope.rounding_tax);
                    }

                    SalesDetail.etdiscountpercentage = (($scope.GetNumber(SalesDetail.etdiscount) / $scope.GetNumber(SalesDetail.etprice)) * 100).round($scope.rounding_discount);
                    if (isNaN(SalesDetail.etdiscountpercentage)) SalesDetail.etdiscountpercentage = 0;

                    if (!($scope.system_supportmanualprice && $scope.IsSaleReturn)) {
                        SalesDetail.price = ($scope.GetNumber(SalesDetail.etprice) + SalesDetail.itemtax).round($scope.rounding_tax);
                        SalesDetail.baseprice = SalesDetail.price - SalesDetail.etdiscount;
                        if ($scope.Billingmode != 2)
                            SalesDetail.additionaldiscount = SalesDetail.etdiscount;
                        if (isNaN(SalesDetail.additionaldiscount)) SalesDetail.additionaldiscount = 0;
                        if (isNaN(SalesDetail.price)) SalesDetail.etprice = 0;
                    }
                }
                else {
                    var baseprice = (SalesDetail.price - SalesDetail.additionaldiscount - (SalesDetail.schemediscount / SalesDetail.qty).round($scope.rounding_discount));
                    if (isNaN(baseprice))
                        baseprice = SalesDetail.price - SalesDetail.additionaldiscount;
                    if (isNaN(baseprice))
                        baseprice = SalesDetail.price;

                    if (SalesDetail.taxInfo &&
                        SalesDetail.taxInfo != null &&
                        SalesDetail.taxInfo.taxsplit &&
                        SalesDetail.taxInfo.taxsplit != null) {

                        if (SalesDetail.taxInfo.taxsplit.findTax == undefined)
                            SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                        var taxitem = SalesDetail.taxInfo.taxsplit.findTax(baseprice);
                        SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                    }

                    var basePrice = (baseprice / (100 + (SalesDetail.taxpercentage)) * 100);
                    if (!$scope.CurrentBill.isinterstatesale) {
                        var halfTax = (basePrice * (SalesDetail.taxpercentage / 2) / 100).round($scope.rounding_tax);
                        SalesDetail.itemtax = (halfTax * 2).round($scope.rounding_tax);
                        SalesDetail.tax = (SalesDetail.qty * SalesDetail.itemtax).round($scope.rounding_tax);
                    }
                    else {
                        SalesDetail.itemtax = (baseprice / (100 + SalesDetail.taxpercentage) * SalesDetail.taxpercentage).round($scope.rounding_tax)
                        SalesDetail.tax = (SalesDetail.qty * baseprice / (100 + SalesDetail.taxpercentage) * SalesDetail.taxpercentage).round($scope.rounding_tax);
                    }
                    SalesDetail.etprice = basePrice.round($scope.rounding_rate);
                }

            }
        };

        $scope.findTax = function (amount, incost) {

            if (amount < 0) amount = amount * -1;

            if (typeof (incost) == 'undefined') incost = false;
            if (typeof (items) == 'undefined' && typeof (this.taxpercentage) != 'undefined')
                return { 'cgst': this.taxpercentage / 2, 'sgst': this.taxpercentage / 2 };

            if (this.items == null || this.items.length == 0)
                return null;

            if (this.items.length == 1)
                return this.items[0];

            if (isNaN(amount)) amount = 0;
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (incost == item.incost && item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            return { 'cgst': 0, 'sgst': 0 };
        }

        $scope.AddBarcode = function (results, autofocus) {

            if (results.serialno && results.serialno != null && results.serialno != '') {
                $scope.SalesDetail.displaycode = results.serialno;
            }
            else {
                $scope.SalesDetail.displaycode = results.barcode;
            }
            $scope.SalesDetail.barcode = results.barcode;
            $scope.SalesDetail.imageid = results.imageid;
            $scope.SalesDetail.printingname = results.sellingname;
            $scope.SalesDetail.productid = results.productid;
            $scope.SalesDetail.sellingmode = results.sellingmode;
            $scope.SalesDetail.price = results.price - results.discount;
            $scope.SalesDetail.discountpercentage = (results.discount / results.price * 100).round(0);
            if (isNaN($scope.SalesDetail.discountpercentage)) $scope.SalesDetail.discountpercentage = 0;
            $scope.SalesDetail.schemediscountpercentage = 0;
            if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                $scope.SalesDetail.additionaldiscount = 0;

            if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount)) {
                $scope.SalesDetail.schemediscount = 0;
                $scope.SalesDetail.additionaldiscountpercentage = 0;
            }

            $scope.SalesDetail.locationid = $rootScope.LocationInfo.location.id;
            if (typeof (results.isuploadqty) == 'undefined' || results.isuploadqty) {
                $scope.SalesDetail.qty = results.qty;
            } else if (typeof (results.isexchange) == 'undefined' || !results.isexchange) {

                if (!$scope.SearchModeEnabled) {
                    $scope.SalesDetail.qty = $scope.SalesDetail.stock > 0 || !autofocus ? 1 : 0;
                }

                if (results.stockisbit) {
                    $scope.SalesDetail.qty = results.stockqty;
                }

                if ($scope.IsSaleReturn) {
                    $scope.SalesDetail.qty = $scope.SalesDetail.qty * 1;
                }
            }

            if ($scope.SalesDetail.qty == 0)
                $scope.SalesDetail.qty = 1;
            $scope.SalesDetail.baseprice = ($scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount - ($scope.SalesDetail.schemediscount / $scope.SalesDetail.qty).round($scope.rounding_discount));
            $scope.SalesDetail.companyid = results.companyid;
            $scope.SalesDetail.discount = results.discount;
            $scope.SalesDetail.tax = 0;
            $scope.SalesDetail.taxpercentage = 0;
            $scope.SalesDetail.taxid = results.taxid;
            $scope.SalesDetail.receivable = $scope.SalesDetail.baseprice * $scope.SalesDetail.qty;
            $scope.SalesDetail.salerate = results.price;
            if (typeof ($scope.SalesDetail.cost) == 'undefined') {
                $scope.SalesDetail.cost = results.buyingprice;
                $scope.SalesDetail.margin = (($scope.SalesDetail.baseprice / results.buyingprice * 100) - 100).round(2);
            }

            if (results.taxid > 0) {

                if (typeof ($scope.SalesDetail.taxInfo) == 'undefined' || $scope.SalesDetail.taxInfo == null)
                    $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                        return arritem.id == $scope.SalesDetail.taxid;
                    });

                if ($scope.SalesDetail.taxInfo != null) {
                    if (typeof ($scope.SalesDetail.taxInfo.taxsplit) == 'undefined' ||
                        $scope.SalesDetail.taxInfo.taxsplit == null) {
                        $scope.SalesDetail.taxInfo.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                    }
                }

                if (typeof ($scope.SalesDetail.taxInfo.taxsplit.findTax) == 'undefined' ||
                    $scope.SalesDetail.taxInfo.taxsplit.findTax == null)
                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                if (typeof (results.isexchange) != 'undefined' && results.isexchange) {
                    $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                        return arritem.taxpercentage == 0;
                    });
                }
                else {
                    $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                        return arritem.id == results.taxid;
                    });
                }


                results.taxname = $scope.SalesDetail.taxname;
                $scope.UpdateTax($scope.SalesDetail);
            }



            $scope.ProductDetails = [];

            $.each($scope.Products, function (k, v) {
                if (v.id == results.productid)
                    $scope.ProductDetails.push(v);
            });


            if (!results.stockisbit && $scope.ProductDetails.length > 0
                && $scope.ProductDetails[0].sellingmode > 1 && $scope.SalesDetail.stock > 0) {
                $scope.SalesDetail.qtyeditable = true;
                $scope.SalesDetail.sellingmode = $scope.ProductDetails[0].sellingmode;
                if (autofocus) $scope.setFocusQty();
            }

            if (results.price == 0) {
                $scope.SalesDetail.priceeditable = true;
                if (autofocus) $scope.setFocusPrice();
            }

            $scope.SalesDetail.barcodeDetail = results;
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.EditBill(item);
            }
        };

        $scope.RequestDiscount = function () {
            $.confirm({
                title: 'Reason',
                content: '' +
                    '<form action="" class="formName">' +
                    '<div class="form-group">' +
                    '<input type="text" placeholder="reason" class="name form-control" required />' +
                    '</div>' +
                    '</form>',
                buttons: {
                    formSubmit: {
                        text: 'OK',
                        btnClass: 'btn-blue',
                        action: function () {
                            var name = this.$content.find('.name').val();
                            if (!name) {
                                $.alert('provide a valid name');
                                return false;
                            }
                            $scope.CurrentBill.discountrequestreason = name;
                            $scope.Save(true, false);
                            return true;
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
        };

        $scope.RejectDiscount = function (item) {
            $.confirm({
                title: 'Reason for rejection',
                content: '' +
                    '<form action="" class="formName">' +
                    '<div class="form-group">' +
                    '<input type="text" placeholder="reason" class="name form-control" required />' +
                    '</div>' +
                    '</form>',
                buttons: {
                    formSubmit: {
                        text: 'Reject',
                        btnClass: 'btn-blue',
                        action: function () {
                            var name = this.$content.find('.name').val();
                            if (!name) {
                                return false;
                            }
                            $scope.ChangeApproveStatus(item, 'R', name);
                            return true;
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
        };

        $scope.Cancel = function (item) {

            if (item.settled) {
                $rootScope.Toaster.Notice("Settled bill can't cancellation, Bill No - " + item.billno);
                return;
            }
            request = {
                "item": {
                    'ids': [item.id], 'locationid': $rootScope.LocationInfo.location.id,
                    'isbulkreturn': item.isbulkreturn,
                    'isbulksale': $scope.IsBulkSale
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Cancellation",
                data: srequest
            }).success(function (result) {
                $scope.CreateNew();
                $scope.lastBills = [];
                $rootScope.Toaster.Notice('Cancellation completed, Bill No - ' + result[0].billno);
            }).error(function (result) {
            });
        };

        $scope.PrintPDF = function (item, pendingbulkbill) {
            var custom = ($scope.Billingmode != 0 && $scope.DefaultPrintingTemplateID > 0) ? ('/' + $scope.DefaultPrintingTemplateID) : '/false';
            if (pendingbulkbill) {
                $rootScope.Download("BillService/PrintBulkPendingPDF/" + $scope.UserInfo.OrganizationID + '/' + item.id + '/false', 'Invoice No -' + item.billno + '.pdf', 'application/pdf');
            }
            else {
                $rootScope.Download("BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.id +
                    (($scope.IsBulkSale || item.isbulkreturn) ? "/true" : "/false", custom),
                    'Invoice No -' + item.billno + '.pdf', 'application/pdf');
            }
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "bill_" + item.id + ".pdf";
                if (pendingbulkbill) {
                    link.href = "BillService/PrintBulkPendingPDF/" + $scope.UserInfo.OrganizationID + '/' + item.id;
                }
                else {
                    link.href = "BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.id + (($scope.IsBulkSale || item.isbulkreturn) ? "/true" : "/false");
                }
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.DownloadExcel = function (item) {

            $rootScope.Download("BillService/DownloadExcel/" + $scope.UserInfo.OrganizationID + '/' + item.id + ($scope.IsBulkSale ? "/true" : "/false"), "Invoice - " + item.billno + ".xlsx", 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "bill_" + item.id + ".xlsx";
                link.href = "BillService/DownloadExcel/" + $scope.UserInfo.OrganizationID + '/' + item.id + ($scope.IsBulkSale ? "/true" : "/false");
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.Print = function (item) {
            try {
                if (typeof (item.printdocument) != 'undefined' &&
                    item.printdocument != null && item.printdocument.length > 0) {
                    if (typeof (item.directpdf) != 'undefined' && item.directpdf) {
                        $scope.Sent2PrinterPDF(item.printdocument);
                    }
                    else {
                        $scope.Sent2Printer(item.printdocument);
                    }
                    return;
                }

                var request = {
                    "Item": {
                        'id': typeof (item.billid) != 'undefined' ? item.billid : item.id,
                        'enableprintdocument': true,
                        'billtype': item.billtype, 'isbulkreturn': item.billtype == 10
                    },
                    "SecurityToken": "String content"
                }

                request.Item.papersize = $scope.PrinterData.PaperSize;
                request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
                request.Item.printtemplateid = $scope.DefaultPrintingTemplateID;
                reqyest.Item.frompos = true;
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BillService/Search",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length == 1) {
                        if (typeof (result[0].directpdf) != 'undefined' && result[0].directpdf) {
                            $scope.Sent2PrinterPDF(result[0].printdocument);
                        }
                        else {
                            $scope.Sent2Printer(result[0].printdocument);
                        }
                    }
                });
            } catch (e) {
                e = null;
            }
        };

        $scope.Sent2PrinterPDF = function (printdocument) {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            $rootScope.Sent2Printer(printdocument, printerData, 'PDF');
            //var request = {
            //    'id': printerData.printerid,
            //    'content': printdocument,
            //    'encoded': 'BASE64'
            //};

            //$.ajax({
            //    type: "POST",
            //    url: printerData.service + 'PDF',
            //    data: JSON.stringify(request),
            //    contentType: 'text/plain'
            //});
        }

        $scope.Sent2PrinterRAW = function (printdocument) {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                return;
            }
            $rootScope.Sent2Printer(printdocument, printerData, 'RAW');
            //var request = {
            //    'id': printerData.printerid,
            //    'content': printdocument,
            //    'encoded': 'BASE64'
            //};

            //$.ajax({
            //    type: "POST",
            //    url: printerData.service + 'RAW',
            //    data: JSON.stringify(request),
            //    contentType: 'text/plain'
            //});
        }


        $scope.Sent2Printer = function (printdocument) {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                $rootScope.Toaster.Notice("Please select the printer..");
                return;
            }
            $rootScope.Sent2Printer(printdocument, printerData);

            //var printerData = localStorageService.get('PrinterData');
            //if (printerData == null) {
            //    return;
            //}
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

        $scope.EditBill = function (item) {
            $scope.SearchCondition.billno = item.billno;
            $scope.SearchCondition.billid = item.id;
            $scope.SearchBill();
        }

        $scope.SearchBill = function (callback) {
            request = {
                "Item": {
                    'billno': $scope.SearchCondition.billno,
                    'billid': $scope.SearchCondition.billid,
                    'isbulkreturn': $scope.SearchCondition.isbulkreturn,
                    'locationid': $rootScope.LocationInfo.location.id,
                    'isb2b': true,
                    'removecancelled': true,
                    'frompos': true
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);
            var somanager = this.SOManager;
            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {

                if (result.length == 0) {
                    if (typeof (callback) != 'undefined')
                        callback(null);
                }
                else {
                    var bill = result[0];
                    //if (bill.additionalcharges && bill.additionalcharges != 0) {
                    //    bill.receivable -= bill.additionalcharges;
                    //    bill.total -= bill.additionalcharges;
                    //}

                    bill.isb2b = true;
                    if (bill.billtype == 10) {
                        bill.billtype = 1;
                        bill.iscreditbill = true;
                    }
                    if ($scope.IsSaleReturn && bill.billtype == 9)
                        return;

                    if ($scope.IsSaleReturn && bill.billtype <= 1) {
                        if (bill.customer != null && bill.customer.srmaxdays > 0) {
                            var billdate = new Date(bill.billdate);
                            var maxday = new Date(billdate.setDate(billdate.getDate() + bill.customer.srmaxdays));
                            if (maxday < new Date()) {
                                bill.srmaxdays = bill.customer.srmaxdays;
                                if (bill.billattributes == null) bill.billattributes = {};
                                bill.billattributes.printrequiredapproval = 'Return policy crossed - ' + $filter('date')(maxday, 'dd-MM-yyyy');
                                $rootScope.Toaster.Error('Return policy crossed - ' + $filter('date')(maxday, 'dd-MM-yyyy'));
                            }
                        }
                    }

                    $.each(bill.Billitems, function (k, v) {
                        if (v.attributes != null && v.attributes.serialno != null && v.attributes.serialno != '')
                            v.displaycode = v.attributes.serialno;
                        else
                            v.displaycode = v.barcode;

                        if (v.attributes != null && v.attributes.cost != null && v.attributes.cost != '')
                            v.cost = v.attributes.cost;

                        if (bill.billtype == 20) {
                            v.etprice = v.price;
                            v.etdiscount = v.discount;
                            if (v.taxpercentage > 0) {
                                v.etprice = (v.price / (v.taxpercentage + 100)) * 100;
                                if (v.discount > 0)
                                    v.etdiscount = (v.discount / (v.taxpercentage + 100)) * 100;
                            }
                        }
                    });


                    if (somanager.Enabled &&
                        bill.billattributes &&
                        bill.billattributes.orderrefid &&
                        bill.billattributes.orderrefid != '') {
                        somanager.SearchSaleOrder(bill.billattributes.orderrefid,
                            bill.Billitems, function () {
                                if ($scope.CurrentBillConfig.type != 'list') {
                                    $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                }
                            });
                    }

                    if (typeof (callback) != 'undefined')
                        callback(bill);

                    if (!bill.settled && bill.billtype == 8 && !$scope.IsSaleReturn) {
                        var billitems = (JSON.parse(JSON.stringify(bill.Billitems)));
                        $.each(billitems, function (k, v) {
                            var existingitem = $scope.CurrentBill.Billitems.find(function (item) {
                                return typeof (item.returnbillitemid) != 'undefined' && item.returnbillitemid == v.id;
                            });
                            if (typeof (existingitem) == 'undefined' || existingitem == null) {
                                v.returnbillitemid = v.id;
                                v.id = 0;
                                $scope.CurrentBill.Billitems.push(v);
                                $scope.CalcualteTotal(v);
                            }
                        });
                        $scope.CurrentBill.returnbillid = bill.id;
                    }
                    else {
                        $scope.CurrentBill = bill;
                        if ($scope.CurrentBill.additionalchargeslist == null)
                            $scope.CurrentBill.additionalchargeslist = [];
                        $scope.agrid.UpdateData($scope.CurrentBill.additionalchargeslist);
                        $scope.additionaldiscountamountrequested = bill.additionaldiscount;
                        $scope.CurrentBill.Billitems = bill.Billitems;
                        $scope.CurrentBill.billTax = bill.BillTax;
                        $scope.CurrentBill.billdatedisplay = $filter('date')($scope.CurrentBill.billdate, 'dd-MM-yyyy');
                        $scope.CurrentBill.billtimedisplay = $filter('date')($scope.CurrentBill.billdate, 'hh:mm a');
                        //if ($scope.IsSaleReturn) {
                        //    $scope.CurrentBill.schemediscount = 0;
                        //    $.each(bill.Billitems, function (k, v) {
                        //        v.discount = (v.discount - v.schemediscount).round(2);
                        //        v.receivable = (v.receivable - v.schemediscount).round(2);
                        //        $scope.CurrentBill.total -= v.schemediscount;
                        //        $scope.CurrentBill.receivable -= v.schemediscount;
                        //        $scope.CurrentBill.discount -= v.schemediscount;
                        //        $scope.CurrentBill.schemediscount += v.schemediscount;
                        //    });
                        //}
                        $.each(bill.BillTax, function (k, v) {
                            var tax = $scope.SalesTax.find(function (item) {
                                return item.id == v.taxid;
                            });
                            v.taxName = tax != null ? tax.name : '';
                        });

                        if (typeof ($scope.CurrentBill.additionalcharges) != 'undefined' && $scope.GetNumber($scope.CurrentBill.additionalcharges) != 0) {

                            if ($scope.CurrentBill.billtype != 8)
                                $scope.CurrentBill.total -= $scope.GetNumber($scope.CurrentBill.additionalcharges);
                        }

                        if (bill.customer != null && bill.customerid > 0) {
                            if ($scope.SOManager && $scope.SOManager != null)
                                $scope.SOManager.CustomerID = bill.customerid;
                            $scope.CustomerChanged();
                        }

                    }

                    if ($scope.IsSaleReturn && $scope.CurrentBill.billtype != 8) {
                        $scope.ReverseBillValue();
                        $scope.CurrentBill.returnbillno = $scope.CurrentBill.billno;
                        $scope.CurrentBill.returnbillid = $scope.CurrentBill.id;
                        $scope.CurrentBill.billno = '';
                        $scope.CurrentBill.id = 0;
                        $scope.autoCoding.code = '';
                        $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
                    }
                    $scope.CheckPOSLayout(false);
                    $scope.CurrentBillConfig.internallist = $scope.CurrentBill.Billitems;
                    $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                    $scope.ComputeAddtionalChargeValues();
                }
            }).error(function (result) {
            });
        };

        $scope.CheckPOSLayout = function (isnew) {
            if (!$scope.system_sizegrouplayout) return;

            if (!isnew &&
                $scope.CurrentBill.billattributes &&
                $scope.CurrentBill.billattributes.sizegroupid &&
                $scope.CurrentBill.billattributes.sizegroupid != $scope.TemplateCurrentBill.billattributes.sizegroupid) {
                var sizegroup = $scope.SizeGroupList.find(function (item) {
                    return item.id == $scope.CurrentBill.billattributes.sizegroupid;
                })
                $scope.system_possizelist = sizegroup.possizelist;
                $scope.GridConfig();
            }

            if (isnew) {
                if ($scope.TemplateCurrentBill.billattributes.sizegroupid &&
                    $scope.TemplateCurrentBill.billattributes.sizegroupid > 0) {
                    var sizegroup = $scope.SizeGroupList.find(function (item) {
                        return item.id == $scope.TemplateCurrentBill.billattributes.sizegroupid;
                    })
                    if (sizegroup && $scope.system_possizelist != sizegroup.possizelist) {
                        $scope.system_possizelist = sizegroup.possizelist;
                        $scope.GridConfig();
                    }
                }
                else if ($scope.system_possizelist != $scope.ApplicationSettings.appsettings.system_possizelist) {
                    $scope.system_possizelist = $scope.ApplicationSettings.appsettings.system_possizelist;
                    $scope.GridConfig();
                }
            }
        }

        $scope.ReverseBillValue = function () {

            $scope.BeginGridTransaction = true;
            $scope.CurrentBill.totalqty = 0;
            $scope.CurrentBill.totalpiece = 0;
            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;
            $scope.CurrentBill.billtype = 9;
            $scope.SchemeReturnBillItems = [];
            for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {

                //var addist = $scope.CurrentBill.Billitems[i].qty * ($scope.CurrentBill.Billitems[i].additionaldiscount + $scope.CurrentBill.Billitems[i].schemediscount);
                //alert(addist);

                if ($scope.CurrentBill.Billitems[i].schemediscount > 0) {
                    $scope.CurrentBill.Billitems[i].schemediscountpercentage = (($scope.CurrentBill.Billitems[i].schemediscount / $scope.CurrentBill.Billitems[i].qty) / $scope.CurrentBill.Billitems[i].price * 100).round(0);
                }

                if ($scope.CurrentBill.Billitems[i].schemeid > 0)
                    $scope.CurrentBill.Billitems[i].style = { "background-color": "lightgrey" };

                $scope.CurrentBill.Billitems[i].qty = Math.abs($scope.CurrentBill.Billitems[i].qty) * -1;
                $scope.CurrentBill.Billitems[i].piece = Math.abs($scope.CurrentBill.Billitems[i].piece) * -1;
                $scope.CurrentBill.Billitems[i].tax = Math.abs($scope.CurrentBill.Billitems[i].tax) * -1;
                $scope.CurrentBill.Billitems[i].discount = Math.abs($scope.CurrentBill.Billitems[i].discount) * -1;
                $scope.CurrentBill.Billitems[i].receivable = Math.abs($scope.CurrentBill.Billitems[i].receivable) * -1;
                $scope.CurrentBill.Billitems[i].additionaldiscount = ($scope.CurrentBill.Billitems[i].additionaldiscount);
                $scope.CurrentBill.Billitems[i].schemediscount = Math.abs($scope.CurrentBill.Billitems[i].schemediscount) * -1;
                $scope.CurrentBill.Billitems[i].saleamount = $scope.CurrentBill.Billitems[i].qty * $scope.CurrentBill.Billitems[i].rate;

                //var addist = $scope.CurrentBill.Billitems[i].qty * ($scope.CurrentBill.Billitems[i].additionaldiscount + $scope.CurrentBill.Billitems[i].schemediscount);
                //alert(addist);
                $scope.CalcualteTotal($scope.CurrentBill.Billitems[i]);

                if ($scope.CurrentBill.Billitems[i].schemeid > 0)
                    $scope.SchemeReturnBillItems.push($scope.CurrentBill.Billitems[i]);
            }

            if ($scope.CurrentBill.additionalchargeslist &&
                $scope.CurrentBill.additionalchargeslist != null &&
                $scope.CurrentBill.additionalchargeslist.length > 0) {
                $.each($scope.CurrentBill.additionalchargeslist, function (k, v) {
                    v.Charge = Math.abs(v.Charge) * -1;
                    v.Amount = Math.abs(v.Amount) * -1;
                    v.Tax = Math.abs(v.Tax) * -1;
                })
            }

            $scope.BeginGridTransaction = false;
            //$scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
            if ($scope.SchemeReturnBillItems.length > 0) {
                $rootScope.Toaster.Notice('Return bill contains offer items, final return value may differ');
            }
        }

        $scope.Search = function () {
            $scope.SearchCondition.removecancelled = true;
            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }
            request.Item.papersize = $scope.PrinterData.PaperSize;
            request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
            request.Item.frompos = true;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
            }).error(function (result) {
            });
        };


        $scope.ChangeApproveStatus = function (item, status, reason) {
            var request = null;
            if (status == 'A') {
                request = {
                    "Item": { 'id': item.id, 'status': 3 },
                    "SecurityToken": "String content"
                }
            } else if (status == 'R') {
                request = {
                    "Item": { 'id': item.id, 'status': 2, 'actionreason': reason },
                    "SecurityToken": "String content"
                }
            } else if (status == 'X') {
                request = {
                    "Item": { 'id': item.id, 'hasaccepted': true },
                    "SecurityToken": "String content"
                }
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountqueueService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
            }).error(function (result) {
            });
        }

        $scope.AddBillHistory = function (result, isdiscount, isupdate) {
            if (typeof ($scope.lastBills) == 'undefined' || $scope.lastBills == null)
                $scope.lastBills = [];
            if (isdiscount) {
                $.each(result, function (k, v) {
                    var existbill = $scope.lastBills.find(function (item) {
                        return item.type == 'B' && item.id == v.billid;
                    });
                    if (existbill != null && existbill.id > 0) {
                        existbill.discountstatus = v.discountstatus;
                    }
                    else {
                        var existbill = $scope.lastBills.find(function (item) {
                            return item.type == 'D' && item.id == v.id;
                        });
                        if (existbill != null && existbill.id > 0) {
                            existbill.discountstatus = v.status;
                        }
                        else {
                            v.type = 'D';
                            v.discountstatus = v.status;
                            $scope.lastBills.push(v);
                        }
                    }
                })
            }
            else {

                $scope.lastBills = [];
                var listofnormalbill = [];
                if (typeof ($scope.lastBills) == 'undefined' || $scope.lastBills == null)
                    $scope.lastBills = [];

                $.each($scope.lastBills, function (k, v) {
                    if (v.type == 'B' && v.discountstatus == 0)
                        listofnormalbill.push(v);
                    if (v.type == 'D' && isupdate && result.id == v.billid)
                        listofnormalbill.push(v);
                });


                $.each(listofnormalbill, function (k, v) {
                    $scope.lastBills.splice($scope.lastBills.indexOf(v), 1);
                });

                if (typeof (result.Bills) != 'undefined') {
                    $.each(result.Bills, function (k, v) {
                        v.type = 'B';
                        $scope.lastBills.push(v);
                    });
                }
                else {
                    result.type = 'B';
                    $scope.lastBills.push(result);
                }
            }
        }

        $scope.discounttimerenabled = false;
        $scope.CheckDiscountRequest = function () {
            var request = {
                "Item": {
                    'requestedby': 1,
                    'mode': $scope.CanApprove ? 2 : 1
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountqueueService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    request.Item.fromid = result[result.length - 1].id;
                    $scope.AddBillHistory(result, true, false);
                    if ($scope.DiscountQueue) {
                        $scope.discounttimerenabled = true;
                        $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                    }
                }
            });
        }

        if ($scope.DiscountQueue) {
            $scope.CheckDiscountRequest();
        }

        $scope.PullDiscountRequest = function (request) {
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountqueueService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    request.Item.fromid = result[result.length - 1].id;
                    $scope.AddBillHistory(result, true, false);
                }
            });
        }

        $scope.CanApprove = true;
        $scope.AllowAdditionalDiscount = false;
        $scope.ShowDiscountApprove = false;
        $scope.ShowDiscountQueue = function (enable) {

        };

        //$scope.ApplyDiscount = function (ispercentage) {
        //    if (ispercentage) {
        //        var percentage = parseFloat($scope.additionaldiscountpercentrequested);
        //        if (percentage < 0 || percentage > 100)
        //            percentage = 0;
        //        discountamount = 0;
        //        var tax = 0;
        //        $.each($scope.CurrentBill.Billitems, function (k, v) {

        //            var existingTaxValue = $scope.CurrentBill.billTax.find(function (item) {
        //                return item.taxid == v.taxid;
        //            });

        //            existingTaxValue.amount -= ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax -= (v.tax);

        //            v.additionaldiscount = Math.round(v.price * percentage / 100, 0);
        //            v.additionaldiscountpercentage = percentage;
        //            discountamount += v.additionaldiscount;
        //            $scope.UpdateTax(v);
        //            tax += v.tax;

        //            existingTaxValue.amount += ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax += (v.tax);

        //        });
        //        $scope.CurrentBill.additionaldiscount = discountamount;
        //        $scope.additionaldiscountamountrequested = discountamount;
        //        $scope.CurrentBill.tax = tax;
        //    }
        //    else {
        //        var discountamount = parseFloat($scope.additionaldiscountamountrequested);
        //        if (discountamount < 0)
        //            discountamount = 0;
        //        $scope.CurrentBill.additionaldiscount = discountamount;
        //        percentage = ((discountamount / $scope.CurrentBill.receivable) * 100);
        //        var maxitem = null;
        //        var tax = 0;
        //        $.each($scope.CurrentBill.Billitems, function (k, v) {
        //            if (maxitem == null || maxitem.price < v.price)
        //                maxitem = v;

        //            var existingTaxValue = $scope.CurrentBill.billTax.find(function (item) {
        //                return item.taxid == v.taxid;
        //            });

        //            existingTaxValue.amount -= ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax -= (v.tax);

        //            v.additionaldiscount = Math.round(v.price * percentage / 100, 0);
        //            v.additionaldiscountpercentage = percentage;
        //            discountamount -= v.additionaldiscount;
        //            $scope.UpdateTax(v);

        //            existingTaxValue.amount += ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax += (v.tax);
        //            tax += v.tax;
        //        });

        //        if (discountamount < 0)
        //            maxitem.additionaldiscount -= discountamount;
        //        else
        //            maxitem.additionaldiscount += discountamount;
        //        //maxitem.receivable = (maxitem.price - maxitem.additionaldiscount) * maxitem.qty;
        //        $scope.CurrentBill.tax = tax;
        //        $scope.additionaldiscountpercentrequested = (percentage).round(2);
        //    }
        //}

        $scope.autoCoding = { 'code': '' };

        $scope.ValidateReturnAndSave = function () {
            if (($scope.CurrentBill.billtype == 8 || $scope.CurrentBill.billtype == 9) &&
                $scope.CurrentBill.receivable < 0) {
                $scope.Save(false, true);
            }
        }

        $scope.ShowCancellDialog = function (bill) {
            $rootScope.Confirm('Delete Bulk Bill', 'Do you want to cancel the bulk bill - ' + bill.billno, function () {
                bill.isbulkreturn = true;
                var request = {
                    "Item": bill
                }
                $http({
                    method: 'POST',
                    url: "BillService/Delete",
                    data: JSON.stringify(request)
                }).success(function (result) {
                    $scope.CreateNew();
                });
            });
        };


        $scope.HasSuggested = function () {
            if ($scope.suggest_selected != null && $scope.autoCoding.code == $scope.suggest_selected.name) {
                switch ($scope.suggest_selected.type) {
                    case 'AREA':
                        $scope.CurrentBill.areaid = $scope.suggest_selected.id;
                        $scope.CurrentBill.areaname = $scope.suggest_selected.name;
                        break;
                    case 'BULKSALEREASON':
                        $scope.CurrentBill.salereasonid = $scope.suggest_selected.id;
                        $scope.CurrentBill.salereason = $scope.suggest_selected.name;
                        break;
                    case 'SEASONS':
                        $scope.CurrentBill.seasonid = $scope.suggest_selected.id;
                        $scope.CurrentBill.season = $scope.suggest_selected.name;
                        break;
                }
                $scope.CurrentBill.displaykey = '';
                if (typeof ($scope.CurrentBill.areaname) != 'undefined')
                    $scope.CurrentBill.displaykey += $scope.CurrentBill.areaname;
                if (typeof ($scope.CurrentBill.salereason) != 'undefined')
                    $scope.CurrentBill.displaykey += ($scope.CurrentBill.displaykey.length > 0 ? ',' : '') + $scope.CurrentBill.salereason;
                if (typeof ($scope.CurrentBill.season) != 'undefined')
                    $scope.CurrentBill.displaykey += ($scope.CurrentBill.displaykey.length > 0 ? ',' : '') + $scope.CurrentBill.season;
                return true;
            }
            return false;
        }

        $scope.SearchAutoCoding = function () {

            if ($scope.autoCoding.code == null || $scope.autoCoding.code == "") {

                if ($scope.CurrentBill.customerid && $scope.CurrentBill.customerid > 0) {
                    if ($scope.system_salecollectrefemp) {
                        $scope.setFocussalesrefid();
                    }
                }
                else {
                    $scope.setFocuscustomername();
                }
                return;
            }

            if ($scope.HasSuggested()) {
                $scope.autoCoding.code = '';
                $scope.setFocusautocoding();
            }

            if (($scope.autoCoding.code == "0")
                && $scope.CurrentBill.Billitems.length > 0) {

                if ($scope.IsSaleReturn) {
                    $scope.setFocusreturnbillno();
                }
                else {
                    $scope.Save(false, true);
                    $scope.autoCoding.code = '';
                }
                return;
            }

            if ($rootScope.UserInfo.p_SALES.print && ($scope.autoCoding.code == "P" || $scope.autoCoding.code == "p")) {
                if ($scope.CurrentBill.id > 0) {
                    $scope.Print($scope.CurrentBill);
                    $scope.autoCoding.code = '';
                }
                return;
            }

            if ($scope.autoCoding.code == "R" || $scope.autoCoding.code == "r") {
                if (!$scope.IsSaleReturn) {
                    $rootScope.Toaster.Notice('Please use sales return screen for returns.');
                    return;
                }

                $scope.ReverseBillValue();
                $scope.autoCoding.code = '';
                $scope.setFocusreturnbillno();
                return;
            }


            if ($scope.autoCoding.code == "m" || $scope.autoCoding.code == "M") {
                $scope.autoCoding.code = '';
                $scope.CurrentBill.id = 0;
                $scope.CurrentBill.billno = '';
                return;
            }

            if ($scope.autoCoding.code == "d" || $scope.autoCoding.code == "D") {
                $scope.autoCoding.code = '';
                $scope.ShowAdditionalDiscount();
                return;
            }

            if ($scope.autoCoding.code == "-"
                && $scope.CurrentBill.Billitems.length > 0) {
                $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[0]);
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code.startsWith("-") && $scope.autoCoding.code.length > 1
                && $scope.CurrentBill.Billitems.length > 0) {
                var lastdiscountisvalue = $scope.autoCoding.code.endsWith("-");
                var lastdiscount = $scope.GetNumber($scope.autoCoding.code.replace('-', ''));
                if (!$rootScope.UserInfo.p_CANAPPROVEDISCOUNT.view) {
                    $rootScope.Toaster.Notice("You dont' have rights to give direct discount!");
                    $scope.autoCoding.code = '';
                    return;
                }

                if (lastdiscount > parseFloat($rootScope.UserInfo.AD)) {
                    $rootScope.Toaster.Notice("Your allowed discount - " + lastdiscount + '%');
                    $scope.autoCoding.code = '';
                    return;
                }

                $scope.ApplySingleRowDiscount($scope.CurrentBill.Billitems[0], lastdiscountisvalue ? '' : '%', lastdiscount);
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code == "*") {
                $scope.CreateNew();
                return;
            }

            if ($scope.autoCoding.code == "N" || $scope.autoCoding.code == "n") {
                $scope.CreateNew();
                return;
            }

            if ($scope.autoCoding.code.startsWith('$') && !$scope.autoCoding.code.endsWith('$')) {

                if ($scope.IsSaleReturn) {
                    $scope.CurrentBill.billattributes.orderrefid = $scope.autoCoding.code.substring(1);
                    $scope.SOManager.SearchSaleOrder($scope.CurrentBill.billattributes.orderrefid,
                        $scope.CurrentBill.Billitems, function () {

                            //    var barcodes = [];
                            //    $.each(results.barcodes, function (k, v) {
                            //        for (var i = 0; i < v.qty; i++) {
                            //            barcodes.push(v.barcode);
                            //        }
                            //    });


                            //    $scope.CurrentBill.customerid = results.customerid;
                            //    source = 'SOPLAN';
                            //    if ($scope.CurrentBill.customerid > 0) {
                            //        $scope.CustomerChanged(function () {
                            //            $scope.LoadPackingSlip(results);
                            //        });
                            //    }
                            //    else {
                            //        $scope.LoadPackingSlip(results);
                            //    }

                        });
                }
                else {
                    $scope.SearchPackingSlip($scope.autoCoding.code.substring(1));
                }
                return;
            }
            if (!$scope.autoCoding.code.startsWith('$') && $scope.autoCoding.code.endsWith('$')) {
                $scope.SearchPackBarcode($scope.autoCoding.code);
                return;
            }

            if (!$scope.system_saleautomobileno &&
                $scope.autoCoding.code.length == 10) {
                var letters = /^[0-9]+$/;
                if ($scope.autoCoding.code.match(letters)) {
                    $scope.CurrentBill.phoneno = $scope.autoCoding.code;
                    $scope.autoCoding.code = '';
                    $scope.SearchCustomer();
                    return;
                }
            }

            if ($scope.autoCoding.code == 'C' || $scope.autoCoding.code == 'c') {
                $scope.ShowCustomerDialog();
                return;
            }

            if ($scope.autoCoding.code == 'L' || $scope.autoCoding.code == 'l') {
                if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                    $rootScope.Toaster.Notice('Please select the customer');
                    return;
                }

                if ($scope.CurrentBill.receivable == 0) {
                    $rootScope.Toaster.Notice('Please enter the barcode');
                    return;
                }

                $scope.ApplyRewardDialog();
                return;
            }

            if ($scope.system_supportstarpricematch && $scope.autoCoding.code.startsWith('*') &&
                $scope.CurrentBill.Billitems.length > 0) {

                var newprice = $scope.GetNumber($scope.autoCoding.code.substring(1));
                $scope.autoCoding.code = '';
                $scope.setFocusautocoding();

                if ($scope.CurrentBill.Billitems[0].barcodeDetail) {
                    for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {
                        if (typeof ($scope.CurrentBill.Billitems[i].attributes.manualprice) == 'undefined' ||
                            $scope.CurrentBill.Billitems[i].attributes.manualprice == null) {
                            $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                        }
                    }
                    return;
                }
            }

            $scope.autoCoding.autoenter = false;
            if ($scope.CuttingBarcodeParser &&
                $scope.CuttingBarcodeParser != null &&
                $scope.CuttingBarcodeParser.valid($scope.autoCoding.code)) {
                var parsebarcode = $scope.CuttingBarcodeParser.parse($scope.autoCoding.code);
                $scope.autoCoding.code = parsebarcode.barcode;
                $scope.autoCoding.qty = parsebarcode.qty;
                $scope.autoCoding.autoenter = true;
            }

            if ($scope.WeighingBarcodeParser &&
                $scope.WeighingBarcodeParser != null &&
                $scope.WeighingBarcodeParser.valid($scope.autoCoding.code)) {
                var parsebarcode = $scope.WeighingBarcodeParser.parse($scope.autoCoding.code);
                $scope.autoCoding.code = parsebarcode.barcode;
                $scope.autoCoding.qty = parsebarcode.qty;
                $scope.autoCoding.autoenter = true;
            }


            if ($scope.system_multiplesalemen && $scope.autoCoding.code == '.') {
                $scope.SelectMultipleSalesmen({}, true);
                $scope.autoCoding.code = '';
                return;
            }
            if ($scope.autoCoding.code.startsWith('.') && $scope.autoCoding.code.length > 0) {

                if ($scope.system_multiplesalemen) {
                    if ($scope.autoCoding.code.startsWith('...')) {
                        for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {
                            if ($scope.CurrentBill.Billitems[i].salesmanid &&
                                ($scope.CurrentBill.Billitems[i].salesmanid > 0 ||
                                    $scope.CurrentBill.Billitems[i].salesmanid < 0))
                                break;
                            $scope.CurrentBill.Billitems[i].salesmanid = -1;
                            $scope.autoCoding.code = '';
                        }
                        return;
                    }
                }

                restService.Get('EmployeeService/EmployeeCode/' + $scope.UserInfo.OrganizationID + '/' + $rootScope.LocationInfo.location.id + '/' + $scope.autoCoding.code.replaceAll('.', ''), true).then(function (results) {
                    if (results.id == 0) return;
                    if ($scope.system_multiplesalemen) {
                        if ($scope.autoCoding.code.startsWith('..')) {
                            for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {
                                if ($scope.CurrentBill.Billitems[i].salesmanid &&
                                    ($scope.CurrentBill.Billitems[i].salesmanid > 0 ||
                                        $scope.CurrentBill.Billitems[i].salesmanid < 0))
                                    break;
                                $scope.CurrentBill.Billitems[i].salesmancode = results.employeecode;
                                $scope.CurrentBill.Billitems[i].salesmanname = results.name;
                                $scope.CurrentBill.Billitems[i].salesmanid = results.id;
                                $scope.autoCoding.code = '';
                            }
                        } else {
                            //$scope.salesmanlist = [];
                            //if ($scope.CurrentBill.Billitems &&
                            //    $scope.CurrentBill.Billitems.length > 0 &&
                            //    $scope.CurrentBill.Billitems[0].attributes &&
                            //    $scope.CurrentBill.Billitems[0].attributes.smcodes &&
                            //    $scope.CurrentBill.Billitems[0].attributes.smcodes.length > 0)
                            //    $scope.salesmanlist = $scope.CurrentBill.Billitems[0].attributes.smcodes;
                            //$scope.salesmanlist.push(results.id);

                            for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {
                                if ($scope.CurrentBill.Billitems[i].attributes == undefined ||
                                    $scope.CurrentBill.Billitems[i].attributes == null)
                                    $scope.CurrentBill.Billitems[i].attributes = { 'smcodes': [] };

                                if ($scope.CurrentBill.Billitems[i].salesmanid &&
                                    ($scope.CurrentBill.Billitems[i].salesmanid > 0 ||
                                        $scope.CurrentBill.Billitems[i].salesmanid < 0))
                                    break;

                                if ($scope.CurrentBill.Billitems[i].attributes.smcodes == undefined ||
                                    $scope.CurrentBill.Billitems[i].attributes.smcodes == null)
                                    $scope.CurrentBill.Billitems[i].attributes.smcodes = [];
                                $scope.CurrentBill.Billitems[i].attributes.smcodes.push(results.id);
                                $scope.autoCoding.code = '';
                            }
                        }
                        $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                    }                    
                    else {
                        if (typeof ($scope.CurrentBill.Billitems[0].salesmanid) == 'undefined' ||
                            $scope.CurrentBill.Billitems[0].salesmanid == null) {
                            for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {
                                if (typeof ($scope.CurrentBill.Billitems[i].salesmanid) == 'undefined' ||
                                    $scope.CurrentBill.Billitems[i].salesmanid == null) {
                                    $scope.CurrentBill.Billitems[i].salesmancode = results.employeecode;
                                    $scope.CurrentBill.Billitems[i].salesmanname = results.name;
                                    $scope.CurrentBill.Billitems[i].salesmanid = results.id;
                                    $scope.autoCoding.code = '';
                                }
                            }
                            $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                        }
                        else {
                            var replacesmid = $scope.CurrentBill.Billitems[0].salesmanid;
                            for (var i = $scope.CurrentBill.Billitems.length - 1; i >= 0; i--) {
                                if ($scope.CurrentBill.Billitems[i].salesmanid == replacesmid) {
                                    $scope.CurrentBill.Billitems[i].salesmancode = results.employeecode;
                                    $scope.CurrentBill.Billitems[i].salesmanname = results.name;
                                    $scope.CurrentBill.Billitems[i].salesmanid = results.id;
                                    $scope.autoCoding.code = '';
                                }
                            }
                            $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                        }
                    }

                    $scope.LocalContext.Post('$scope.CurrentBill', $scope.CurrentBill, false);
                });
                return;
            }

            for (var index = 0; index < $scope.CurrentBill.Billitems.length; index++) {

                if (!$scope.system_disableuniquebarcode &&
                    $scope.system_restrictuniquebarcode &&
                    $scope.CurrentBill.Billitems[index].barcode == $scope.autoCoding.code) {
                    if ($scope.system_restrictuniquebarcode) {
                        $scope.autoCoding.code = "";
                        $rootScope.Toaster.Error('Already Added');
                        return;
                    }
                }

                if (($scope.CurrentBill.Billitems[index].barcode == $scope.autoCoding.code || $scope.CurrentBill.Billitems[index].displaycode == $scope.autoCoding.code) &&
                    (typeof ($scope.CurrentBill.Billitems[index].salesmanid) == 'undefined'
                        || $scope.CurrentBill.Billitems[index].salesmanid == 0
                        || $scope.CurrentBill.Billitems[index].sellingmode == 1)) {

                    if (($scope.CurrentBill.Billitems[index].qty < 0 && $scope.IsSaleReturn) ||
                        ($scope.CurrentBill.Billitems[index].qty > 0 && !$scope.IsSaleReturn)) {

                        if ($scope.CurrentBill.Billitems[index].sellingmode == 1) {

                            if (typeof ($scope.CurrentBill.Billitems[index].stock) != 'undefined' && $scope.CurrentBill.Billitems[index].stock <= 1) {
                                if ($scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[index]))
                                    $rootScope.Toaster.Notice('Deleted');
                                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                $scope.autoCoding.code = "";
                                return;
                            }
                            else {

                                var newQty = $scope.IsSaleReturn ? -1 : 1;

                                $scope.CurrentBill.Billitems[index].qty += newQty;

                                if (newQty > 0 || $scope.IsSaleReturn) {
                                    $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + newQty).round(2);
                                    $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + newQty).round(2);
                                    //$scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);
                                }

                                if ($scope.CurrentBill.Billitems[index].sellingmode == 3) {
                                    $scope.CurrentBill.Billitems[index].piece = $scope.GetNumber($scope.CurrentBill.Billitems[index].piece) + newQty;
                                }
                                else {
                                    $scope.CurrentBill.Billitems[index].piece = $scope.GetNumber($scope.CurrentBill.Billitems[index].qty);
                                }

                                //if ($scope.CurrentBill.Billitems[index].piece > 0) {
                                //    $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                                //}

                                $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);
                                $scope.autoCoding.code = "";
                                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                $rootScope.Toaster.Notice('Qty increased to ' + $scope.CurrentBill.Billitems[index].piece);

                                if ($scope.CurrentBill.Billitems[index].piece == $scope.system_multiplesaleqty && $scope.system_multiplesaleqtypopup) {

                                    var evt = event;
                                    $timeout(function () {

                                        if (typeof evt.stopPropagation == "function") {
                                            evt.stopPropagation();
                                        } else {
                                            evt.cancelBubble = true;
                                        }

                                        $rootScope.Prompt('Multiple', 'No Of Pieces', function (count) {
                                            var totalcount = $rootScope.GetNumber(count);
                                            if (totalcount > 0 || $scope.IsSaleReturn) {

                                                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - $scope.CurrentBill.Billitems[index].qty).round(2);
                                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);

                                                $scope.CurrentBill.Billitems[index].qty = totalcount;
                                                $scope.CurrentBill.Billitems[index].piece = totalcount;

                                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                                                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + $scope.CurrentBill.Billitems[index].qty).round(2);

                                                $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);
                                                $scope.SOManager.UpdateItem($scope.CurrentBill.Billitems[index], function () {
                                                    $rootScope.Toaster.Notice('Qty altered to ' + $scope.CurrentBill.Billitems[index].piece);
                                                    $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                                                    $scope.UpdateOffer();
                                                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                                });
                                            }
                                        }, $scope.CurrentBill.Billitems[index].piece);

                                    }, 100);

                                }
                                else {
                                    $scope.SOManager.UpdateItem($scope.CurrentBill.Billitems[index], function () {
                                        $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                                        $scope.UpdateOffer();
                                        $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                    });
                                }

                                return;
                            }
                        }
                        if ($scope.CurrentBill.Billitems[index].sellingmode == 2) {

                            var newQty = $scope.IsSaleReturn ? -1 : 1;

                            $scope.CurrentBill.Billitems[index].qty += newQty;
                            if (newQty || $scope.IsSaleReturn) {
                                $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + newQty).round(2);
                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);
                            }

                            if ($scope.CurrentBill.Billitems[index].sellingmode == 3) {
                                $scope.CurrentBill.Billitems[index].piece = $scope.GetNumber($scope.CurrentBill.Billitems[index].piece) + newQty;
                            }
                            else {
                                $scope.CurrentBill.Billitems[index].piece = parseInt($scope.CurrentBill.Billitems[index].qty);
                            }
                            if (newQty) {
                                $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                            }
                            $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);
                            $scope.autoCoding.code = "";
                            $rootScope.Toaster.Notice('Qty increased to ' + $scope.CurrentBill.Billitems[index].qty);

                            if ($scope.CurrentBill.Billitems[index].piece == $scope.system_multiplesaleqty && $scope.system_multiplesaleqtypopup) {
                                var evt = event;
                                //$timeout(function () {

                                //    if (typeof evt.stopPropagation == "function") {
                                //        evt.stopPropagation();
                                //    } else {
                                //        evt.cancelBubble = true;
                                //    }

                                $rootScope.Prompt('Multiple', 'No Of Pieces', function (count) {

                                    var totalcount = $rootScope.GetNumber(count);
                                    if (totalcount > 0 || $scope.IsSaleReturn) {

                                        $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - $scope.CurrentBill.Billitems[index].qty).round(2);
                                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);

                                        $scope.CurrentBill.Billitems[index].qty = totalcount;
                                        $scope.CurrentBill.Billitems[index].piece = totalcount;

                                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                                        $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + $scope.CurrentBill.Billitems[index].qty).round(2);
                                        $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);
                                        $rootScope.Toaster.Notice('Qty altered to ' + $scope.CurrentBill.Billitems[index].piece);
                                        $scope.SOManager.UpdateItem($scope.CurrentBill.Billitems[index], function () {
                                            $scope.UpdateOffer();
                                            $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                                            $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                        });
                                    }
                                }, $scope.CurrentBill.Billitems[index].piece);
                                //}, 100);
                            }
                            else {
                                $scope.SOManager.UpdateItem($scope.CurrentBill.Billitems[index], function () {
                                    $scope.UpdateOffer();
                                    $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                });
                            }
                            return;
                        }

                    }

                }
            }

            $scope.SalesDetail.printingname = '';
            $scope.SalesDetail.productid = 0;
            $scope.SalesDetail.sellingmode = 0;
            $scope.SalesDetail.barcodeDetail = null;
            $scope.SalesDetail.qtyeditable = false;
            $scope.SalesDetail.priceeditable = false;

            if ($scope.autoCoding.code.startsWith('#') || $scope.BillNoConfig.isbillno($scope.autoCoding.code)
                || $scope.BillNoConfig.isbulkbillno($scope.autoCoding.code)) {
                $scope.BillNoHandling();
                return;
            }

            $scope.SearchBarcodeDirect(false);
        };

        $scope.SearchBarcodeDirect = function (fromBillSearch) {


            if (typeof ($scope.CurrentBill.isbulkreturn) != 'undefined' &&
                $scope.CurrentBill.isbulkreturn) {
                $rootScope.Toaster.Error("Approved Bill can't support additional items");
                return;
            }

            if (typeof ($scope.CurrentBill.isbulkreturngoods) != 'undefined' &&
                $scope.CurrentBill.isbulkreturngoods) {
                $rootScope.Toaster.Error("Approved Bill can't support additional items");
                return;
            }

            if ($scope.SearchModeEnabled) {
                $scope.SearchMFGBarcode();
                return;
            }

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                $rootScope.Toaster.Notice('Please select the customer');
                return;
            }

            var barcode = $scope.autoCoding.code;
            if ($scope.system_maxbarcodelength > 0 && barcode.length > $scope.system_maxbarcodelength) {

                var barcodeparts = barcode.split(/(\d+)/);
                if (barcodeparts[0].length == 1) {
                    barcode = barcode.substring(0,
                        $scope.system_maxbarcodelength + barcodeparts[0].length);
                }
                else {
                    barcode = barcode.substring(0,
                        $scope.system_maxbarcodelength);
                }
            }


            if (barcode == null || barcode == '' || barcode.indexOf(' ') >= 0 ||
                barcode.indexOf('\\') >= 0) {
                if (!fromBillSearch && !$scope.BillNoHandling())
                    $rootScope.Toaster.Error('Unsupported barcode format');
                return;
            }
            restService.Get($scope.SearchAPI + $rootScope.LocationInfo.location.id + '/' + $scope.qEncodeURIComponent(barcode)).then(function (results) {

                if (results.errormessage != null && results.errormessage != '') {

                    if (results.sellingmode == 3 &&
                        results.errormessage.indexOf('Loaded from bill no') >= 0) {
                        var allparts = results.errormessage.split('#');
                        var customername = allparts[allparts.length - 1].trim();
                        if ($scope.CurrentBill.customername.indexOf(customername) < 0) {
                            $rootScope.Toaster.Error(results.errormessage + " can't load for " + $scope.CurrentBill.customername);
                            return;
                        }
                    }
                    $rootScope.Toaster.Error(results.errormessage);
                }

                if (results.id != 0) {

                    //if ($scope.CurrentBill.id > 0) {
                    //    $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
                    //    $scope.CurrentBill = (JSON.parse(JSON.stringify($scope.TemplateCurrentBill)));
                    //    $scope.SchemeReturnBillItems = [];
                    //    $scope.CurrentBill.billdate = new Date();
                    //}

                    if (typeof (results.imageid) != 'undefined' && results.imageid != null && results.imageid.length > 0) {
                        $scope.currentimageid = results.imageid;
                        $scope.ShowImage = true;
                    }
                    else {
                        $scope.ShowImage = false;
                    }
                    results.buyingprice = results.buyingprice.round(2);
                    $scope.SalesDetail.barcode = results.barcode;
                    $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.SalesDetail.attributes = {
                        'serialno': results.serialno,
                        'designid': results.designid,
                        'size': results.sizename,
                        'mrp': results.mrp,
                        'brand': results.brandname,
                        'typename': results.typename,
                        'colourname': results.colourname,
                        'materialname': results.materialname,
                        'stylename': results.stylename,
                        'patternname': results.patternname
                    };

                    //if ($scope.CurrentBill.ccmarginmode) {
                    //    if (results.buyingprice > 0) {
                    //        var newprice = results.buyingprice + (results.buyingprice * $scope.CurrentBill.ccmargin / 100).round(0);
                    //        var newdiscount = results.price - newprice;
                    //        results.discount = 0;
                    //        results.additionaldiscount = 0;
                    //        results.price = newprice;
                    //        results.additionaldiscountpercentage = 0;

                    //        results.taxInfo = $scope.SalesTax.find(function (arritem) {
                    //            return arritem.id == results.taxid;
                    //        });

                    //        if (results.taxInfo != null) {
                    //            if (results.taxInfo.taxsplit == null) {
                    //                results.taxInfo.taxsplit = { 'taxpercentage': results.taxInfo.taxpercentage };
                    //            }
                    //            results.taxInfo.taxsplit.findTax = $scope.findTax;
                    //        }

                    //        if (results.taxInfo != null && results.taxInfo.taxsplit != null) {
                    //            var taxitem = results.taxInfo.taxsplit.findTax(results.price);
                    //            results.taxpercentage = taxitem.cgst + taxitem.sgst;
                    //        }

                    //        if (!$scope.CurrentBill.isinterstatesale) {
                    //            var halfTax = (results.price * results.taxpercentage / 2 / 100).round(5);
                    //            results.itemtax = (halfTax * 2).round(5);
                    //        }
                    //        else {
                    //            results.itemtax = (results.price * results.taxpercentage / 100).round(5);
                    //        }
                    //        results.price = (results.price + results.itemtax).round(0);
                    //        results.salerate = results.price;
                    //    }
                    //}

                    if ($scope.CurrentBill.pricesource > 0) {

                        if ($scope.CurrentBill.pricesource == 5 && $scope.system_supportbrandprice &&
                            results.brandmargin > 0 && results.mrp > 0) {
                            results.price = results.mrp - (results.mrp * results.brandmargin / 100).round($scope.rounding_rate);
                        }

                        if ($scope.CurrentBill.pricesource == 6 && $scope.system_supportbrandprice &&
                            results.brandmargin > 0 && results.buyingprice > 0) {
                            results.price = results.buyingprice + (results.buyingprice * results.brandmargin / 100).round($scope.rounding_rate);

                            $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                                return arritem.id == results.taxid;
                            });

                            if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null)
                                $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                            var taxitemvalue = $scope.SalesDetail.taxInfo.taxsplit.findTax(results.price, true);
                            var taxpercentage = taxitemvalue.cgst + taxitemvalue.sgst;
                            var tax = (taxpercentage * results.price / 100).round($scope.rounding_tax);
                            results.price += tax;
                        }

                        if ($scope.CurrentBill.pricesource == 1) {
                            if (results.wsrate > 0) {
                                results.price = results.wsrate;
                                results.priceextax = results.wsrateextax;
                            } else if (results.dealerprice > 0) {
                                results.price = results.dealerprice;
                                results.priceextax = results.dealerpriceextax;
                            }
                            if ($scope.system_b2bexclusivetax) {
                                results.priceextax = results.price;
                                $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                                    return arritem.id == results.taxid;
                                });

                                if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null)
                                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                                var taxitemvalue = $scope.SalesDetail.taxInfo.taxsplit.findTax(results.price, true);
                                var taxpercentage = taxitemvalue.cgst + taxitemvalue.sgst;
                                var tax = (taxpercentage * results.price / 100).round($scope.rounding_tax);
                                results.price += tax;
                            }
                        }

                        if ($scope.CurrentBill.pricesource == 2 && results.dealerprice > 0) {
                            results.price = results.dealerprice;
                            results.priceextax = results.dealerpriceextax;
                        }

                        if (results.priceextax == 0) {
                            $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                                return arritem.id == results.taxid;
                            });

                            if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null)
                                $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                            var taxitemvalue = $scope.SalesDetail.taxInfo.taxsplit.findTax(results.price);
                            var taxpercentage = taxitemvalue.cgst + taxitemvalue.sgst;
                            var tax = (taxpercentage * results.price / 100).round($scope.rounding_tax);
                            results.priceextax = results.price - tax;
                        }
                        results.discount = 0;
                        results.discountpercentage = 0;
                        results.additionaldiscount = 0;
                        results.additionaldiscountpercentage = 0;
                    }

                    var product = $scope.Products.find(function (item) {
                        return item.id == results.productid;
                    });

                    $scope.AddBarcode(results, false);

                    $scope.SalesDetail.sellingmode = product.sellingmode;
                    $scope.SalesDetail.qty = $scope.IsSaleReturn ? -1 : 1;

                    if (results.stockisbit) {
                        $scope.SalesDetail.qty = $scope.IsSaleReturn ? $rootScope.GetNumber(results.retailqty) * -1 : results.retailqty;
                    }
                    $scope.SalesDetail.sellingmode = product.sellingmode;
                    if ($scope.SalesDetail.sellingmode == 3 && !results.stockisbit) {
                        if ($scope.system_poscutstock) {
                            $scope.autoCoding.qty = $scope.IsSaleReturn ? results.qty : results.retailqty;
                            if ($scope.system_poscutstockauto && !$scope.IsSaleReturn) {
                                $timeout(function () {
                                    $scope.LastBiteEnter();
                                }, 10)
                                return;
                            }
                        }
                        $scope.setFocusautocodingqty();
                        if ($scope.autoCoding.autoenter) {
                            $scope.autoCoding.autoenter = false;
                            $scope.LastBiteEnter();
                            return;
                        }
                    }
                    else {
                        if ($scope.system_multipleinvoiceqty) {
                            $scope.setFocusmultipleinvoiceqty();
                        }
                        else if ($scope.system_supportmanualprice) {
                            if ($scope.IsSaleReturn)
                                $scope.autoCoding.price = ($scope.SalesDetail.price && $scope.SalesDetail.price > 0) ?
                                    $scope.SalesDetail.price : results.price;
                            $scope.setFocuseditprice();
                        }
                        else {
                            $scope.AddBillItem();
                            $scope.autoCoding.code = "";
                            $scope.autoCoding.qty = "";
                            $scope.autoCoding.lastbit = "";
                            $scope.setFocusautocoding();
                        }
                    }
                }
                else if (!fromBillSearch) {

                    $scope.BillNoHandling();
                }
            });
        }

        $scope.ShowBatchList = function () {
            restService.Get('SalablegoodsService/BatchListByBrand/' + $rootScope.LocationInfo.location.id + '/' +
                $scope.SearchCondition.BrandID.toString()
            ).then(function (results) {
                $scope.BatchList = results;
                $timeout(function () {
                    $scope.setFocusBatchID();
                    $scope.$digest();
                }, 10);
            });
        }

        //$scope.marginchanged = function () {

        //    $scope.SalesDetail.price = $scope.SalesDetail.cost;
        //    $scope.SalesDetail.amount = $scope.SalesDetail.cost * $scope.GetNumber($scope.SalesDetail.qty);
        //    if ($scope.GetNumber($scope.SalesDetail.margin) > 0) {
        //        $scope.SalesDetail.price += ($scope.SalesDetail.price * $scope.GetNumber($scope.SalesDetail.margin) / 100).round(2)
        //        $scope.SalesDetail.amount += ($scope.SalesDetail.amount * $scope.GetNumber($scope.SalesDetail.margin) / 100).round(2);
        //    }

        //    if ($scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage) > 0) {
        //        $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.price * $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage) / 100).round(2)
        //        $scope.SalesDetail.amount -= ($scope.SalesDetail.additionaldiscount * $scope.GetNumber($scope.SalesDetail.qty)).round(2);
        //    }

        //    if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null) {
        //        $scope.SalesDetail.taxpercentage = $scope.SalesDetail.taxInfo.taxpercentage;
        //        $scope.SalesDetail.taxname = $scope.SalesDetail.taxInfo.name;
        //        if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
        //            $scope.SalesDetail.additionaldiscount = 0;

        //        if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount))
        //            $scope.SalesDetail.schemediscount = 0;

        //        $scope.SalesDetail.baseprice = $scope.SalesDetail.cost;
        //        if ($scope.SalesDetail.taxInfo != null && $scope.SalesDetail.taxInfo.taxsplit != null) {
        //            var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.baseprice);
        //            $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
        //        }

        //        if (!$scope.CurrentBill.isinterstatesale) {
        //            var halfTax = ($scope.SalesDetail.baseprice * ($scope.SalesDetail.taxpercentage / 2) / 100).round($scope.rounding_tax);
        //            $scope.SalesDetail.itemtax = (halfTax * 2).round($scope.rounding_tax);
        //        }
        //        else {
        //            $scope.SalesDetail.itemtax = ($scope.SalesDetail.baseprice * $scope.SalesDetail.taxpercentage / 100).round($scope.rounding_tax);
        //        }
        //        $scope.SalesDetail.price = ($scope.SalesDetail.price + $scope.SalesDetail.itemtax).round(0);
        //        $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round($scope.rounding_tax);
        //        $scope.SalesDetail.amount = ($scope.SalesDetail.amount + $scope.SalesDetail.tax).round(0);
        //    }

        //    $scope.SalesDetail.rate = $scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount;
        //    $scope.SalesDetail.salerate = $scope.SalesDetail.price * $scope.SalesDetail.qty;
        //}


        $scope.AddGroupItem = function (items) {

            var baseSalesDetail = (JSON.parse(JSON.stringify($scope.SalesDetail)));

            for (var i = 0; i < items.length; i++) {
                var results = items[i];
                $scope.SalesDetail = (JSON.parse(JSON.stringify(baseSalesDetail)));
                $scope.SalesDetail.barcode = results.barcode;
                $scope.SalesDetail.printingname = results.sellingname;
                if (results.qty == 0) results.qty = 1;
                $scope.SalesDetail.qty = results.qty;
                $scope.SalesDetail.stock = results.stockqty;
                $scope.SalesDetail.hsncode = results.hsncode;
                $scope.SalesDetail.attributes = {
                    'serialno': results.serialno,
                    'groupkey': results.groupkey,
                    'groupname': results.groupname,
                    'designid': results.designid,
                    'sizegroup': results.sizegroup,
                    'colourname': results.colourname,
                    'colourcode': results.colourcode,
                    'size': results.sizename,
                    'mrp': results.mrp,
                    'obarcode': results.obarcode,
                    'packingno': results.packingno,
                    'so': results.saleorderitems,
                    'brand': results.brandname,
                    'cost': results.buyingprice,
                    'typename': results.typename,
                    'materialname': results.materialname,
                    'stylename': results.stylename,
                    'patternname': results.patternname
                };
                $scope.SalesDetail.discountpercentage = 0;
                $scope.SalesDetail.additionaldiscountpercentage = 0;
                $scope.SalesDetail.schemediscountpercentage = 0;

                var product = $scope.Products.find(function (item) {
                    return item.id == results.productid;
                });

                if (!($scope.IsUANBased && $scope.IsSaleReturn)) {
                    if ($scope.CurrentBill.pricesource > 0) {
                        if ($scope.CurrentBill.pricesource == 1) {
                            results.price = results.wsrateextax;
                            results.priceextax = results.wsrateextax;
                            results.etprice = results.wsrateextax;
                        }
                        else if ($scope.CurrentBill.pricesource == 2) {
                            results.price = results.dealerpriceextax;
                            results.priceextax = results.dealerpriceextax;
                            results.etprice = results.dealerpriceextax;
                        }
                    }
                    else {
                        results.price = results.priceextax;
                        results.priceextax = results.priceextax;
                        results.etprice = results.priceextax;
                    }
                }

                $scope.AddAdditionalChargeFromSO(results.qty, results);
                if (results.etdiscount && results.etdiscount != null && results.etdiscount != 0)
                    $scope.SalesDetail.etdiscount = results.etdiscount;
                if (results.priceextax == 0) {
                    $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                        return arritem.id == results.taxid;
                    });

                    if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null)
                        $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

                    var taxitemvalue = $scope.SalesDetail.taxInfo.taxsplit.findTax(results.price);
                    var taxpercentage = taxitemvalue.cgst + taxitemvalue.sgst;
                    var tax = (taxpercentage * results.price / 100).round($scope.rounding_tax);
                    results.priceextax = results.price - tax;
                }


                //results.discount = 0;
                //results.discountpercentage = 0;
                //results.additionaldiscount = 0;
                //results.additionaldiscountpercentage = 0;

                $scope.SalesDetail.etprice = results.priceextax;
                $scope.SalesDetail.pricewithouttax = results.priceextax;
                $scope.SalesDetail.pricewithtax = results.price;
                //results.discount = 0;
                //results.discountpercentage = 0;
                //results.additionaldiscount = 0;
                //results.additionaldiscountpercentage = 0;
                //results.schemediscount = 0;
                //results.schemediscountpercentage = 0;
                $scope.AddBarcode(results, false);
                var product = $scope.Products.find(function (item) {
                    return item.id == results.productid;
                });
                /*var brand = $scope.Brands.find(function (item) {
                    return item.id == results.brandid;
                });*/
                $scope.SalesDetail.sellingmode = product.sellingmode;
                $scope.AddBillItem(false, false);

            }
            $scope.ComputeAddtionalChargeValues();
            $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
            $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
            $scope.setFocusautocoding();
        }

        $scope.AddAdditionalChargeFromSO = function (qty, results) {

            if (results.addnlchargeid > 0) {
                var exist = $scope.CurrentBill.additionalchargeslist.find(function (item) {
                    return item.ChargeID == results.addnlchargeid;
                });
                if (exist && exist != null) {
                    exist.Charge += (qty * exist.Rate);
                    exist.Amount += (qty * exist.Rate);
                }
                else {
                    var addn = $scope.AdditionalChargeList.find(function (item) {
                        return item.id == results.addnlchargeid;
                    });
                    if (addn.additionalinfo.paytype == 'PACKING') {
                        results.price = results.wsrateextax + addn.additionalinfo.billdiscount;
                        results.priceextax = results.wsrateextax + addn.additionalinfo.billdiscount;
                        results.etprice = results.wsrateextax + addn.additionalinfo.billdiscount;
                        $scope.SalesDetail.attributes.packing = addn.additionalinfo.billdiscount;
                        $scope.SalesDetail.attributes.boxid = results.addnlchargeid;
                    }
                    else {
                        $scope.CurrentBill.additionalchargeslist.push({
                            'ChargeID': results.addnlchargeid,
                            'Name': addn.name,
                            'Type': addn.additionalinfo.paytype,
                            'Tax': 0,
                            'Rate': addn.additionalinfo.billdiscount,
                            'Charge': qty * results.addnlcharge,
                            'Amount': qty * results.addnlcharge
                        });
                    }
                }
            }

            if (results.fgboxid != results.soboxid) {

                var actualprice = results.priceextax;

                if ($scope.CurrentBill.pricesource == 1)
                    actualprice = results.wsrateextax;

                if ($scope.CurrentBill.pricesource == 2)
                    actualprice = results.dealerpriceextax;

                $scope.SalesDetail.attributes.boxid = results.fgboxid;
                if (results.fgboxid > 0) {
                    var fgbox = $scope.AdditionalChargeList.find(function (item) {
                        return item.id == results.fgboxid;
                    });
                    actualprice -= fgbox.additionalinfo.billdiscount;
                    results.price = actualprice;
                    results.priceextax = actualprice;
                    results.etprice = actualprice;
                }

                if (results.soboxid > 0) {
                    var fgbox = $scope.AdditionalChargeList.find(function (item) {
                        return item.id == results.soboxid;
                    });
                    actualprice += fgbox.additionalinfo.maxlimit;
                    results.price = actualprice;
                    results.priceextax = actualprice;
                    results.etprice = actualprice;
                }
            }


            if (results.discountpercentage && results.discountpercentage > 0) {
                results.etdiscount = (results.discountpercentage * results.etprice / 100).round(4);
            }
        }

        $scope.AddB2BItem = function () {

            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                $rootScope.Toaster.Notice('Please select the customer');
                return false;
            }

            var selectedbatchid = $scope.SearchCondition.BatchID;
            var selectedbatch = $scope.BatchList.find(function (item) {
                return item.id == selectedbatchid;
            });

            var baseSalesDetail = (JSON.parse(JSON.stringify($scope.SalesDetail)));

            var require = $scope.GetNumber($scope.SalesDetail.qty);
            if (selectedbatch.qty < require) {
                $rootScope.Toaster.Notice('Available stock - ' + selectedbatch.qty);
                return;
            }

            $scope.CurrentBill.ccmargin = $scope.SalesDetail.margin;
            $scope.CurrentBill.ccmarginmode = true;

            for (var i = 0; require > 0 && i < selectedbatch.items.length; i++) {
                var results = selectedbatch.items[i];

                $scope.SalesDetail = (JSON.parse(JSON.stringify(baseSalesDetail)));
                $scope.SalesDetail.barcode = results.barcode;
                $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
                $scope.SalesDetail.hsncode = results.hsncode;
                $scope.SalesDetail.attributes = { 'serialno': results.serialno };
                $scope.SalesDetail.additionaldiscountpercentage = $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage);
                results.price = $scope.SalesDetail.price;
                results.discount = 0;
                results.discountpercentage = 0;
                results.additionaldiscount = $scope.SalesDetail.additionaldiscount;
                results.additionaldiscountpercentage = $scope.GetNumber($scope.SalesDetail.additionaldiscountpercentage);
                results.schemediscount = 0;
                results.schemediscountpercentage = 0;
                $scope.AddBarcode(results, false);

                var product = $scope.Products.find(function (item) {
                    return item.id == results.productid;
                });
                var brand = $scope.Brands.find(function (item) {
                    return item.id == results.brandid;
                });

                $scope.SalesDetail.sellingmode = product.sellingmode;
                $scope.SalesDetail.qty = 0;

                if (results.iqty - (results.dqty + results.oqty) > require) {
                    $scope.SalesDetail.qty = require;
                    require = 0;
                }
                else {
                    $scope.SalesDetail.qty = results.iqty - (results.dqty + results.oqty);
                    require -= results.iqty - (results.dqty + results.oqty);
                }

                $scope.AddBillItem(true);
            }
            $scope.SearchCondition.BatchID = null;
            $scope.setFocusBatchID();
        }

        $scope.SelectBatch = function () {
            if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid <= 0 ||
                typeof ($scope.CurrentBill.customerdiscount) == 'undefined' || $scope.CurrentBill.customerdiscount <= 0) {
            }

            if (typeof ($scope.SearchCondition.BatchID) == 'undefined' ||
                $scope.SearchCondition.BatchID == null ||
                $scope.SearchCondition.BatchID == '') {
                $rootScope.Toaster.Notice('Please select the batch');
                return;
            }


            var selectedbatchid = $scope.SearchCondition.BatchID;
            var selectedbatch = $scope.BatchList.find(function (item) {
                return item.id == selectedbatchid;
            });

            if (typeof (selectedbatch) != 'undefined' && selectedbatch != null) {
                $scope.SelectedBatch = selectedbatch;
            }
            else {
                return;
            }

            $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));

            //var qty = $scope.GetNumber($scope.SearchCondition.qty);
            //if (qty <= 0) {
            //    $rootScope.Toaster.Notice('Please enter the qty');
            //    return;
            //}

            //if (selectedbatch.qty < qty) {
            //    $rootScope.Toaster.Notice('Available Stock - ' + selectedbatch.qty);
            //    return;
            //}

            $scope.SalesDetail.cost = selectedbatch.items[0].buyingprice;
            if (typeof ($scope.CurrentBill.ccmarginmode) != 'undefined') {
                $scope.SalesDetail.margin = $scope.CurrentBill.ccmargin;
            }
            //var require = qty;
            //for (var i = 0; require > 0 && i < selectedbatch.items.length; i++) {
            //    var item = selectedbatch.items[i];
            //    if (item.iqty - (item.dqty + item.oqty) > require) {
            //        $scope.ShowBatchDetail(item, require);
            //        require = 0;
            //    }
            //    else {
            //        $scope.ShowBatchDetail(item, item.iqty - (item.dqty + item.oqty));
            //        require -= item.iqty - (item.dqty + item.oqty);
            //    }
            //}

            if (typeof (selectedbatch.items[0].imageid) != 'undefined' && selectedbatch.items[0].imageid != null && selectedbatch.items[0].imageid.length > 0) {
                $scope.currentimageid = selectedbatch.items[0].imageid;
                $scope.ShowImage = true;
            }
            else {
                $scope.ShowImage = false;
            }

            $scope.SalesDetail.barcode = selectedbatch.items[0].barcode;
            $scope.SalesDetail.stock = (selectedbatch.items[0].iqty - (selectedbatch.items[0].dqty + selectedbatch.items[0].oqty));
            $scope.SalesDetail.hsncode = selectedbatch.items[0].hsncode;
            $scope.SalesDetail.attributes = { 'serialno': selectedbatch.items[0].serialno };

            var product = $scope.Products.find(function (item) {
                return item.id == selectedbatch.items[0].productid;
            });

            $scope.SalesDetail.sellingmode = product.sellingmode;
            $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                return arritem.id == selectedbatch.items[0].taxid;
            });

            if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null)
                $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;

            $scope.SearchCondition.qty = null;
            $timeout(function () {
                $scope.setFocusmargin();
                $scope.$digest();
            }, 10);
        }

        $scope.ShowBatchDetail = function (results, qty) {
            if (typeof (results.imageid) != 'undefined' && results.imageid != null && results.imageid.length > 0) {
                $scope.currentimageid = results.imageid;
                $scope.ShowImage = true;
            }
            else {
                $scope.ShowImage = false;
            }

            $scope.SalesDetail.barcode = results.barcode;
            $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
            $scope.SalesDetail.hsncode = results.hsncode;
            $scope.SalesDetail.attributes = { 'serialno': results.serialno };
            $scope.AddBarcode(results, false);

            var product = $scope.Products.find(function (item) {
                return item.id == results.productid;
            });

            $scope.SalesDetail.sellingmode = product.sellingmode;
            $scope.SalesDetail.qty = qty;
            $scope.AddBillItem();
        }

        $scope.BillNoHandling = function () {
            //($scope.billprefix != null && $scope.autoCoding.code.startsWith($scope.billprefix) && $scope.autoCoding.code.length > 8) || ($scope.autoCoding.code.startsWith('#'))
            if ($scope.BillNoConfig.isbillno($scope.autoCoding.code)) {
                $scope.SearchCondition.billno = ($scope.autoCoding.code.startsWith('#') ? $scope.autoCoding.code.substring(1) : $scope.autoCoding.code);
                $scope.SearchCondition.billid = 0;
                $scope.SearchCondition.isbulkreturn = false;
                $scope.SearchBill(function (bill) {
                    if (bill != null) {
                        if ($scope.autoCoding.code.startsWith('#') && $scope.IsSaleReturn) {
                            $scope.autoCoding.code = "";
                            $scope.autoCoding.qty = "";
                            $scope.autoCoding.lastbit = "";
                        }
                        else {
                            $scope.autoCoding.code = "";
                            $scope.autoCoding.qty = "";
                            $scope.autoCoding.lastbit = "";
                            $scope.CurrentBill.isbulkreturn = false;
                            $scope.CurrentBill.returnbillno = bill.billno;
                            bill.returnbillno = bill.billno;
                        }
                        var oldbill = $scope.lastBills.find(function (item) {
                            return item.id == bill.id;
                        });

                        if (typeof (oldbill) == 'undefined' || oldbill == null)
                            $scope.lastBills.push((JSON.parse(JSON.stringify(bill))));
                        $scope.setFocusautocoding();
                    }
                    else {
                        $scope.SearchBarcodeDirect(true);
                    }

                });
                return true;
            }
            else if (!$scope.IsSaleReturn && $scope.BillNoConfig.isbulkbillno($scope.autoCoding.code)) {
                $scope.SearchCondition.billno = $scope.autoCoding.code;
                $scope.SearchCondition.billid = 0;
                $scope.SearchCondition.isbulkreturn = true;
                $scope.SearchBill(function (bill) {

                    if (bill != null) {

                        $scope.autoCoding.code = "";
                        $scope.autoCoding.qty = "";
                        $scope.autoCoding.lastbit = "";
                        $scope.CurrentBill.isbulkreturn = true;
                        $scope.CurrentBill.returnbillno = bill.billno;
                        $scope.CurrentBill.billno = bill.billno;
                        bill.returnbillno = bill.billno;
                        bill.isbulkreturn = true;
                        $scope.setFocusautocoding();

                        if (bill.billtype == 20 || bill.billtype == 21) {
                            if (typeof (bill.billattributes) == 'undefined')
                                bill.billattributes = {};
                            bill.billattributes.ebillno = [];
                            bill.billattributes.ebillno.push({ 'billno': bill.billno, 'countername': bill.billattributes.countername });
                            bill.billtype = 0;
                            bill.isb2b = false;
                            bill.id = 0;
                            bill.billmasterid = 0;
                            bill.iscreditbill = false;
                            bill.returnbillno = '';
                            bill.billno = '';
                            bill.isbulkreturn = false;
                        }
                        else {

                            var oldbill = $scope.lastBills.find(function (item) {
                                return item.id == bill.id;
                            });

                            if (typeof (oldbill) == 'undefined' || oldbill == null)
                                $scope.lastBills.push((JSON.parse(JSON.stringify(bill))));
                            else
                                oldbill.isbulkreturn = true;

                            if ($scope.IsBulkSale) {
                                $rootScope.Confirm('Return goods', 'Do you want to return the previous approval goods without billing?',
                                    function () {
                                        if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                            $scope.CurrentBill.billattributes = {};
                                        $scope.CurrentBill.isbulkreturngoods = true;
                                    });
                            } else {
                                $rootScope.Confirm('Return goods', 'Did you received products except billing products?',
                                    function () {
                                        if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                            $scope.CurrentBill.billattributes = {};
                                        $scope.CurrentBill.isbulkreturngoods = true;
                                    });
                            }
                        }
                    } else {
                        $scope.SearchBarcodeDirect(true);
                    }
                    /*if ($scope.IsBulkSale) {
                        $scope.ShowCancellDialog(bill);
                    };*/
                });
                return true;
            }
        }


        $scope.RequestCustomerOTP = function () {
            var request = {
                "Item": {
                    'id': $scope.Parent.CurrentBill.customerid,
                    'mobileno': $scope.Parent.CurrentBill.phoneno
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CustomerService/SendOTP",
                data: srequest
            }).success(function (result) {
                $rootScope.Prompt('OTP Approval', 'Enter Customer OTP', function (value) {
                    $scope.ConfirmOTP(value);
                });
            });
        }

        $scope.ConfirmOTP = function (otp) {

            if (typeof (otp) == 'undefined' || otp == '' || otp.length < 4) {
                $rootScope.Toaster.Notice('Customer discount disabled');
                $scope.Parent.CurrentBill.discountotpconfirmed = false;
                return;
            }

            var request = {
                "Item": {
                    'id': $scope.Parent.CurrentBill.customerid,
                    'mobileno': $scope.Parent.CurrentBill.phoneno,
                    'otp': otp
                },
                "SecurityToken": "String content"
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CustomerService/ConfirmOTP",
                data: srequest
            }).success(function (result) {

                if (result.Success && result.Item) {
                    $scope.Parent.CurrentBill.discountotpconfirmed = true;
                    $scope.ApplyCustomerDiscount();
                }
                else {
                    $rootScope.Toaster.Notice(result.ErrorMessage == null ? 'OTP Mismatch' : result.ErrorMessage);
                    $rootScope.Prompt('OTP Approval', 'Enter Customer OTP', function (value) {
                        $scope.ConfirmOTP(value);
                    });
                }
            });
        }


        $scope.CheckCreditFocus = function (key) {
            if (key == 'salesrefid') {
                if ($scope.system_salecollectrefemp)
                    $scope.setFocussalesrefid();
                else if ($scope.system_supportshipto)
                    $scope.setFocusshifttoid();
                else
                    $scope.setFocusremark();
            }
            else if (key == 'orderby' || key == 'orderrefid') {
                if ($scope.system_salecollectrefemp)
                    $scope.setFocusorderby();
                else
                    $scope.setFocustaxid();
            }
            else {
                $scope.setFocusremark();
            }
        }

        $scope.EditPriceEnter = function () {
            $scope.autoCoding.lastbit = '';
            if ($scope.system_b2bexclusivetax) {
                $scope.SalesDetail.etprice = $scope.GetNumber($scope.autoCoding.price);
                $scope.SalesDetail.price = $scope.GetNumber($scope.autoCoding.price);

                if (typeof ($scope.SalesDetail.taxInfo.taxsplit.findTax) == 'undefined')
                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.price, true);
                $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                $scope.SalesDetail.price = ($scope.SalesDetail.price + $scope.SalesDetail.price * $scope.SalesDetail.taxpercentage / 100).round(2);
            }
            else {
                $scope.SalesDetail.price = $scope.GetNumber($scope.autoCoding.price);
                $scope.SalesDetail.etprice = $scope.GetNumber($scope.autoCoding.price);
                if (typeof (item.taxInfo.taxsplit.findTax) == 'undefined')
                    item.taxInfo.taxsplit.findTax = $scope.findTax;
                var taxitem = item.taxInfo.taxsplit.findTax($scope.SalesDetail.price);
                item.taxpercentage = taxitem.cgst + taxitem.sgst;
                $scope.SalesDetail.etprice = (($scope.SalesDetail.price / (100 + $scope.SalesDetail.taxpercentage)) * $scope.SalesDetail.taxpercentage).round(2);
            }
            if (!$scope.SalesDetail.attributes)
                $scope.SalesDetail.attributes = {};
            $scope.SalesDetail.attributes.manualprice = true;
            if ($scope.SalesDetail.price < $scope.SalesDetail.cost) {
                $scope.SalesDetail.attributes.cost = $scope.SalesDetail.cost;
                $scope.SalesDetail.attributes.lossprice = true;
            }
            $scope.AddBillItem();
            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.multipleqty = "";
            $scope.autoCoding.lastbit = "";
            $scope.autoCoding.price = "";
            $scope.setFocusautocoding();
        }

        $scope.MultipleQtyEnter = function () {
            var ismultiqty = false;
            var multiqty = 0;
            $scope.autoCoding.lastbit = '';
            if ($scope.autoCoding.multipleqty.toString().indexOf('*') < 0) {
                var qty = $scope.GetNumber($scope.autoCoding.multipleqty);
                $scope.SalesDetail.qty = qty;
            } else if ($scope.autoCoding.multipleqty.toString().indexOf('*') > 0) {
                ismultiqty = true;
                var bitinfo = $scope.autoCoding.qty.split('*');
                var qty = $scope.GetNumber(bitinfo[0]);
                multiqty = $scope.GetNumber(bitinfo[1]);
                $scope.SalesDetail.qty = $scope.GetNumber(qty);
            }

            if (!ismultiqty) {
                $scope.AddBillItem();
            }
            else {
                var idstart = new Date().getTime();
                var saledetailcopy = (JSON.parse(JSON.stringify($scope.SalesDetail)));
                for (var i = 0; i < multiqty; i++) {
                    $scope.SalesDetail = (JSON.parse(JSON.stringify(saledetailcopy)));
                    $scope.SalesDetail.id = idstart + i;
                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                    $scope.AddBillItem();
                }
            }
            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.multipleqty = "";
            $scope.autoCoding.lastbit = "";
            $scope.setFocusautocoding();
        }

        $scope.LastBiteEnter = function () {
            var islastbit = false;
            var ismultiqty = false;
            var multiqty = 0;
            $scope.autoCoding.lastbit = '';
            if ($scope.autoCoding.qty.toString().indexOf('-') > 0) {
                var bitinfo = $scope.autoCoding.qty.split('-');
                var qty = $scope.GetNumber(bitinfo[0]);
                var lastbit = $scope.GetNumber(bitinfo[1]);
                if (qty < lastbit) {
                    $rootScope.Toaster.Error('last bit discount cant exceed actual');
                    return;
                }
                var journalqty = qty - lastbit;
                $scope.autoCoding.lastbit = journalqty.toString();
                $scope.autoCoding.qty = qty.toString();
                islastbit = true;
            } else if ($scope.autoCoding.qty.toString().indexOf('*') > 0) {
                ismultiqty = true;
                var bitinfo = $scope.autoCoding.qty.split('*');
                var qty = $scope.GetNumber(bitinfo[0]);
                multiqty = $scope.GetNumber(bitinfo[1]);
                $scope.autoCoding.qty = qty.toString();
            }

            var bqty = Math.abs($scope.GetNumber($scope.autoCoding.qty));
            $scope.SalesDetail.qty = bqty;

            if (!ismultiqty) {
                $scope.AddBillItem();
            }
            else {
                var idstart = new Date().getTime();
                var saledetailcopy = (JSON.parse(JSON.stringify($scope.SalesDetail)));
                for (var i = 0; i < multiqty; i++) {
                    $scope.SalesDetail = (JSON.parse(JSON.stringify(saledetailcopy)));
                    $scope.SalesDetail.id = idstart + i;
                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                    $scope.AddBillItem();
                }
            }

            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.lastbit = "";
            if (islastbit)
                $rootScope.Toaster.Notice('last bit added');
            $scope.setFocusautocoding();
        };

        hotkeys.add('alt+p', 'Print', function (event) {
            event.preventDefault();
            if ($rootScope.UserInfo.p_SALES.print)
                $scope.Print($scope.CurrentBill);
        });


        $scope.ApplyRewardDialog = function () {
            var dialog = ngDialog.open({
                template: 'ApplyReward',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', function ($scope, $timeout, $http, $rootScope) {
                    $scope.Parent = $scope.ngDialogData[0]
                    $scope.reward_point = 0;
                    $scope.reward_convertable = 0;
                    $scope.reward_amount = 0;
                    $scope.couponcode = '';
                    $scope.coupon_convertable = 0;
                    $scope.coupon_amount = 0;
                    $scope.coupon_value = 0;
                    $scope.reason = $scope.Parent.ApplicationSettings.appsettings.sale_customerOTP ? 'Require OTP to Claim Rewards.' : '';
                    $scope.couponid = 0;
                    $scope.reward_maxamount = 0;
                    $scope.coupon_maxamount = 0;
                    $scope.showmanual = $scope.Parent.ManualRewards;
                    $scope.ismanual = false;
                    $scope.manual_value = 0;
                    $scope.manual_amount = 0;
                    if (!$scope.showmanual) {
                        $timeout(function () {
                            $scope.setFocusapplyreward();
                            $scope.$digest();
                        }, 10);
                    }

                    $scope.Clear = function () {
                        $scope.closeThisDialog({ 'type': 'clear', 'amount': 0 });
                    }

                    $scope.ApplyReward = function ($event) {


                        if ($scope.reward_amount < 0 || ($scope.reward_amount > $scope.reward_maxamount && $scope.reward_maxamount > 0)) {
                            $rootScope.Toaster.Notice('Amount cant exceed ' + $scope.reward_maxamount);
                            return;
                        }

                        var requiredpoints = ($scope.reward_point / $scope.reward_convertable) * $scope.reward_amount;
                        if (requiredpoints < ($scope.reward_point - $scope.reward_new))
                            $scope.reward_new = 0;
                        else
                            $scope.reward_new = requiredpoints - ($scope.reward_point - $scope.reward_new);

                        if ($scope.Parent.ApplicationSettings.appsettings.sale_customerOTP) {
                            var request = {
                                "Item": {
                                    'id': $scope.Parent.CurrentBill.customerid,
                                    'mobileno': $scope.Parent.CurrentBill.phoneno
                                },
                                "SecurityToken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "CustomerService/SendOTP",
                                data: srequest
                            }).success(function (result) {
                                $rootScope.Prompt('OTP Approval', 'Enter Customer OTP', function (value) {
                                    $scope.ConfirmOTP(value);
                                });
                            });
                        }
                        else {
                            $scope.closeThisDialog({
                                'type': 'reward', 'amount': $scope.reward_amount, 'newreward': $scope.reward_new,
                                'reward': requiredpoints
                            });
                        }
                    }

                    $scope.ConfirmOTP = function (otp) {
                        var request = {
                            "Item": {
                                'id': $scope.Parent.CurrentBill.customerid,
                                'mobileno': $scope.Parent.CurrentBill.phoneno,
                                'otp': otp
                            },
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/ConfirmOTP",
                            data: srequest
                        }).success(function (result) {
                            var requiredpoints = ($scope.reward_point / $scope.reward_convertable) * $scope.reward_amount;
                            if (result.Success && result.Item)
                                $scope.closeThisDialog({ 'type': 'reward', 'amount': $scope.reward_amount, 'newreward': $scope.reward_new, 'reward': requiredpoints });
                            else
                                $rootScope.Toaster.Notice(result.ErrorMessage == null ? 'OTP Mismatch' : result.ErrorMessage);
                        });
                    }

                    $scope.ApplyCoupon = function () {
                        if ($scope.coupon_amount < 0 || ($scope.coupon_amount > $scope.coupon_maxamount && $scope.coupon_maxamount > 0)) {
                            $rootScope.Toaster.Notice('Amount cant exceed ' + $scope.coupon_maxamount);
                            return;
                        }
                        $scope.closeThisDialog({
                            'type': 'coupon',
                            'amount': $scope.coupon_amount,
                            'coupon_code': $scope.couponcode,
                            'coupon_id': $scope.couponid,
                            'coupons': $scope.coupons
                        });
                    }

                    $scope.ApplyManual = function () {
                        if ($scope.manual_point - $scope.reward_new < $scope.manual_value)
                            $scope.reward_new = $scope.manual_value - ($scope.manual_point - $scope.reward_new);
                        else
                            $scope.reward_new = 0;
                        $scope.closeThisDialog({
                            'type': 'manual', 'amount': $scope.manual_amount,
                            'rewards': $scope.manual_value, 'newreward': $scope.reward_new
                        });
                    }


                    $scope.findreward = function () {

                        $scope.receivable = $scope.Parent.CurrentBill.receivable;
                        var srequest = JSON.stringify({
                            "Item": {
                                'customerid': $scope.Parent.CurrentBill.customerid,
                                'locationid': $rootScope.LocationInfo.location.id,
                                'billvalue': $scope.Parent.CurrentBill.receivable
                            },
                            "securitytoken": "String content"
                        });

                        $http({
                            method: 'POST',
                            url: "CustomerrewardsService/SearchReward",
                            data: srequest
                        }).success(function (result) {
                            if (result != null && typeof (result) != 'undefined') {
                                $scope.reward_point = result.rewards;
                                $scope.reward_convertable = result.available;
                                $scope.reward_amount = result.convertable;
                                $scope.reward_maxamount = result.convertable;
                                $scope.manual_point = result.rewards;
                                $scope.reward_new = result.billrewards;
                                if ($scope.showmanual)
                                    $scope.setFocusmanual_point();
                            }
                        }).error(function (result) {

                        });
                    }();

                    $scope.FindCoupon = function () {
                        var srequest = JSON.stringify({
                            "Item": {
                                'acceptablelocationid': $rootScope.LocationInfo.location.id,
                                'code': $scope.couponcode,
                                'billvalue': $scope.Parent.CurrentBill.receivable,
                                'bill': $scope.Parent.CurrentBill,
                            },
                            "securitytoken": "String content"
                        });

                        $http({
                            method: 'POST',
                            url: "VouchersService/SearchCoupen",
                            data: srequest
                        }).success(function (result) {
                            if (result != null && typeof (result) != 'undefined') {

                                if ($scope.system_duplicatecoupon) {
                                    $scope.coupon_value += result.available;
                                    $scope.coupon_convertable += result.convertable;
                                    $scope.coupon_amount += result.convertable;
                                    $scope.coupon_maxamount += result.available;
                                    $scope.reason = result.reason;
                                    $scope.coupons.push({
                                        'id': result.couponid, 'code': $scope.couponcode, 'amount': result.convertable
                                    });
                                    $scope.couponcode = '';
                                }
                                else {
                                    $scope.coupon_value = result.available;
                                    $scope.coupon_convertable = result.convertable;
                                    $scope.coupon_amount = result.convertable;
                                    $scope.coupon_maxamount = result.convertable;
                                    $scope.reason = result.reason;
                                    $scope.couponid = result.couponid;
                                    $scope.setFocusapplycoupon();
                                }
                            }
                        }).error(function (result) {

                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        data.value.amount = parseFloat(data.value.amount);
                        $scope.ApplyAdditionalDiscount(parseFloat(data.value.amount), 'COUPON');
                        $scope.CurrentBill.HasCouponApplied = false;
                        if (data.value.amount == 0) {
                            if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                $scope.CurrentBill.billattributes = {};
                            $scope.CurrentBill.billattributes.mode = '';
                            $scope.CurrentBill.billattributes.discountamount = 0;
                            $scope.CurrentBill.billattributes.rewardamount = 0;
                            $scope.CurrentBill.billattributes.newreward = 0;
                            $scope.CurrentBill.billattributes.couponamount = 0;
                            $scope.CurrentBill.billattributes.couponid = 0;
                            $scope.CurrentBill.billattributes.coupons = [];
                        }
                        else {
                            if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                $scope.CurrentBill.billattributes = {};
                            $scope.CurrentBill.billattributes.rewardamount = 0;
                            $scope.CurrentBill.billattributes.newreward = 0;
                            $scope.CurrentBill.billattributes.couponamount = 0;
                            $scope.CurrentBill.billattributes.couponid = 0;
                            $scope.CurrentBill.billattributes.coupons = [];

                            $scope.CurrentBill.billattributes.mode = data.value.type;
                            $scope.CurrentBill.billattributes.discountamount = data.value.amount;
                            if (data.value.type == 'reward') {
                                $scope.CurrentBill.billattributes.rewardamount = data.value.amount;
                                $scope.CurrentBill.billattributes.newreward = data.value.newreward;
                            }
                            else if (data.value.type == 'coupon') {
                                $scope.CurrentBill.billattributes.couponamount = data.value.amount;
                                $scope.CurrentBill.billattributes.couponid = data.value.coupon_id;
                                $scope.CurrentBill.billattributes.couponcode = data.value.coupon_code;
                                $scope.CurrentBill.billattributes.coupons = data.value.coupons;
                                $scope.CurrentBill.HasCouponApplied = $scope.CurrentBill.billattributes.couponamount > 0;
                            }
                            else if (data.value.type == 'manual') {
                                $scope.CurrentBill.billattributes.manualrewardamount = data.value.amount;
                                $scope.CurrentBill.billattributes.manualrewardpoint = data.value.rewards;
                                $scope.CurrentBill.billattributes.newreward = data.value.newreward;
                            }
                        }
                    }
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                    $scope.autoCoding.code = '';
                    $scope.setFocusautocoding();
                }
            });
        };

        $scope.ApplySingleRowDiscount = function (item, type, newvalue) {
            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;

            item.receivable += (item.additionaldiscount * item.qty);
            if (type == '%') {
                item.additionaldiscount = (item.receivable * newvalue / 100 / item.qty).round(2);
                item.additionaldiscountpercentage = newvalue.round(2);
            }
            else {
                item.additionaldiscount = (newvalue / item.qty).round(2);
                item.additionaldiscountpercentage = (((item.additionaldiscount * item.qty) / (item.receivable + item.discount)) * 100).round(2);
            }

            item.receivable -= (item.additionaldiscount * item.qty).round(2);

            $.each($scope.CurrentBill.Billitems, function (k, v) {
                v.additionaldiscount = $scope.GetNumber(v.additionaldiscount);
                v.additionaldiscountpercentage = $scope.GetNumber(v.additionaldiscountpercentage);

                if (typeof (v.barcodeDetail) == 'undefined') {
                    v.barcodeDetail = {
                        'taxid': v.taxid,
                        'discount': v.qty != 0 ? (v.discount / v.qty) : 0,
                        'additionaldiscount': v.additionaldiscount,
                        'stockid': v.stockid,
                        'id': v.salablegoodsid,
                        'companyid': v.companyid
                    };

                    if (v.taxid > 0) {
                        v.taxInfo = $scope.SalesTax.find(function (arritem) {
                            return arritem.id == v.taxid;
                        });

                        if (v.taxInfo != null)
                            v.taxInfo.taxsplit.findTax = $scope.findTax;
                    }
                    v.barcodeDetail.discount = v.discount;
                    v.barcodeDetail.additionaldiscount = 0;
                }
                $scope.UpdateTax(v);
                $scope.UpdateItemInfo(v, false);
            });
            $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
        }

        $scope.GetApprovedBy = function () {
            $rootScope.Prompt('Approved By', 'Code', function (code) {
                code = code.toString();
                if (typeof (code) != 'undefined' && code.length > 0) {
                    restService.Get('EmployeeService/EmployeeCode/' + $scope.UserInfo.OrganizationID + '/' + $rootScope.LocationInfo.location.id + '/' + code.replace('.', ''), true).then(function (results) {
                        $scope.CurrentBill.approvedby = results.name;
                        if (typeof ($scope.CurrentBill.billattributes) == 'undefined' || $scope.CurrentBill.billattributes == null)
                            $scope.CurrentBill.billattributes = {};

                        $scope.CurrentBill.billattributes.approvedby = results.id;
                    });
                }
            })
        }
        $scope.ApplyAdditionalDiscount = function (newvalue) {

            $scope.PopupList = [];
            $.each($scope.CurrentBill.Billitems, function (k, v) {
                $scope.PopupList.push({
                    'core': v, 'additionaldiscount': v.additionaldiscount, 'additionaldiscountpercentage': v.additionaldiscountpercentage, 'receivable': v.receivable
                });
            });

            var avail = newvalue;
            $scope.TotalValue = 0;
            $.each($scope.PopupList, function (k, v) {
                $scope.TotalValue += v.core.receivable;
            });

            var percent = (avail / Math.abs($scope.TotalValue) * 100).round(2);
            for (var index = 0; index < $scope.PopupList.length; index++) {
                if (index == ($scope.PopupList.length - 1)) {
                    $scope.PopupList[index].additionaldiscount = (avail / $scope.PopupList[index].core.qty).round(2);
                    $scope.PopupList[index].additionaldiscountpercentage = ((($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty) / ($scope.PopupList[index].core.receivable + $scope.PopupList[index].core.discount)) * 100).round(2);
                }
                else {
                    $scope.PopupList[index].additionaldiscount = ($scope.PopupList[index].core.receivable * percent / 100 / $scope.PopupList[index].core.qty).round(2);
                    $scope.PopupList[index].additionaldiscountpercentage = ((($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty) / ($scope.PopupList[index].core.receivable + $scope.PopupList[index].core.discount)) * 100).round(2);
                    avail -= ($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty);
                }

                $scope.PopupList[index].receivable += ($scope.PopupList[index].core.additionaldiscount * $scope.PopupList[index].core.qty)
                $scope.PopupList[index].receivable -= ($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty);
                $scope.PopupList[index].receivable = $scope.PopupList[index].receivable.round(2);
            }

            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;
            $.each($scope.PopupList, function (k, v) {
                v.core.additionaldiscount = $scope.GetNumber(v.additionaldiscount);
                v.core.additionaldiscountpercentage = $scope.GetNumber(v.additionaldiscountpercentage);

                if (typeof (v.core.barcodeDetail) == 'undefined') {
                    v.core.barcodeDetail = { 'taxid': v.core.taxid };

                    if (v.core.taxid > 0) {
                        v.core.taxInfo = $scope.SalesTax.find(function (arritem) {
                            return arritem.id == v.core.taxid;
                        });

                        if (v.core.taxInfo != null)
                            v.core.taxInfo.taxsplit.findTax = $scope.findTax;
                    }
                    v.core.barcodeDetail.discount = v.core.discount / v.core.qty;
                    v.core.barcodeDetail.additionaldiscount = 0;
                    v.core.barcodeDetail.stockid = v.core.stockid;
                    v.core.barcodeDetail.id = v.core.salablegoodsid;
                    v.core.barcodeDetail.companyid = v.core.companyid;
                }
                $scope.UpdateTax(v.core);
                $scope.UpdateItemInfo(v.core, false);
            });
            $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
        }

        $scope.ShowCustomerDialog = function () {
            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',

                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', function ($scope, $timeout, $http, hotkeys, $rootScope) {

                    $scope.StateList = restService.Get('ReferencelistService/STATE', true).then(function (results) {
                        $scope.StateList = results;
                    });
                    $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
                        $scope.CityList = results;
                    });
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
                        if (!$scope.name || $scope.name == null || $scope.name == '') {
                            $rootScope.Toaster.Notice('Please enter the customer name');
                        }
                        if (!$scope.contactno || $scope.contactno == null || $scope.contactno == '') {
                            $rootScope.Toaster.Notice('Please enter the customer contact no');
                        }
                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno,
                                'registredlocationid': $rootScope.LocationInfo.location.id,
                                'locationid': $rootScope.LocationInfo.location.id, 'customertype': 2
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
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno,
                                'locationid': $rootScope.LocationInfo.location.id,
                                'registredlocationid': $rootScope.LocationInfo.location.id
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
                        //$scope.setcustomer_contact();
                    }, 10);

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {

                        var existingCustomer = $scope.Customers.find(function (item) {
                            return item.id == data.value.id;
                        });
                        if (typeof (existingCustomer) == 'undefined' || existingCustomer == null)
                            $scope.Customers.push(data.value);

                        $scope.CurrentBill.customerid = data.value.id;
                        $scope.CurrentBill.code = '';
                        $scope.CurrentBill.phoneno = data.value.mobileno;
                        $scope.CurrentBill.customername = data.value.name;
                        $scope.CurrentBill.customergst = data.value.gstno;
                        $scope.CurrentBill.customerdiscount = data.value.discount;
                        $scope.CurrentBill.ccmargin = data.value.ccmargin;
                        $scope.CurrentBill.ccloyelty = !data.value.disableloyalty && data.value.loyeltyno != null
                            && data.value.loyeltyno != '' && data.value.loyeltyno.length > 1;
                        $scope.CurrentBill.ccmarginmode = data.value.ccmarginmode;
                        $scope.CurrentBill.billdiscountinclude = data.value.billdiscountinclude;
                        $scope.CurrentBill.rewardunit = data.value.rewardunit;
                        $scope.CurrentBill.discountotp = data.value.discountotp;

                        $scope.CurrentBill.ccmarginmrp = data.value.ccmarginmrp;
                        $scope.CurrentBill.ccmarginprice = data.value.ccmarginprice;
                        $scope.CurrentBill.cccomputemode = data.value.cccomputemode;

                        if ($scope.CurrentBill.discountotp && ($scope.CurrentBill.ccmarginmode || $scope.CurrentBill.customerdiscount > 0)) {
                            $scope.CurrentBill.discountotpconfirmed = false;
                            $rootScope.Toaster.Notice('Customer OTP Require for special discounts!');
                            $scope.RequestCustomerOTP();
                        }
                        else {
                            $scope.CurrentBill.discountotpconfirmed = false;
                            $scope.CurrentBill.discountotp = false;
                            $scope.ApplyCustomerDiscount();
                        }
                    }
                    $scope.autoCoding.code = '';
                    $scope.setFocusautocoding();
                    $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                }
            });
        }

        $scope.ShowExchangeDialog = function () {
            $rootScope.Prompt('Exchange', 'Enter Barcode/IMEI', function (value) {

                restService.Get('BuybackService/Barcode/' + $rootScope.LocationInfo.location.id + '/' + $scope.qEncodeURIComponent(value)).then(function (results) {

                    if (results.id != 0) {

                        if (typeof (results.imageid) != 'undefined' && results.imageid != null && results.imageid.length > 0) {
                            $scope.currentimageid = results.imageid;
                            $scope.ShowImage = true;
                        }
                        else {
                            $scope.ShowImage = false;
                        }
                        $scope.SalesDetail.stock = 1;
                        $scope.SalesDetail.qty = -1;
                        $scope.SalesDetail.hsncode = results.hsncode;
                        $scope.SalesDetail.attributes = { 'serialno': results.serialno };
                        $scope.AddBarcode(results);
                        $scope.AddBillItem();
                        $scope.autoCoding.code = "";
                        $scope.autoCoding.qty = "";
                        $scope.autoCoding.lastbit = "";
                        $scope.setFocusautocoding();
                    }
                    else {
                        if (results.errormessage != null && results.errormessage != '') {
                            $rootScope.Toaster.Error(results.errormessage);
                        }
                        else {
                            $rootScope.Toaster.Error('Unknown issue, cant find the item');
                        }
                    }

                });

            });
        }

        $scope.ShowEMIDialog = function () {
            if ($scope.CurrentBill.receivable <= 0) return;

            var dialog = ngDialog.open({
                template: 'EMIDialog',
                data: [$scope, $scope.CurrentBill],
                width: '700px',

                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService',
                    function ($scope, $timeout, $http, hotkeys, $rootScope, restService) {

                        $scope.ParentScop = $scope.ngDialogData[0];
                        $scope.EMIScheme = typeof ($scope.ngDialogData[1].emidetail) == 'undefined' ? {} : $scope.ngDialogData[1].emidetail;
                        $scope.BillValue = $scope.ngDialogData[1].receivable;
                        $scope.EMIScheme.billvalue = $scope.ngDialogData[1].receivable;

                        $scope.SchemeChanged = function () {
                            $scope.CalculatePremium(true);
                        }

                        $scope.CalculatePremium = function (schemechanged) {
                            var scheme = $scope.EMISchemes.find(function (item) {
                                return item.id == $scope.EMIScheme.schemeid;
                            });

                            if (schemechanged && typeof (scheme) != 'undefined' && scheme != null) {
                                $scope.EMIScheme.rateofinterest = scheme.rateofinterest;
                                $scope.EMIScheme.loanproviderid = scheme.loanproviderid;
                                $scope.EMIScheme.schemename = scheme.name;
                                $scope.EMIScheme.tenure = scheme.tenure;
                                var provider = $scope.LoanProviders.find(function (item) {
                                    return item.id == $scope.EMIScheme.loanproviderid;
                                });

                                if (typeof (provider) != 'undefined' && provider != null)
                                    $scope.EMIScheme.loanprovidername = provider.name;

                                if (typeof ($scope.EMIScheme.advancemonth) == 'undefined' ||
                                    $scope.EMIScheme.advancemonth == null || $scope.EMIScheme.advancemonth < scheme.minimumadvancemonth)
                                    $scope.EMIScheme.advancemonth = scheme.minimumadvancemonth;

                                if (scheme.dbdpercentage > 0) {
                                    $scope.EMIScheme.dbdvalue = ($scope.BillValue * scheme.dbdpercentage / 100).round(0);
                                }
                                else {
                                    $scope.EMIScheme.dbdvalue = scheme.dbdvalue;
                                }
                                if (scheme.documentchargepercentage > 0) {
                                    $scope.EMIScheme.documentcharge = ($scope.BillValue * scheme.documentchargepercentage / 100).round(0);
                                }
                                else {
                                    $scope.EMIScheme.documentcharge = scheme.documentcharge;
                                }
                                $scope.EMIScheme.emicardcharge = scheme.emicardcharge;
                            }

                            var marginmoney = $scope.ParentScop.GetNumber($scope.EMIScheme.marginmoney);
                            var advanceamt = (($scope.BillValue - marginmoney) / $scope.EMIScheme.tenure * $scope.EMIScheme.advancemonth).round(0);
                            advanceamt += $scope.ParentScop.GetNumber($scope.EMIScheme.marginmoney);
                            var loanAmount = ($scope.BillValue - advanceamt);
                            var numberOfMonths = $scope.EMIScheme.tenure - $scope.EMIScheme.advancemonth;

                            var monthlyInterestRatio = ($scope.EMIScheme.rateofinterest / 100) / 12;
                            var top = Math.pow((1 + monthlyInterestRatio), numberOfMonths);
                            var bottom = top - 1;
                            var sp = top / bottom;
                            var emi = (monthlyInterestRatio <= 0 ? loanAmount / numberOfMonths : ((loanAmount * monthlyInterestRatio) * sp)).round(0);
                            var full = numberOfMonths * emi;
                            var interest = full - loanAmount;
                            var int_pge = (interest / full) * 100;

                            $scope.EMIScheme.loanamount = loanAmount;
                            $scope.EMIScheme.emiamount = emi;
                            $scope.EMIScheme.emicharges = $scope.ParentScop.GetNumber($scope.EMIScheme.dbdvalue)
                                + $scope.ParentScop.GetNumber($scope.EMIScheme.documentcharge)
                                + $scope.ParentScop.GetNumber($scope.EMIScheme.emicardcharge)
                                + $scope.ParentScop.GetNumber($scope.EMIScheme.additionalcharge);
                            - $scope.ParentScop.GetNumber($scope.EMIScheme.marginmoney);
                            $scope.EMIScheme.advanceemi = advanceamt + $scope.EMIScheme.emicharges;
                        }

                        $scope.EMISchemes = restService.Get('ReferencelistService/EMISCHEMES', true).then(function (results) {
                            $scope.EMISchemes = results;
                        });

                        $scope.LoanProviders = restService.Get('ReferencelistService/LOANPROVIDER', true).then(function (results) {
                            $scope.LoanProviders = results;
                        });

                        $scope.Select = function () {
                            $scope.CalculatePremium();
                            $scope.closeThisDialog($scope.EMIScheme);
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        $scope.CurrentBill.hasemi = true;
                        $scope.CurrentBill.emidetail = data.value;
                        if ($scope.ValidateBill()) {

                            if ($scope.AutoSettlement && (!$scope.IsSaleReturn) && (!$scope.IsBulkSale) && (!$scope.CurrentBill.iscreditbill || $scope.system_crsettlementonbill)) {
                                $scope.ShowSettlement($scope.CurrentBill);
                            }
                            else {
                                $scope.SaveBill();
                            }
                        }
                    }
                    else {
                        $scope.autoCoding.code = '';
                        $scope.setFocusautocoding();
                    }
                }
            });
        }


        $scope.TouchButton = function (action, key) {
            $scope.autoCoding.code = key;
            $scope.SearchAutoCoding();
        }

        $scope.ShowAdditionalDiscount = function () {
            var list = [];
            $.each($scope.CurrentBill.Billitems, function (k, v) {
                var product = $scope.Products.find(function (item) {
                    return item.id == v.productid;
                });

                if (typeof (product) != 'undefined' && product != null && product.discountmode == 4) {
                    //no bill level discounts.
                }
                //else if (v.receivable + $scope.GetNumber(v.additionaldiscount) <= 0)
                //{
                //    //no discount allowed for free scheme goods
                //}
                else {
                    list.push({
                        'core': v, 'additionaldiscount': v.additionaldiscount,
                        'additionaldiscountpercentage': v.additionaldiscountpercentage,
                        'etdiscount': v.etdiscount,
                        'etprice': v.etprice,
                        'receivable': v.receivable
                    });
                }
            });

            var dialog = ngDialog.open({
                template: 'AdditionalDiscountDialog',
                data: [$scope, list],
                width: '850px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', function ($scope, $timeout, $http, $rootScope) {
                    $scope.Receivable = 0;
                    $scope.PopupList = $scope.ngDialogData[1];
                    $scope.IsSaleReturn = $scope.ngDialogData[0].IsSaleReturn;
                    $scope.ShowPasscode = false;
                    $scope.ShowDesign = $scope.ngDialogData[0].system_ismfgapplication;
                    $scope.selectallitems = false;
                    $scope.CanApprove = $rootScope.UserInfo.p_CANAPPROVEDISCOUNT.view;
                    $scope.sale_requestpasscodetosms = $scope.ngDialogData[0].sale_requestpasscodetosms;
                    $scope.system_salesrounding = $scope.ngDialogData[0].system_salesrounding;
                    $scope.system_adonfinalprice = $scope.ngDialogData[0].system_adonfinalprice;
                    $scope.system_b2bexclusivetax = $scope.ngDialogData[0].system_b2bexclusivetax && $scope.ngDialogData[0].Billingmode != 2;

                    $scope.rounding_rate = $scope.ngDialogData[0].rounding_rate;
                    $scope.rounding_tax = $scope.ngDialogData[0].rounding_tax;
                    $scope.rounding_discount = $scope.ngDialogData[0].rounding_discount;
                    $scope.rounding_amount = $scope.ngDialogData[0].rounding_amount;

                    $scope.CanAskPasscode = !$scope.sale_requestpasscodetosms;
                    $scope.AD = parseFloat($rootScope.UserInfo.AD);
                    if (isNaN($scope.AD)) $scope.AD = 0;

                    $scope.TotalValue = -1;
                    $scope.RoundingValue = 0;
                    $scope.ComputeTotal = function () {
                        $scope.Receivable = 0;
                        $.each($scope.PopupList, function (k, v) {
                            $scope.Receivable += v.receivable;
                        });
                        $scope.RoundingValue = $scope.Receivable;
                        $scope.Receivable = $rootScope.RoundRupee($scope.Receivable, $scope.system_salesrounding);
                        $scope.RoundingValue = $scope.Receivable - $scope.RoundingValue;
                    }

                    $scope.SelectAllItems = function () {
                        $.each($scope.PopupList, function (k, v) {
                            v.selected = $scope.selectallitems;
                        });
                    }

                    $scope.MultipleDiscountCheck = function (item) {
                        if (item.additionaldiscountpercentage.toString().indexOf('+') >= 0) {
                            var discountparts = item.additionaldiscountpercentage.toString().split('+');
                            if (discountparts[0].length == 0) discountparts[0] = '0';
                            item.additionaldiscount = (item.core.salerate * parseFloat(discountparts[0]) / 100).round($scope.rounding_discount);
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round($scope.rounding_amount);
                            item.additionaldiscount += (item.receivable * parseFloat(discountparts[1]) / 100 / item.core.qty).round($scope.rounding_discount);
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round($scope.rounding_amount);
                            item.additionaldiscountpercentage = ((item.additionaldiscount / item.core.salerate) * 100).round($scope.rounding_discount);
                            if (!$scope.IsSaleReturn && item.receivable < 0) {
                                item.additionaldiscount = item.core.additionaldiscount;
                                item.receivable = item.core.receivable;
                                item.additionaldiscountpercentage = item.core.additionaldiscountpercentage;
                            }
                            $scope.ComputeTotal();
                        }
                    }

                    $timeout(function () {
                        $scope.setFocusdiscountpercentage();
                        $scope.ComputeTotal();
                        $scope.$digest();
                    }, 10);

                    $scope.RequestSMS = function () {
                        if ($scope.sale_requestpasscodetosms) {

                            var totaldiscount = 0;
                            var totalreceivable = 0;
                            $.each($scope.PopupList, function (k, v) {
                                if ($scope.IsSaleReturn || $rootScope.GetNumber(v.core.qty) > 0) {
                                    totaldiscount += $rootScope.GetNumber(v.core.qty) * $rootScope.GetNumber(v.additionaldiscount);
                                    totalreceivable += v.receivable;
                                }
                            });
                            totalreceivable = $rootScope.RoundRupee(totalreceivable, $scope.system_salesrounding);
                            var request = {
                                "Item": {
                                    'code': $scope.EmployeeCode,
                                    'locationid': $rootScope.LocationInfo.location.id,
                                    'permission': 'CANAPPROVEDISCOUNT',
                                    'source':
                                    {
                                        'receivable': totalreceivable,
                                        'discount': totaldiscount,
                                        'locationid': $rootScope.LocationInfo.location.id,
                                        'customerid': $scope.ngDialogData[0].CurrentBill.customerid,
                                        'customername': $scope.ngDialogData[0].CurrentBill.customername,
                                        'phoneno': $scope.ngDialogData[0].CurrentBill.phoneno,
                                        'counterid': $scope.ngDialogData[0].CurrentBill.counterid
                                    }
                                }
                            }

                            $http({
                                method: 'POST',
                                url: "UsersService/RequestPASSCODE",
                                data: JSON.stringify(request)
                            }).success(function (result) {

                                if (typeof (result) == 'undefined' || !result.success) {
                                    $rootScope.Toaster.Notice('Given code not available');
                                    return;
                                }

                                if (!result.permission.view) {
                                    $rootScope.Toaster.Notice(result.employee + " don't have permission to approve");
                                    return;
                                }

                                $rootScope.Toaster.Notice(result.employee + " approval requested for discount");
                                $scope.AD = parseFloat(result.alloweddiscount);
                                if (isNaN($scope.AD)) $scope.AD = 0;
                                $scope.CanAskPasscode = true;
                                $timeout(function () {
                                    $scope.setFocuspasscode();
                                    $scope.$digest();
                                }, 10);

                            });
                        }
                    }

                    $scope.RequestPermission = function () {

                        $http({
                            method: 'GET',
                            url: "UsersService/Passcode/" + $scope.EmployeeCode + "/" + $scope.PassCode + "/CANAPPROVEDISCOUNT/" + $rootScope.LocationInfo.location.id,
                            data: null
                        }).success(function (result) {
                            if (typeof (result) == 'undefined' || !result.success) {
                                $rootScope.Toaster.Notice('Given code not available');
                                return;
                            }

                            if (!result.permission.view) {
                                $rootScope.Toaster.Notice(result.employee + " don't have permission to approve");
                                return;
                            }

                            $rootScope.Toaster.Notice(result.employee + " approved for discount");
                            $scope.AD = parseFloat(result.alloweddiscount);
                            if (isNaN($scope.AD)) $scope.AD = 0;
                            $scope.CanApprove = true;
                            $scope.Approve();
                        });
                    }

                    $scope.Approve = function () {

                        if ($scope.GetNumber($scope.discountpercentage) > $scope.AD) {
                            var hasExcess = 0;
                            $.each($scope.GetSelectedList(), function (k, v) {
                                var adp = parseFloat(v.additionaldiscountpercentage);
                                if (isNaN(adp)) adp = 0;
                                if (adp > $scope.AD) {
                                    hasExcess++;
                                }
                            });
                            if (hasExcess > 0) {
                                $rootScope.Toaster.Notice("Your allowed discount maximum @" + $scope.AD + '%');
                                return;
                            }
                        }
                        else if ($scope.GetNumber($scope.discountpercentage) == isNaN) {
                            $rootScope.Toaster.Notice("Please enter valid discount percentage ");
                            return;
                        }
                        $scope.closeThisDialog({ 'items': $scope.PopupList, 'status': 'approved', 'rounding': $scope.RoundingValue });
                    };

                    $scope.Request = function () {
                        $scope.closeThisDialog({ 'items': $scope.PopupList, 'status': 'request', 'rounding': $scope.RoundingValue });
                    };

                    $scope.CompleteDiscount = function (option) {
                        if (option == '%' && $scope.discountpercentage.toString().indexOf('+') >= 0) {

                            var discountparts = $scope.discountpercentage.toString().split('+');
                            if (discountparts[0].length == 0) discountparts[0] = '0';
                            var totaldiscount = 0;
                            $.each($scope.GetSelectedList(), function (k, v) {
                                if ($scope.IsSaleReturn || v.core.qty > 0) {
                                    var baserate = v.core.salerate - ($scope.system_adonfinalprice ? (v.core.discount / v.core.qty) : 0);
                                    v.additionaldiscount = (baserate * parseFloat(discountparts[0]) / 100).round($scope.rounding_discount);
                                    v.receivable = (v.core.receivable + (v.core.additionaldiscount * v.core.qty)) - (v.additionaldiscount * v.core.qty);
                                    v.additionaldiscount += (v.receivable * parseFloat(discountparts[1]) / 100 / v.core.qty).round($scope.rounding_discount);
                                    v.additionaldiscountpercentage = ((v.additionaldiscount / baserate) * 100).round($scope.rounding_discount);
                                    v.receivable = (v.core.receivable + (v.core.additionaldiscount * v.core.qty)) - (v.additionaldiscount * v.core.qty);
                                    v.receivable = v.receivable.round($scope.rounding_amount);
                                    if (!$scope.IsSaleReturn && v.receivable < 0) {
                                        v.additionaldiscount = v.core.additionaldiscount;
                                        v.additionaldiscountpercentage = v.core.additionaldiscountpercentage;
                                        v.receivable = v.core.receivable;
                                    }
                                    totaldiscount += (v.additionaldiscount * v.core.qty);
                                    $scope.discountpercentage = v.additionaldiscountpercentage;
                                    v.etdiscountpercentage = v.additionaldiscountpercentage;
                                    v.etdiscount = (v.etprice * v.additionaldiscountpercentage / 100).round($scope.rounding_discount);
                                }
                            });
                            $scope.discountvalue = totaldiscount;
                            $scope.ComputeTotal();
                            return;
                        } else if (option == '%' && $scope.discountpercentage.toString().toLowerCase() == 'r') {

                            var plist = $scope.GetSelectedList();
                            for (var index = 0; index < plist.length; index++) {
                                plist[index].additionaldiscount = (plist[index].core.discount * -1 / plist[index].core.qty).round($scope.rounding_discount);
                                plist[index].additionaldiscountpercentage = plist[index].core.discountpercentage * -1;
                                plist[index].etdiscountpercentage = plist[index].core.etdiscountpercentage;
                                plist[index].etdiscount = plist[index].core.etdiscount;
                                plist[index].receivable = plist[index].core.receivable - plist[index].core.discount;
                                plist[index].receivable = plist[index].receivable.round($scope.rounding_discount);
                            }

                        }

                        if ($scope.CanApprove) {
                            $scope.Approve();
                        }
                        else {
                            $scope.setFocusemployeecode();
                        }
                    }

                    $scope.DiscountChanged = function (type, item, newvalue, oldvalue) {
                        if (newvalue.toString().indexOf('+') >= 0)
                            return;
                        if ($scope.system_b2bexclusivetax) {
                            if (type == '%') {
                                item.etdiscount = (item.core.etprice * newvalue / 100).round($scope.rounding_discount);
                                item.etdiscountpercentage = newvalue.round($scope.rounding_discount);
                            }
                            else {
                                item.etdiscount = newvalue.round($scope.rounding_discount);
                                item.etdiscountpercentage = ((item.etdiscount / item.core.etprice) * 100).round($scope.rounding_discount);
                            }
                            var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);
                            item.additionaldiscountpercentage = item.etdiscountpercentage;
                            item.additionaldiscount = (baserate * item.etdiscountpercentage / 100).round($scope.rounding_discount);
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round($scope.rounding_discount);
                        }
                        else {
                            var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);
                            if (type == '%') {
                                item.additionaldiscount = (baserate * newvalue / 100).round($scope.rounding_discount);
                                item.additionaldiscountpercentage = newvalue.round($scope.rounding_discount);
                            }
                            else {
                                item.additionaldiscount = newvalue.round($scope.rounding_discount);
                                item.additionaldiscountpercentage = ((newvalue / baserate) * 100).round($scope.rounding_discount);
                            }
                            item.etdiscountpercentage = item.additionaldiscountpercentage;
                            item.etdiscount = (item.etprice * item.additionaldiscountpercentage / 100).round($scope.rounding_discount);
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round($scope.rounding_discount);
                        }
                        if (!$scope.IsSaleReturn && item.receivable < 0) {
                            item.additionaldiscount = item.core.additionaldiscount;
                            item.additionaldiscountpercentage = item.core.additionaldiscountpercentage;
                            item.receivable = item.core.receivable;
                        }
                        $scope.ComputeTotal();
                    };

                    $scope.GetSelectedList = function () {
                        var lst = [];
                        $.each($scope.PopupList, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected)
                                lst.push(v);
                        });
                        if (lst.length == 0)
                            return $scope.PopupList;
                        return lst;
                    }

                    $scope.DirectDiscountChanage = function (type, newvalue, oldvalue) {
                        newvalue = parseFloat(newvalue);
                        if (isNaN(newvalue))
                            newvalue = 0;

                        $scope.TotalValue = 0;
                        $.each($scope.GetSelectedList(), function (k, v) {
                            if ($scope.IsSaleReturn || v.core.qty > 0) {
                                if ($scope.system_posexclusivetax) {
                                    $scope.TotalValue += v.core.qty * v.core.etprice;
                                }
                                else {
                                    $scope.TotalValue += v.core.receivable;
                                    if (!$scope.system_adonfinalprice) {
                                        $scope.TotalValue += v.core.discount;
                                    }
                                }
                            }
                        });
                        if (type == '%') {
                            var totaldiscount = 0;
                            $.each($scope.GetSelectedList(), function (k, v) {
                                if ($scope.IsSaleReturn || v.core.qty > 0) {
                                    if ($scope.system_b2bexclusivetax) {
                                        v.etdiscount = (v.core.etprice * newvalue / 100).round($scope.rounding_discount);
                                        v.etdiscountpercentage = ((v.etdiscount / v.core.etprice) * 100).round($scope.rounding_discount);
                                        var baserate = v.core.salerate - ($scope.system_adonfinalprice ? (v.core.discount / v.core.qty) : 0);
                                        v.additionaldiscountpercentage = v.etdiscountpercentage;
                                        v.additionaldiscount = (baserate * v.etdiscountpercentage / 100).round($scope.rounding_discount);
                                        v.receivable = ((v.core.receivable + (v.core.additionaldiscount)) - (v.additionaldiscount * v.core.qty)).round($scope.rounding_discount);
                                    }
                                    else {
                                        var baserate = v.core.salerate - ($scope.system_adonfinalprice ? (v.core.discount / v.core.qty) : 0);
                                        v.additionaldiscount = (baserate * newvalue / 100).round($scope.rounding_discount);
                                        v.additionaldiscountpercentage = newvalue;

                                        v.etdiscountpercentage = v.additionaldiscountpercentage;
                                        v.etdiscount = (v.etprice * v.additionaldiscountpercentage / 100).round($scope.rounding_discount);

                                        v.receivable = (v.core.receivable + (v.core.additionaldiscount * v.core.qty)) - (v.additionaldiscount * v.core.qty);
                                        v.receivable = v.receivable.round($scope.rounding_discount);
                                    }
                                    if (!$scope.IsSaleReturn && v.receivable < 0) {
                                        v.additionaldiscount = v.core.additionaldiscount;
                                        v.additionaldiscountpercentage = v.core.additionaldiscountpercentage;
                                        v.etdiscountpercentage = v.core.etdiscountpercentage;
                                        v.etdiscount = v.core.etdiscount;
                                        v.receivable = v.core.receivable;
                                    }
                                    totaldiscount += (v.additionaldiscount * v.core.qty);
                                }
                            });
                            $scope.discountvalue = totaldiscount;
                        }
                        else {
                            var avail = newvalue;
                            var percent = (avail / Math.abs($scope.TotalValue) * 100).round($scope.rounding_discount);
                            $scope.discountpercentage = percent;
                            var plist = $scope.GetSelectedList();
                            for (var index = 0; index < plist.length; index++) {
                                var item = plist[index];
                                if ($scope.IsSaleReturn || item.core.qty > 0) {
                                    if ($scope.system_b2bexclusivetax) {
                                        if (index == (plist.length - 1)) {
                                            item.etdiscount = (avail / Math.abs(item.core.qty)).round($scope.rounding_discount);
                                            item.etdiscountpercentage = ((item.etdiscount / (item.core.etprice)) * 100).round($scope.rounding_discount);
                                        }
                                        else {
                                            item.etdiscountpercentage = percent;
                                            item.etdiscount = (percent * item.core.etprice / 100).round($scope.rounding_discount);
                                            avail -= (item.etdiscount * Math.abs(item.core.qty));
                                        }
                                        var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);
                                        item.additionaldiscountpercentage = item.etdiscountpercentage;
                                        item.additionaldiscount = (baserate * item.etdiscountpercentage / 100).round($scope.rounding_discount);
                                        item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round($scope.rounding_discount);
                                    }
                                    else {
                                        var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);
                                        if (index == (plist.length - 1)) {
                                            item.additionaldiscount = (avail / Math.abs(item.core.qty)).round($scope.rounding_discount);
                                            item.additionaldiscountpercentage = ((item.additionaldiscount / (baserate)) * 100).round($scope.rounding_discount);
                                        }
                                        else {
                                            item.additionaldiscountpercentage = percent;
                                            item.additionaldiscount = ((baserate) * percent / 100).round($scope.rounding_discount);
                                            avail -= (item.additionaldiscount * Math.abs(item.core.qty));
                                        }
                                        item.etdiscountpercentage = item.additionaldiscountpercentage;
                                        item.etdiscount = (item.etprice * item.additionaldiscountpercentage / 100).round($scope.rounding_discount);
                                        item.receivable = (item.core.receivable + (item.core.additionaldiscount * item.core.qty)) - (item.additionaldiscount * item.core.qty);
                                        item.receivable = item.receivable.round($scope.rounding_amount);
                                    }
                                    if (!$scope.IsSaleReturn && item.receivable < 0) {
                                        item.additionaldiscount = item.core.additionaldiscount;
                                        item.additionaldiscountpercentage = item.core.additionaldiscountpercentage;
                                        item.etdiscountpercentage = item.core.etdiscountpercentage;
                                        item.etdiscount = item.core.etdiscount;
                                        item.receivable = item.core.receivable;
                                    }
                                }
                            }
                        }
                        $scope.ComputeTotal();
                    };

                    $scope.DiscountChanged = function (type, item, newvalue, oldvalue) {
                        if (type == '%' && newvalue.toString().indexOf('+') >= 0) {
                            return;
                        }

                        newvalue = parseFloat(newvalue);
                        if (isNaN(newvalue))
                            newvalue = 0;

                        if (oldvalue == newvalue) return;

                        var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);

                        if (type == '%') {
                            item.additionaldiscount = (baserate * newvalue / 100).round($scope.rounding_discount);
                            item.additionaldiscountpercentage = newvalue.round($scope.rounding_discount);
                        }
                        else {
                            item.additionaldiscount = newvalue.round($scope.rounding_discount);
                            item.additionaldiscountpercentage = ((item.additionaldiscount / baserate) * 100).round($scope.rounding_discount);
                        }

                        item.etdiscountpercentage = item.additionaldiscountpercentage;
                        item.etdiscount = (item.etprice * item.additionaldiscountpercentage / 100).round($scope.rounding_discount);
                        item.receivable = ((item.core.receivable + (item.core.additionaldiscount * item.core.qty)) - (item.additionaldiscount * item.core.qty)).round($scope.rounding_discount);

                        if (!$scope.IsSaleReturn && item.receivable < 0) {
                            item.additionaldiscount = item.core.additionaldiscount;
                            item.additionaldiscountpercentage = item.core.additionaldiscountpercentage;
                            item.etdiscountpercentage = item.core.etdiscountpercentage;
                            item.etdiscount = item.core.etdiscount;
                            item.receivable = item.core.receivable;
                        }
                        if ($scope.TotalValue == -1) {
                            $scope.TotalValue = 0;
                            $.each($scope.PopupList, function (k, v) {
                                $scope.TotalValue += (v.core.receivable + v.core.additionaldiscount);
                            });
                        }
                        totaldiscount = 0;
                        $.each($scope.PopupList, function (k, v) {
                            totaldiscount += (v.additionaldiscount * v.core.qty);
                        });
                        $scope.discountpercentage = (totaldiscount / $scope.TotalValue * 100).round($scope.rounding_discount);
                        $scope.discountvalue = totaldiscount;
                        $scope.ComputeTotal();
                    };

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {

                        if ($scope.system_salesrounding > 0 && data.value.rounding != 0) {
                            var maxDiscount = data.value.items[0];
                            maxDiscount.additionaldiscount = $scope.GetNumber(maxDiscount.additionaldiscount);

                            $.each(data.value.items, function (k, v) {
                                if (maxDiscount.additionaldiscount < $scope.GetNumber(v.additionaldiscount)) {
                                    maxDiscount = v;
                                    maxDiscount.additionaldiscount = $scope.GetNumber(maxDiscount.additionaldiscount);
                                }
                            });
                            maxDiscount.additionaldiscount -= data.value.rounding;
                            maxDiscount.additionaldiscount.round($scope.rounding_discount);
                        }



                        $.each(data.value.items, function (k, v) {

                            if ($scope.Billingmode != 0) {
                                if (v.etdiscount == undefined || v.etdiscount == null) {
                                    $rootScope.Toaster.Error('Exclusive discount value is missing');
                                    return;
                                }

                                if (v.etdiscountpercentage == undefined || v.etdiscountpercentage == null) {
                                    $rootScope.Toaster.Error('Exclusive discount percentage is missing');
                                    return;
                                }
                                $rootScope.Toaster.Info('Exclusive discount @ ' + v.etdiscountpercentage + '% : ' + v.etdiscount.round(2) + '\r\n' +
                                    'Additional discount @ ' + v.additionaldiscountpercentage + '% : ' + v.additionaldiscount.round(2) + '\r\n'
                                );
                            }
                            else {
                                $rootScope.Toaster.Info('Exclusive discount @ ' + v.etdiscountpercentage + '% : ' + v.etdiscount.round(2) + '\r\n' +
                                    'Additional discount @ ' + v.additionaldiscountpercentage + '% : ' + v.additionaldiscount.round(2) + '\r\n'
                                );
                            }

                            $scope.CurrentBill.total -= v.core.receivable;
                            $scope.CurrentBill.receivable -= v.core.receivable;
                            $scope.CurrentBill.discount -= v.core.discount;
                            $scope.CurrentBill.schemediscount -= v.core.schemediscount;
                            $scope.CurrentBill.additionaldiscount -= (v.core.additionaldiscount * v.core.qty);
                            $scope.CurrentBill.tax -= v.core.tax;

                            v.core.additionaldiscount = $scope.GetNumber(v.additionaldiscount).round($scope.rounding_discount);
                            v.core.additionaldiscountpercentage = $scope.GetNumber(v.additionaldiscountpercentage).round($scope.rounding_discount);
                            v.core.etdiscount = $scope.GetNumber(v.etdiscount).round($scope.rounding_discount);
                            v.core.etdiscountpercentage = $scope.GetNumber(v.etdiscountpercentage).round($scope.rounding_discount);

                            if (typeof (v.core.barcodeDetail) == 'undefined') {
                                v.core.barcodeDetail = { 'taxid': v.core.taxid, 'companyid': v.core.companyid };

                                if (v.core.taxid > 0) {
                                    v.core.taxInfo = $scope.SalesTax.find(function (arritem) {
                                        return arritem.id == v.core.taxid;
                                    });

                                    if (v.core.taxInfo != null)
                                        v.core.taxInfo.taxsplit.findTax = $scope.findTax;
                                }
                                v.core.barcodeDetail.discount = v.core.discount / v.core.qty;
                                v.core.barcodeDetail.additionaldiscount = 0;
                                v.core.barcodeDetail.stockid = v.core.stockid;
                                v.core.barcodeDetail.id = v.core.salablegoodsid;
                                v.core.barcodeDetail.companyid = v.core.companyid;
                            }
                            $scope.UpdateTax(v.core);
                            $scope.UpdateItemInfo(v.core, false);
                        });

                        $scope.CurrentBill.tax = 0;
                        $scope.CurrentBill.discount = 0;
                        $scope.CurrentBill.receivable = 0;
                        $scope.CurrentBill.rounding = 0;
                        $scope.CurrentBill.total = 0;
                        $scope.CurrentBill.additionaldiscount = 0;

                        $.each($scope.CurrentBill.Billitems, function (k, v) {
                            $scope.CurrentBill.tax += v.tax;
                            $scope.CurrentBill.discount += v.discount;
                            $scope.CurrentBill.receivable += v.receivable;
                            $scope.CurrentBill.schemediscount += v.schemediscount;
                            $scope.CurrentBill.additionaldiscount += (v.additionaldiscount * v.qty);
                        })
                        $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                        $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                        $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
                        $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                        $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                    }
                }
            });
        }

        //template: 'SettlementDialog',

        $scope.ShowSettlement = function (bill) {
            var dialog = ngDialog.open({
                template: 'SettlementDialog',
                data: [$scope, bill],
                width: '650px',
                controller: 'SettlementsController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;

                    if (typeof ($scope.CurrentBill.billattributes) == 'undefined' || $scope.CurrentBill.billattributes == null)
                        $scope.CurrentBill.billattributes = {};

                    $scope.CurrentBill.billattributes.settlement = data.value;

                    if ($scope.ValidateBill()) {
                        $scope.SaveBill();
                    }
                }
            });
        }

        $scope.SaveBill = function () {

            $scope.CurrentBill.isb2b = $scope.CurrentBill.iscreditbill ? true : false;
            var request = {
                "Item": $scope.CurrentBill
            }

            request.Item.papersize = $scope.PrinterData.PaperSize;
            request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "BillService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {

                $scope.SaveRequest = false;
                if (result.id > 0) {

                    var allsaved = true;
                    $.each(result.Bills, function (k, v) {
                        if (v.id <= 0)
                            allsaved = false;
                    });

                    if (allsaved) {
                        $scope.ErrorMessage = "Successfully saved";
                        if (print && !$scope.system_b2bprintdisable) {
                            if (result.printdocument == null || result.printdocument == '') {
                                $.each(result.Bills, function (k, v) {
                                    $scope.Print(v);
                                });
                            }
                            else {
                                $scope.Print(result);
                            }
                        }

                        $scope.ErrorMessage = "Successfully saved";
                        if ($scope.DiscountQueue && !$scope.discounttimerenabled && $scope.CurrentBill.additionaldiscount > 0) {
                            $scope.discounttimerenabled = true;
                            $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                        }

                        $scope.AddBillHistory(result, false, true)
                        $scope.CreateNew();
                        if ($scope.DiscountQueue) {
                            $scope.AllowAdditionalDiscount = false;
                        }
                    }
                    else {
                        $scope.ErrorMessage = "Can't Save the bill..";
                        if (typeof (result.errormessage) != 'undefined' &&
                            result.errormessage != null && result.errormessage != '')
                            $rootScope.Toaster.Error(result.errormessage);
                    }
                }
                else {
                    $scope.ErrorMessage = result.errormessage;
                    if (typeof (result.errormessage) != 'undefined' &&
                        result.errormessage != null && result.errormessage != '')
                        $rootScope.Toaster.Error(result.errormessage);
                }

            }).error(function (result) {
                $scope.ErrorMessage = result;
                $scope.SaveRequest = false;
            });
        }

        $scope.AdditionalChargeItem_Add = function () {
            if (typeof ($scope.AdditionalChargeItem.Charge) == 'undefined' ||
                $scope.AdditionalChargeItem.Charge == null ||
                $scope.AdditionalChargeItem.Charge == '') {
                $scope.setFocusautocoding();
                return;
            }

            if ($scope.AdditionalChargeItem.Charge.toString().endsWith('%')) {
                $scope.AdditionalChargeItem.Charge = ($scope.CurrentBill.total * $scope.GetNumber($scope.AdditionalChargeItem.Charge.toString().replace('%', '')) / 100).round(0);
                sourcevalue = $scope.AdditionalChargeItem.Charge;
                $scope.AdditionalChargeItem_Changed(false);
                return;
            }
            if ($scope.AdditionalChargeItem.taxonly) {
                $scope.AdditionalChargeItem.Charge = 0;
                $scope.AdditionalChargeItem.Amount = $scope.AdditionalChargeItem.Tax;
            }
            if ($scope.AdditionalChargeItem.chargetype == 'EXPORTGST') {
                $.each($scope.CurrentBill.additionalchargeslist, function (k, v) {
                    if (v.chargetype == 'PACKING') {
                        v.Tax = 0;
                        v.Amount = $scope.GetNumber(v.Charge);
                    }
                })
                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    if ($scope.AdditionalChargeItem.TaxPercentage == 0) {
                        v.taxpercentage = 0;
                        v.itemtax = 0;
                        v.tax = 0;
                    }
                    else {
                        v.taxpercentage = $scope.AdditionalChargeItem.TaxPercentage;
                        v.itemtax = (v.taxpercentage * (v.etprice - v.etdiscount) / 100).round($scope.rounding_tax);
                        v.tax = v.itemtax * v.qty;
                        v.price = (v.etprice - v.etdiscount) + v.itemtax;
                        v.receivable = v.price * v.qty;
                    }
                });
                $rootScope.Toaster.Error("B2B Bill converted to EXPORT bill");
                $scope.Retotal(false);
                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
            }

            $scope.AdditionalChargeItem.Charge = parseFloat($scope.AdditionalChargeItem.Charge);
            if (!isNaN($scope.AdditionalChargeItem.Charge)) {
                if ($scope.IsSaleReturn) {
                    $scope.AdditionalChargeItem.Charge = Math.abs($scope.AdditionalChargeItem.Charge) * -1;
                    $scope.AdditionalChargeItem.Tax = Math.abs($scope.AdditionalChargeItem.Tax) * -1;
                    $scope.AdditionalChargeItem.Amount = Math.abs($scope.AdditionalChargeItem.Amount) * -1;
                }
                $scope.CurrentBill.additionalchargeslist.push(JSON.parse(JSON.stringify($scope.AdditionalChargeItem)));
                $scope.ComputeAddtionalChargeValues();
                $scope.AdditionalChargeItem.Charge = "";
                $scope.AdditionalChargeItem.Tax = "";
                $scope.AdditionalChargeItem.Amount = "";
            }
            $scope.setFocustaxid();
        }

        $scope.AdditionalChargeItem_Changed = function (fromtype) {
            var sourcevalue = $scope.GetNumber($scope.AdditionalChargeItem.Charge);
            $scope.AdditionalChargeItem.Tax = 0;
            $scope.AdditionalChargeItem.Amount = 0;
            $scope.AdditionalChargeItem.TaxId = 0;
            $scope.AdditionalChargeItem.TaxPercentage = 0;
            $scope.AdditionalChargeItem.taxonly = false;
            var item = $scope.AdditionalChargeList.find(function (item) {
                return $scope.AdditionalChargeItem.ChargeID == item.id
            });
            if (typeof (item) != 'undefined' && item != null) {

                if (item.additionalinfo.paytype == 'EXPORTGST') {
                    $scope.AdditionalChargeItem.additionalchargestypeid = item.id;
                    $scope.AdditionalChargeItem.TaxId = item.referenceid;
                    $scope.AdditionalChargeItem.Name = item.name;
                    $scope.AdditionalChargeItem.chargetype = item.additionalinfo.paytype;
                    $scope.AdditionalChargeItem.taxonly = true;
                    var sourcevalue = 0;
                    $.each($scope.CurrentBill.Billitems, function (k, v) {
                        sourcevalue += ((v.etprice - v.etdiscount) * v.qty);
                    });

                    $.each($scope.CurrentBill.additionalchargeslist, function (k, v) {
                        if (v.chargetype == 'PACKING')
                            sourcevalue += $scope.GetNumber(v.Charge);
                    })

                    $scope.AdditionalChargeItem.Charge = sourcevalue;
                    var taxitem = $scope.SalesTax.find(function (titem) {
                        return titem.id == item.referenceid;
                    });
                    if (typeof (taxitem) != 'undefined' && taxitem != null) {

                        if (taxitem.taxsplit == null) {
                            taxitem.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                        }
                        taxitem.taxsplit.findTax = $scope.findTax;
                        var taxitemvalue = taxitem.taxsplit.findTax(sourcevalue);
                        $scope.AdditionalChargeItem.TaxPercentage = taxitemvalue.cgst + taxitemvalue.sgst;
                        $scope.AdditionalChargeItem.Tax = ($scope.AdditionalChargeItem.TaxPercentage * sourcevalue / 100).round($scope.rounding_tax);
                    }
                }
                else {
                    if (fromtype && item.additionalinfo.billdiscount != 0) {

                        if (item.additionalinfo.paytype == 'PACKING') {
                            var perpiece = item.additionalinfo.billdiscount;
                            $scope.AdditionalChargeItem.Charge = $scope.CurrentBill.totalqty * perpiece;
                            sourcevalue = $scope.GetNumber($scope.AdditionalChargeItem.Charge);
                        }
                        if (item.additionalinfo.paytype == 'SERVICE') {
                            var pertask = item.additionalinfo.billdiscount;
                            $scope.AdditionalChargeItem.Charge = $scope.CurrentBill.totalqty * pertask;
                            sourcevalue = $scope.GetNumber($scope.AdditionalChargeItem.Charge);
                        }
                        else {
                            if (item.additionalinfo.billdiscountinclude) {
                                $scope.AdditionalChargeItem.Charge = ($scope.CurrentBill.total * item.additionalinfo.billdiscount / 100).round(0);
                                sourcevalue = $scope.AdditionalChargeItem.Charge;
                            }
                            else {
                                $scope.AdditionalChargeItem.Charge = (($scope.CurrentBill.total + $scope.CurrentBill.additionalcharges) * item.additionalinfo.billdiscount / 100).round(0);
                                sourcevalue = $scope.AdditionalChargeItem.Charge;
                            }
                        }
                    }

                    $scope.AdditionalChargeItem.additionalchargestypeid = item.id;
                    $scope.AdditionalChargeItem.TaxId = item.referenceid;
                    $scope.AdditionalChargeItem.Name = item.name;
                    $scope.AdditionalChargeItem.chargetype = item.additionalinfo.paytype;
                    if (item.additionalinfo.paytype != 'PACKING' && item.additionalinfo.disabled) {
                        $scope.AdditionalChargeItem.TaxPercentage = item.additionalinfo.billdiscount;
                        $scope.AdditionalChargeItem.Tax = $scope.AdditionalChargeItem.Charge;
                        $scope.AdditionalChargeItem.taxonly = true;
                        sourcevalue = 0;
                    }
                    else if (!item.additionalinfo.disabled || item.additionalinfo.paytype == 'PACKING') {
                        $scope.AdditionalChargeItem.taxonly = (item.additionalinfo.paytype != 'PACKING'
                            && item.additionalinfo.paytype != 'FRIGHT' && item.additionalinfo.paytype != 'SERVICE');
                        var taxitem = $scope.SalesTax.find(function (titem) {
                            return titem.id == item.referenceid;
                        });
                        if (typeof (taxitem) != 'undefined' && taxitem != null) {

                            if (taxitem.taxsplit == null) {
                                taxitem.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                            }
                            taxitem.taxsplit.findTax = $scope.findTax;
                            var taxitemvalue = taxitem.taxsplit.findTax(sourcevalue);
                            $scope.AdditionalChargeItem.TaxPercentage = taxitemvalue.cgst + taxitemvalue.sgst;
                            $scope.AdditionalChargeItem.Tax = ($scope.AdditionalChargeItem.TaxPercentage * sourcevalue / 100).round($scope.rounding_tax);
                        }
                    }
                    else {
                        $scope.AdditionalChargeItem.taxonly = false;
                        $scope.AdditionalChargeItem.TaxPercentage = 0;
                        $scope.AdditionalChargeItem.Tax = 0;
                    }
                }
            }
            $scope.AdditionalChargeItem.Amount = (sourcevalue + $scope.AdditionalChargeItem.Tax).round(2);
        }

        $scope.ComputeAddtionalChargeValues = function () {
            $scope.CurrentBill.additionalcharges = 0;
            $.each($scope.CurrentBill.additionalchargeslist, function (k, v) {
                if (v.chargetype != 'EXPORTGST')
                    $scope.CurrentBill.additionalcharges += $scope.GetNumber(v.Amount);// + $scope.GetNumber(v.Tax);
            });
            $scope.agrid.UpdateData($scope.CurrentBill.additionalchargeslist);
        }

        $scope.AdditionalChargeItem_Remove = function (source) {

            if (source.chargetype == 'EXPORTGST') {
                var removeList = [];
                $.each($scope.CurrentBill.additionalchargeslist, function (k, v) {
                    if (v.chargetype == 'PACKING' || v.chargetype == 'SERVICE')
                        removeList.push(v);
                })

                if (removeList.length > 0) {
                    $.each(removeList, function (k, v) {
                        $scope.CurrentBill.additionalchargeslist.splice($scope.CurrentBill.additionalchargeslist.indexOf(v), 1);
                    })
                }

                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    if (v.taxInfo == undefined || v.taxInfo == null) {
                        v.taxInfo = $scope.SalesTax.find(function (arritem) {
                            return arritem.id == v.taxid;
                        });

                        if (v.taxInfo != null) {
                            if (v.taxInfo.taxsplit == null) {
                                v.taxInfo.taxsplit = { 'taxpercentage': v.taxInfo.taxpercentage };
                            }
                            v.taxInfo.taxsplit.findTax = $scope.findTax;
                        }
                    }
                });

                $scope.Retotal(true);
                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                $rootScope.Toaster.Error("EXPORT bill revert back to B2B Bill");
            }

            $scope.CurrentBill.additionalchargeslist.splice($scope.CurrentBill.additionalchargeslist.indexOf(source), 1);
            $scope.ComputeAddtionalChargeValues();
        }

        $scope.ShowDirectSettlement = function (bill) {
            var dialog = ngDialog.open({
                template: 'clientside/views/Settlements_dialog.html',
                data: [$scope, bill],
                width: '1000px',
                controller: 'SettlementsController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    $scope.setFocusautocoding();
                }
            });
        }

        $scope.GridConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.options.rowHeight = 30;
            igrid.AddSerialNo(40);
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            if (($scope.system_posscrtemplate != 3)) {
                if ($scope.system_posscrtemplate != 2) {
                    if ($scope.CurrentBillConfig.type == 'list') {
                        igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                    }

                    if ($scope.system_multiplesalemen) {
                        igrid.AddButtonColumn('SMMChange', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='SMMChange' class='btn btn-info btn-xs'><i class='fa fa-users' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                    }
                    else {
                        igrid.AddButtonColumn('SMChange', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='SMChange' class='btn btn-info btn-xs'><i class='fa fa-user' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                    }

                }

                if ($scope.CurrentBillConfig.type == 'list') {
                    var column = igrid.AddCustomColumn('SO', '', 35, function (item, col) {
                        if (item.billattributes && item.billattributes.soqty && item.billattributes.soqty > 0) {
                            if (item.billattributes.soqty < item.qty) {
                                return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='SO' class='btn btn-warning btn-xs'><i class='fa fa-info-circle' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
                            }
                            return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='SO' class='btn btn-primary btn-xs'><i class='fa fa-info-circle' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
                        }
                        return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='SO' class='btn btn-secondary btn-xs'><i class='fa fa-info-circle' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
                    });

                    column.toolTip = function (item) {
                        if (item.billattributes && item.billattributes.soqty && item.billattributes.soqty > 0) {
                            if (item.billattributes.soqty < item.qty) {
                                return 'Partial - ' + item.billattributes.soqty;
                            }
                            return "Allocated - " + item.billattributes.soqty;
                        }
                        return "No Allocation";
                    };
                }
            }

            if (!$scope.system_ismfgapplication) {
                igrid.AddSimpleColumn('displaycode', 'Barcode', 120);
                var column = igrid.AddSimpleColumn('printingname', 'Product', 180);
                column.toolTip = function (item) {
                    if (typeof (item.schemeid) == 'undefined' || item.schemeid == null || item.schemeid == 0)
                        return '';
                    return item.scheme;
                };
            }
            else {

                if ($scope.system_posscrtemplate == 3 && $scope.CurrentBillConfig.type == 'list') {
                    igrid.AddSimpleColumn('displaycode', 'Barcode', 130);
                    igrid.AddSimpleColumn('printingname', 'Product', 120);
                    var column = igrid.AddCustomColumn('Design', 'Design', 150, function (item, col) {
                        return item.attributes.groupkey + "-" + item.attributes.size;
                    });
                    column.sortable = true;
                    column.getValue = function (item, col) {
                        return item.attributes.groupkey + "-" + item.attributes.size;
                    }
                }
                else if ($scope.system_posscrtemplate == 4) {
                    igrid.AddSimpleColumn('soqty', 'SO', 40);
                    igrid.AddSimpleColumn('displaycode', 'Barcode', 130);
                    var column = igrid.AddCustomColumn('Design', 'Design', 150, function (item, col) {
                        return item.attributes.designid + "-" + item.attributes.size;
                    });
                    column.sortable = true;
                    column.getValue = function (item, col) {
                        return item.attributes.designid + "-" + item.attributes.size;
                    }
                } else {
                    //var column = igrid.AddSimpleColumn('designitemcode', 'Product', 180);
                    var column = igrid.AddSimpleColumn('printingname', 'Product', 180);
                    column.toolTip = function (item) {
                        if (typeof (item.schemeid) == 'undefined' || item.schemeid == null || item.schemeid == 0)
                            return '';
                        return item.scheme;
                    };
                }
            }
            igrid.AddSimpleColumn('taxpercentage', 'Tax %', 60).cssClass = 'rightalign';

            if (!$scope.system_salepricemrpdifference && !$scope.system_marginbasedb2b) {
                var column = igrid.AddCustomColumn('mrp', 'MRP', 60, function (item, col) {
                    if (item.attributes && item.attributes.mrp)
                        return $filter('IndianNumber')(item.attributes.mrp, 2);
                    return '';
                });
                column.cssClass = 'rightalign';
            }

            if (!$scope.system_ismfgapplication) {

                if ($scope.Billingmode == 4) {
                    var column = igrid.AddCustomColumn('cost', 'Cost', 60, function (item, col) {
                        if ($scope.system_b2bexclusivetax) {
                            if (item.attributes.margincost == undefined)
                                item.attributes.margincost = item.cost;
                            return $filter('IndianNumber')(item.attributes.margincost, 2);
                        }
                        else {
                            if (item.attributes.margincost == undefined)
                                item.attributes.margincost = item.cost + (item.cost * item.taxpercentage / 100).round(2);
                            return $filter('IndianNumber')(item.attributes.margincost, 2);
                        }
                    });
                    column.cssClass = 'rightalign';
                }
                else {
                    if (!$scope.system_hidecost && $scope.system_supportcostedit) {
                        igrid.AddColumn(
                            {
                                id: "cost", name: "Cost", field: "cost",
                                width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                                'sortable': true
                            }
                        );
                    }
                    else if (!$scope.system_hidecost) {
                        var column = igrid.AddFormatedColumn('cost', 'Cost', 70, 'N2', 2);
                        column.cssClass = 'rightalign';
                    }
                }
            }

            var pricecolumnheader = !$scope.system_supportpostaxtype ? 'Price' : ($scope.system_b2bexclusivetax ? 'Price(ET)' : 'Price(IT)');

            if ($scope.system_marginbasedb2b) {
                var column = igrid.AddSimpleColumn('Margin', 'Margin', 70);
                column.editor = Slick.Editors.Text;
                column.getValue = function (item, col) {
                    if (item.attributes && item.attributes != null) {
                        return item.attributes.margin;
                    }
                    return '-';
                };
                column.formatter = function (row, cell, searchValue, columnDef, dataContext) {
                    if (dataContext.attributes && dataContext.attributes != null) {
                        if (dataContext['Margin'] == undefined || dataContext['Margin'] == null || dataContext['Margin'] == '') {
                            if ($scope.system_b2bexclusivetax) {
                                dataContext.attributes.margin = (($scope.GetNumber(dataContext.etprice) / dataContext.cost * 100) - 100).round(2);
                                if ($scope.SupportMarkDOWN)
                                    dataContext.attributes.margin = (100 - ((dataContext.cost / dataContext.etprice) * 100)).round(2);
                                dataContext.attributes.margincost = dataContext.cost;
                            }
                            else {
                                dataContext.attributes.margincost = dataContext.cost + (dataContext.cost * dataContext.taxpercentage / 100).round(2);
                                dataContext.attributes.margin = (($scope.GetNumber(dataContext.price) / dataContext.attributes.margincost * 100) - 100).round(2);
                                if ($scope.SupportMarkDOWN)
                                    dataContext.attributes.margin = (100 - ((dataContext.attributes.margincost / dataContext.price) * 100)).round(2);
                            }
                            dataContext['Margin'] = dataContext.attributes.margin;
                        }
                        return dataContext.attributes.margin;
                    }
                    return '-';
                }
                column.cssClass = 'rightalign';
            }

            if ($scope.system_b2bexclusivetax) {
                if ($scope.system_supportpriceedit &&
                    $rootScope.UserInfo.p_PRICEEDITONB2BPOS.view) {
                    var column = igrid.AddColumn(
                        {
                            id: "etprice", name: pricecolumnheader, field: "etprice",
                            width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    );
                    column.cssClass = 'rightalign';
                }
                else {
                    igrid.AddFormatedColumn('etprice', pricecolumnheader, 70, 'N2', 2);
                }
            }
            else {
                if ($scope.system_supportpriceedit &&
                    $rootScope.UserInfo.p_PRICEEDITONB2BPOS.view) {
                    var column = igrid.AddColumn(
                        {
                            id: "salerate", name: pricecolumnheader, field: "salerate",
                            width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    );
                    column.cssClass = 'rightalign';
                }
                else {
                    igrid.AddFormatedColumn('salerate', pricecolumnheader, 70, 'N2', 2);
                }
            }

            if ($scope.system_salepricemrpdifference && !$scope.system_marginbasedb2b) {
                var column = igrid.AddCustomColumn('mrp', 'MRP', 60, function (item, col) {
                    if (item.attributes && item.attributes.mrp)
                        return $filter('IndianNumber')(item.attributes.mrp, 2);
                    return '';
                });
                column.cssClass = 'rightalign';
            }


            var enabletotal = false;
            if ($scope.system_ismfgapplication) {

                if ($scope.system_possizelist != null &&
                    $scope.system_possizelist != '') {
                    igrid.AddFormatedColumn('soqty', 'SO', 50, 'N2', 0);
                    var slist = $scope.system_possizelist.split(',');
                    enabletotal = true;
                    $.each(slist, function (k, v) {
                        var column = igrid.AddCustomColumn('size_' + k, v, 30, function (item, col) {
                            if (item.list && item.list.length > 0) {
                                var qty = 0;
                                $.each(item.list, function (k, v) {
                                    if (v.attributes && v.attributes.size == col.size) {
                                        qty += v.qty;
                                    }
                                })
                                if (qty != 0)
                                    return $filter('IndianNumber')(qty, 0)
                            }
                            return '';
                        });
                        column.cssClass = 'rightalign';
                        column.size = v;
                        column.sortable = true;
                        column.total = true;
                        column.filtertype = 'number';
                        column.getValue = function (item, col) {
                            if (item.list && item.list.length > 0) {
                                var qty = 0;
                                $.each(item.list, function (k, v) {
                                    if (v.attributes && v.attributes.size == col.size) {
                                        qty += v.qty;
                                    }
                                })
                                return qty;
                            }
                            return 0;
                        }
                    })
                }
                else if ($scope.system_posscrtemplate != 3 && $scope.system_posscrtemplate != 4) {
                    igrid.AddFormatedColumn('soqty', 'SO', 50, 'N2', 0);
                }
                else if ($scope.system_posscrtemplate = 4) {
                    var column = igrid.AddCustomColumn('qty', 'Qty', 60, function (item, col) {
                        return item.journalqty && item.journalqty != 0 ? $filter('IndianNumber')(item.qty, 2) + "(" +
                            $filter('IndianNumber')(item.journalqty, 2) + ")" : $filter('IndianNumber')(item.qty, 2);
                    });
                    column.cssClass = 'rightalign';
                }
                else {
                    var column = igrid.AddCustomColumn('qty', 'Qty', 60, function (item, col) {
                        return item.journalqty && item.journalqty != 0 ? $filter('IndianNumber')(item.qty, 2) + "(" +
                            $filter('IndianNumber')(item.journalqty, 2) + ")" : $filter('IndianNumber')(item.qty, 2);
                    });
                    column.cssClass = 'rightalign';
                }
            }
            else {
                var column = igrid.AddCustomColumn('qty', 'Qty', 60, function (item, col) {
                    return item.journalqty && item.journalqty != 0 ? $filter('IndianNumber')(item.qty, 2) + "(" +
                        $filter('IndianNumber')(item.journalqty, 2) + ")" : $filter('IndianNumber')(item.qty, 2);
                });
                column.cssClass = 'rightalign';
            }

            var column = igrid.AddFormatedColumn('discount', 'Discount', 60, 'N2', 2);
            column.cssClass = 'rightalign';

            if ($scope.system_showexclusivetaxonpos && $scope.system_b2bexclusivetax) {
                var column = igrid.AddCustomColumn('etdiscount', 'A.D', 60, function (item, col) {
                    var val = parseFloat(item.etdiscount);
                    if (isNaN(val)) val = 0;
                    if (val != 0) return $filter('IndianNumber')(val, 2);
                    return '';
                });
                column.cssClass = 'rightalign';
                column.total = true;
            }
            else {
                var column = igrid.AddCustomColumn('adddiscount', 'A.D', 60, function (item, col) {
                    var val = (item.qty * item.additionaldiscount) + item.schemediscount;
                    if (isNaN(val)) val = 0;
                    if (val != 0) return $filter('IndianNumber')(val, 2);
                    return '';
                });
                column.cssClass = 'rightalign';
            }

            var column = igrid.AddCustomColumn('discountpercentage', '%', 60, function (item, col) {
                var val = item.discountpercentage + item.schemediscountpercentage + item.additionaldiscountpercentage;
                if (val != 0) return $filter('IndianNumber')(val, 2);
                return '';
            });
            column.cssClass = 'rightalign';

            if ($scope.system_b2bexclusivetax && $scope.system_showexclusivetaxonpos) {

                var column = igrid.AddCustomColumn('etgross', 'Amount', 80, function (item, col) {
                    var val = item.etprice * item.qty;
                    if (val != 0) return $filter('IndianNumber')(val, 2);
                    return '';
                });
                column.getValue = function (item, col) {
                    return item.etprice * item.qty;
                }
                column.total = true;
                column.cssClass = 'rightalign';

            }
            else {
                var column = igrid.AddFormatedColumn('receivable', 'Amount', 80, 'N2', 2);
                column.total = true;
                column.cssClass = 'rightalign';
            }

            if ($scope.system_posscrtemplate != 4)
                igrid.AddSimpleColumn('salesmanname', 'SM', 120);

            if ($scope.system_b2bposfields &&
                $scope.system_b2bposfields != null &&
                $scope.system_b2bposfields != '') {
                $.each($scope.system_b2bposfields.split(','), function (k, v) {
                    if (v == 'size') {
                        var column = igrid.AddCustomColumn('b2bfield_size', 'Size', 80, function (item, col) {
                            if (item.attributes && item.attributes.size &&
                                item.attributes.size != null)
                                return item.attributes.size;
                            return '';
                        });
                    }

                    if (v == 'design') {
                        var column = igrid.AddCustomColumn('b2bfield_design', 'Design', 80, function (item, col) {
                            if (item.attributes && item.attributes.designid &&
                                item.attributes.designid != null)
                                return item.attributes.designid;
                            return '';
                        });
                    }
                    if (v == 'brand') {
                        var column = igrid.AddCustomColumn('b2bfield_brand', 'Brand', 80, function (item, col) {
                            if (item.attributes && item.attributes.brand &&
                                item.attributes.brand != null)
                                return item.attributes.brand;
                            return '';
                        });
                    }
                    if (v.startsWith('LASTPRICELIST')) {
                        $scope.LastPriceListDays = 60;
                        var parts = v.split(':');
                        if (parts.length == 2) {
                            $scope.LastPriceListDays = parseInt(parts[1]);
                            if (isNaN($scope.LastPriceListDays)) $scope.LastPriceListDays = 60;
                        }
                        var buttons = igrid.AddButtonColumn('LASTPRICELIST', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-info btn-xs'><i class='fa fa-list-ol' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                        igrid.columns.splice(igrid.columns.length - 1, 1, 0);
                        igrid.columns.splice(2, 0, buttons);

                    }
                });
            }

            igrid.options.autoEdit = true;
            if (enabletotal) {
                igrid.EnableTotal();
            }
            igrid.Initialize();
            igrid.setCellCSS('igridbigrows');
            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData().getItem(row);
                if (rowdata.schemeid && rowdata.schemeid > 0)
                    rowdata.style = 'igridbigrowswithmark';
                else
                    rowdata.style = rowdata.attributes &&
                        (rowdata.attributes.lossprice || rowdata.attributes.somissing) ?
                        'igridbigrowswithred' : (rowdata.attributes.somismatch ? 'igridbigrowswithmark' : '');
                return { 'cssClasses': 'igridbigrows' };
            }

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {

                    if ($scope.CurrentBillConfig.type == 'list' && command == 'LASTPRICELIST') {
                        $scope.ShowLastSales(item);
                    }
                    if ($scope.CurrentBillConfig.type != 'list' && command == 'Delete') {
                        $scope.DeleteGroupItem('Delete', item);
                    }
                    else if ($scope.CurrentBillConfig.type == 'list' && command == 'SO') {
                        $scope.ShowSOAllocation(item, function () {
                            $timeout(function () {
                                igrid.grid.invalidateRow(cell.row);
                                igrid.grid.render();
                            }, 10);
                        });
                    }
                    else {
                        $scope.ManageBillItem(command, item);
                    }
                });
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);

                if (args.grid.getColumns()[args.cell].id == 'Margin') {
                    item.attributes.margin = item[args.grid.getColumns()[args.cell].id];
                    $scope.MarginChanged(item, item.attributes.margin);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'cost') {
                    var lastcost = item.attributes.cost;
                    item.attributes.cost = item.cost;
                    $scope.CostChanged(item, item.attributes.cost, lastcost);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'salerate') {
                    $scope.PriceChanged(item, item.salerate, item.salerate);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'etprice') {
                    $scope.PriceChanged(item, item.etprice, item.etprice);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            igrid.grid.onDblClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'taxpercentage') {
                    $scope.ChangeTaxChange(item);
                }
                if (command.startsWith('size_')) {
                    var column = igrid.grid.getColumns()[cell.cell];
                    if (item.list && item.list != null && item.list.length > 0) {
                        var eitem = item.list.find(function (si) {
                            return si.attributes && si.attributes.size == column.size;
                        });
                        if (eitem && eitem != null) {

                            $rootScope.Prompt('Edit', 'Qty', function (count) {
                                var totalcount = $rootScope.GetNumber(count);

                                $timeout(function () {

                                    $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - eitem.qty).round(2);
                                    $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - eitem.piece).round(2);

                                    $scope.CurrentBill.tax = ($scope.CurrentBill.tax - eitem.tax).round(5);
                                    $scope.CurrentBill.discount = ($scope.CurrentBill.discount - eitem.discount).round(2);
                                    $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - eitem.schemediscount).round(2);
                                    $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - eitem.receivable).round(2);
                                    $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (eitem.additionaldiscount * eitem.qty)).round(2);

                                    eitem.discount = (item.discount / eitem.qty) * totalcount;
                                    eitem.schemediscount = (item.schemediscount / eitem.qty) * totalcount;
                                    eitem.qty = totalcount;
                                    if (eitem.sellingmode == 3)
                                        eitem.piece = 1;
                                    else
                                        eitem.piece = totalcount;

                                    $scope.CurrentBill.schemediscount += eitem.schemediscount;
                                    $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + eitem.piece).round(2);
                                    $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + eitem.qty).round(2);
                                    $scope.UpdateItemInfo(eitem, false);
                                    $scope.SOManager.UpdateItem(eitem, function () {
                                        $scope.SOManager.UpdateOrder($scope.CurrentBillConfig.groupList());
                                        if (!$scope.BeginGridTransaction) {
                                            if ($scope.igrid != null && $scope.CurrentBillConfig.items != null)
                                                $scope.igrid.UpdateData($scope.CurrentBillConfig.items, '_id');
                                        }
                                        $scope.LocalContext.Post('$scope.CurrentBill', $scope.CurrentBill, false);
                                    });
                                    $scope.$digest();
                                }, 10);

                            }, eitem.qty);

                        }
                    }
                }
            });

            $scope.igrid = igrid;
            $scope.igrid.UpdateData([]);
            $scope.AdditionalChargeGridConfig();
            $scope.setFocusautocoding();
        }


        $scope.ShowTaxTypeBox = function () {

            if ($scope.CurrentBillConfig.items &&
                $scope.CurrentBillConfig.items != null &&
                $scope.CurrentBillConfig.items.length > 0) {
                $rootScope.Toaster.Error("Please remove all scanned items before change tax mode");
                return;
            }
            var dialog = ngDialog.open({
                template: 'ChooseTaxType',
                data: {
                    'scope': $scope
                },
                width: '300px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.ReportList = null;
                    $scope.ParentScope = $scope.ngDialogData.scope;
                    restService.Get('ConfigurationService/ReportList?id=' + ($scope.ngDialogData.scope.IsSaleReturn ? 'SALERETURNINVOICE' : 'SALEINVOICE'), true).then(function (results) {
                        $scope.ReportList = results;
                    });
                    $scope.Selected = function (computationmethod) {
                        if ($scope.printingformatid == undefined ||
                            $scope.printingformatid == null) {
                            $scope.ParentScope.Toaster.Info('Please select default printing format');
                            return;
                        }
                        $scope.closeThisDialog({
                            'computationmethod': computationmethod,
                            'printingtemplateid': $scope.printingformatid
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.Billingmode = data.value.computationmethod;
                        $scope.DefaultPrintingTemplateID = data.value.printingtemplateid;
                        $scope.Billingmodename = $scope.BillModes[$scope.Billingmode - 1].name;
                        switch (data.value.computationmethod) {
                            case 1:
                                $scope.system_b2bexclusivetax = false;
                                break;
                            case 2:
                                $scope.system_b2bexclusivetax = true;
                                $scope.system_retailpriceasb2bexclusive = true;
                                break;
                            case 3:
                                $scope.system_b2bexclusivetax = true;
                                break;
                            case 4:
                                $scope.system_b2bexclusivetax = false;
                                $scope.system_marginbasedb2b = true;
                                break;
                            case 5:
                                $scope.system_b2bexclusivetax = true;
                                $scope.system_marginbasedb2b = true;
                                break;
                        }
                        $scope.GridConfig();
                    } else {
                        $scope.ShowTaxTypeBox();
                    }
                }
            });
        }

        $scope.SelectMultipleSalesmen = function (item, allitems) {
            var dialog = ngDialog.open({
                template: 'ChooseSalesMan',
                data: {
                    'scope': $scope,
                    'item': item,
                    'allitems': allitems
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.supervisorid = $scope.ngDialogData.item.salesmanid;
                    $scope.EmployeeList = $scope.ngDialogData.scope.EmployeeList;
                    $scope.SearchRef = $scope.ngDialogData.scope.SearchRef;
                    $scope.allitems = $scope.ngDialogData.allitems;
                    $scope.salesmenid = 0;
                    $scope.salesmanlist = [];
                    if (!$scope.allitems) {
                        if ($scope.ngDialogData.item.attributes && $scope.ngDialogData.item.attributes != null &&
                            $scope.ngDialogData.item.attributes.smcodes && $scope.ngDialogData.item.attributes.smcodes != null)
                            $scope.salesmanlist = $scope.ngDialogData.item.attributes.smcodes;
                        else
                            $scope.ngDialogData.item.attributes.smcodes = $scope.salesmanlist;
                    }

                    $scope.Selected = function () {
                        if ($scope.allitems) {
                            var CurrentBill = $scope.ngDialogData.scope.CurrentBill;
                            var supervisor = $scope.EmployeeList.find(function (itm) {
                                return itm.id == $scope.supervisorid;
                            });
                            if (supervisor) {
                                for (var i = 0; i < CurrentBill.Billitems.length; i++) {
                                    CurrentBill.Billitems[i].salesmancode = supervisor.code.split(' - ')[0];
                                    CurrentBill.Billitems[i].salesmanname = supervisor.name;
                                    CurrentBill.Billitems[i].salesmanid = supervisor.id;
                                    if (CurrentBill.Billitems[i].attributes == undefined ||
                                        CurrentBill.Billitems[i].attributes == null)
                                        CurrentBill.Billitems[i].attributes = {};
                                    CurrentBill.Billitems[i].attributes.smcodes = (JSON.parse(JSON.stringify($scope.salesmanlist)));
                                }
                            }
                        }
                        else {
                            $scope.ngDialogData.item.salesmanid = $scope.supervisorid;
                        }
                        $scope.closeThisDialog(true);
                    };

                    $scope.ShowEmployee = function (id, list) {
                        var employee = $scope.EmployeeList.find(function (itm) {
                            return itm.id == id;
                        });
                        if (employee && employee != null) {
                            return employee.code;
                        }
                        return '-';
                    }

                    $scope.AddSalesmen = function (salesmencode) {
                        if (salesmencode && salesmencode != null && salesmencode != '') {

                            if (salesmencode == '.' || salesmencode == '0') {
                                $scope.Selected();
                                return;
                            }

                            var code = salesmencode.replaceAll('.', '');
                            var employee = $scope.EmployeeList.find(function (itm) {
                                return itm.code.startsWith(code + ' - ');
                            });
                            if (employee && employee != null) {
                                if (salesmencode.startsWith('..')) {
                                    $scope.supervisorid = employee.id;
                                } else {
                                    $scope.salesmanlist.push(employee.id);
                                    $scope.salesmenid = null;
                                }
                                $scope.salesmencode = '';
                            }
                        }
                        else {
                            if ($scope.salesmenid && $scope.salesmenid != null && $scope.salesmenid > 0) {
                                $scope.salesmanlist.push($scope.salesmenid);
                                $scope.salesmenid = null;
                            }
                        }
                    }
                    $scope.DeleteSM = function (itm, arrlist) {
                        arrlist.splice(arrlist.indexOf(itm), 1);
                    }

                    $timeout(function () {
                        $scope['setFocusmultisalesmencode']();
                    }, 50);

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                    }
                }
            });
        }

        $scope.ChangeTaxChange = function (item) {
            var dialog = ngDialog.open({
                template: 'ChooseTax',
                data: {
                    'scope': $scope,
                    'item': item
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.taxid = $scope.ngDialogData.item.taxid;
                    $scope.SalesTax = $scope.ngDialogData.scope.SalesTax;
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'taxid': $scope.taxid
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var selectedtax = $scope.SalesTax.find(function (taxitem) {
                            return taxitem.id == data.value.taxid;
                        });
                        if (selectedtax && selectedtax != null) {
                            item.taxid = selectedtax.id;
                            if (item.barcodeDetail)
                                item.barcodeDetail.taxid = item.taxid;
                            item.taxInfo = selectedtax;
                            if (item.taxInfo != null) {
                                if (item.taxInfo.taxsplit == null) {
                                    item.taxInfo.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                                }
                                item.taxInfo.taxsplit.findTax = $scope.findTax;
                            }
                            var taxitem = item.taxInfo.taxsplit.findTax(item.baseprice);
                            item.taxpercentage = taxitem.cgst + taxitem.sgst;
                            item.taxname = item.taxInfo.name;
                            $scope.UpdateTax(item);
                            $scope.UpdateItemInfo(item, true);
                            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                        }
                    }
                    if (typeof (callback) != 'undefined' && callback != null)
                        callback();
                }
            });
        }

        $scope.AdditionalChargeGridConfig = function () {
            if ($scope.agrid != null)
                $scope.agrid.destroy();

            var igrid = new InfyGrid('additionalchargelist', '450px', '100%', true, false, true,
                $scope.system_salecollectrefemp ? 390 : 330);
            igrid.options.rowHeight = 30;
            igrid.AddSimpleColumn('Name', 'Type', 120);
            igrid.AddFormatedColumn('Charge', 'Taxable', 60, 'N2', 2);
            igrid.AddFormatedColumn('Tax', 'Tax', 50, 'N2', 2);
            igrid.AddFormatedColumn('Amount', 'Amount', 70, 'N2', 2);

            //var commands = "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='X' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete') {
                    $scope.$apply(function () {
                        $scope.AdditionalChargeItem_Remove(item);
                        $timeout(function () {
                            igrid.grid.invalidateRow(cell.row);
                            igrid.grid.render();
                        }, 10);
                    });
                }
            });

            $scope.agrid = igrid;
            $scope.agrid.UpdateData([]);
        }

        $scope.PrinterData = null;

        $timeout(function () {

            $scope.PrinterData = localStorageService.get('PrinterData');
            if ($scope.PrinterData == null) {
                $scope.PrinterData =
                {
                    'server': null,
                    'printer': null,
                    'company': null,
                    'printerid': null,
                    'service': null,
                    'report': null,
                    'IsPDF': false,
                    'IsDirectDos': false,
                    'PaperSize': 0,
                    'IsDesign': false,
                    'isandroid': false,
                    'printertype': '',
                    'printerinfo': null
                };
            }

            $scope.GridConfig();
            $scope.setFocusautocoding();
            $rootScope.slideToggeler.Toggle(true);

            var value = $scope.LocalContext.Get('Source', false);
            if (typeof (value) != 'undefined' && value == null) {
                $scope.CurrentSource = value;
            }

            $scope.$on(
                "$destroy",
                function (event) {
                    if ($scope.UpdateOffertimeout != null)
                        $timeout.cancel($scope.UpdateOffertimeout);
                }
            );

        }, 10);


        $scope.suggest_selected = null;

        function suggest_state(term) {
            $scope.suggest_selected = null;
            var q = term.toUpperCase().trim();
            var results = [];

            if (q.length < 3 || typeof ($scope.BillKeys) == 'undefined' || $scope.BillKeys == null || $scope.BillKeys.length == 0)
                return results;

            for (var i = 0; i < $scope.BillKeys.length && results.length < 10; i++) {
                var state = $scope.BillKeys[i];
                if (state.name.toUpperCase().indexOf(q) === 0)
                    results.push({ label: $sce.trustAsHtml(state.name.toUpperCase()), value: state.name.toUpperCase(), obj: state });
            }
            return results;
        }

        $scope.HSNLimit = {
            max: 0,
            init: function (length) {
                this.max = length
            },
            validate: function (items, newitem) {
                if (this.max <= 0) return true;
                var hsnlist = [newitem.hsncode];
                $.each(items, function (k, v) {
                    if (hsnlist.indexOf(k.hsncode) < 0)
                        hsnlist.push(k.hsncode);
                });
                return hsnlist.length <= this.max;
            }
        }

        $scope.autocomplete_options = {
            suggest: suggest_state,
            on_select: function (selected) {
                $scope.suggest_selected = selected.obj;
            }
        };
        $scope.openNav = function () {
            document.getElementById("mySidenav").style.width = "50px";
        }
        $scope.closeNav = function () {
            document.getElementById("mySidenav").style.width = "0";
        }


        $scope.ShowLastSales = function (item) {
            restService.Get('SalablegoodsService/SearchCustomerHistory/' + $scope.CurrentBill.customerid +
                '/' + item.productid + '/' + $scope.LastPriceListDays, true).then(function (results) {
                    $scope.ShowCustomerHistory(results, item.productid, $scope.CurrentBill.customerid, $scope.LastPriceListDays);
                });
        }

        $scope.ShowCustomerHistory = function (items, productid, customerid, days) {


            $.each(items, function (k, v) {
                v.billedon = $filter('date')(v.billedon, 'dd-MM-yyyy');
                v.invoicedate = $filter('date')(v.invoicedate, 'dd-MM-yyyy');
            })

            var dialog = ngDialog.open({
                template: 'CustomerBillHistory',
                data: {
                    'scope': $scope,
                    'items': items,
                    'productid': productid,
                    'customerid': customerid,
                    'days': days
                },
                width: '650px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.items = $scope.ngDialogData.items;
                    $scope.days = $scope.ngDialogData.days;
                    $scope.Selected = function () {

                        restService.Get('SalablegoodsService/SearchCustomerHistory/' + $scope.ngDialogData.customerid +
                            '/' + $scope.ngDialogData.productid + '/' + $scope.days, true).then(function (results) {
                                $scope.items = results;

                                $.each(results, function (k, v) {
                                    v.billedon = $filter('date')(v.billedon, 'dd-MM-yyyy');
                                    v.invoicedate = $filter('date')(v.invoicedate, 'dd-MM-yyyy');
                                })

                            });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                }
            });
        }

        $scope.CurrentSource = { 'remember': false, 'source': 'BARCODE', 'from': 'BARCODE' };
        $scope.ShowSearchMode = function () {

            if ($scope.CurrentSource == undefined || $scope.CurrentSource == null)
                $scope.CurrentSource = { 'remember': false, 'source': 'BARCODE', 'from': 'BARCODE' };

            var dialog = ngDialog.open({
                template: 'SourceDialog',
                data: [$scope],
                width: '700px',
                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService',
                    function ($scope, $timeout, $http, hotkeys, $rootScope, restService) {
                        $scope.ParentScope = $scope.ngDialogData[0];
                        $scope.TypeList = [{ 'id': 'BARCODE', 'name': 'Barcode' },
                            { 'id': 'BILLNO', 'name': 'Bill No' },
                            { 'id': 'LRENTRYNO', 'name': 'LR Entry No' },
                            { 'id': 'SOBARCODE', 'name': 'SO Barcode' },
                            { 'id': 'DESIGN/SIZESET/COLOUR', 'name': 'DESIGN/SIZESET/COLOUR' },
                            { 'id': 'GLNEAN', 'name': 'EAN/GLN'}];
                        $scope.remember = false;
                        $scope.ShowInvoices = false;
                        $scope.selectedtype = '';
                        $scope.SeletedInvoice = null;
                        $timeout(function () {
                            $scope.selectedtype = $scope.ParentScope.CurrentSource.source;
                        }, 10)


                        $scope.InvoiceSelected = function () {
                            $scope.InvoiceBarcodeList = null;
                            $scope.SeletedInvoice = $scope.InvoiceList.find(function (item) {
                                return item.id == $scope.invoiceid;
                            });
                            $scope.InvoiceBarcodeList = $scope.SeletedInvoice.stockitems;
                        }

                        $scope.ImportInvoices = function () {
                            if ($scope.InvoiceBarcodeList &&
                                $scope.InvoiceBarcodeList != null) {
                                $scope.ParentScope.CurrentSource.from = $scope.ParentScope.CurrentSource.source;
                                $scope.ParentScope.CurrentSource.source = $scope.selectedtype;
                                $scope.ParentScope.CurrentSource.remember = $scope.remember;
                                $scope.closeThisDialog($scope.SeletedInvoice);
                            }
                        }

                        $scope.ShowInvoices = false;
                        $scope.SearchBill = function () {
                            $scope.ShowInvoices = false;
                            $scope.InvoiceBarcodeList = null;
                            var request = {
                                'item': {
                                    'invoiceno': $scope.billno,
                                    'searchtype': $scope.selectedtype,
                                    'locationid': $rootScope.LocationInfo.location.id
                                }
                            };
                            $http({
                                method: 'POST',
                                url: "LrinvoiceService/SupplierInvoiceBarcodes",
                                data: JSON.stringify(request)
                            }).success(function (availableitems) {
                                if (availableitems &&
                                    availableitems != null &&
                                    availableitems.length > 0) {
                                    $scope.ShowInvoices = true;
                                    $scope.InvoiceList = availableitems;
                                }
                            });
                        }

                        $scope.Selected = function () {
                            $scope.ParentScope.CurrentSource.from = $scope.ParentScope.CurrentSource.source;
                            $scope.ParentScope.CurrentSource.source = $scope.selectedtype;
                            $scope.ParentScope.CurrentSource.remember = $scope.remember;
                            $scope.closeThisDialog();
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    $scope.autoCoding.code = '';
                    $scope.setFocusautocoding();
                    $scope.LocalContext.Post('Source', $scope.CurrentSource, false);
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        if ($scope.CurrentSource.source == 'BILLNO' || $scope.CurrentSource.source == 'LRENTRYNO') {
                            var excelsheet = { 'rows': [] };
                            $.each(data.value.stockitems, function (k, v) {
                                if (v.stockqty > 0) {
                                    excelsheet.rows.push({ 'values': [v.barcode, v.stockqty] });
                                }
                            });
                            $scope.fileuploadedcomplete(excelsheet);
                        }
                    }
                }
            });
        }


        $scope.ShowSizeGroupMode = function () {
            var dialog = ngDialog.open({
                template: 'SizeGroupMode',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService',
                    function ($scope, $timeout, $http, hotkeys, $rootScope, restService) {
                        $scope.ParentScope = $scope.ngDialogData[0];
                        $scope.SizeGroupList = $scope.ParentScope.SizeGroupList;
                        $scope.Selected = function () {
                            var sizegroup = $scope.SizeGroupList.find(function (item) {
                                return item.id == $scope.sizegroupid;
                            });
                            if (sizegroup && sizegroup != null)
                                $scope.closeThisDialog(sizegroup);
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        $scope.system_possizelist = data.value.possizelist;
                        $scope.TemplateCurrentBill.billattributes.sizegroupid = data.value.id;
                        $scope.CurrentBill.billattributes.sizegroupid = data.value.id;
                        $scope.GridConfig();
                    }
                }
            });
        }

        var PriceListManager = function (categorypricelist) {
            var PriceListCollection = [];
            var PriceList = function (customerid, customercategoryid, result) {
                var List = [];
                this.CustomerID = customerid;
                this.CategoryID = customercategoryid;
                this.GetPrice = function (design) {
                    var price = List.find(function (item) {
                        return item.id == design;
                    });
                    if (price && price != null)
                        return price.price;
                    return 0;
                }
                this.AddPrice = function (priceitem) {
                    List.push(priceitem);
                }
                $.each(result, function (k, v) {
                    List.push({ 'id': v.design, 'price': v.price });
                });
            };
            this.LoadPrice = function (customerid) {
                var pricelist = PriceListCollection.find(function (item) {
                    return item.CustomerID == customerid;
                })
                if (typeof (pricelist) == 'undefined' || pricelist == null) {
                    restService.Get('CustomerpriceService/SearchAll/' + customerid, true).then(function (result) {
                        var pricelist = new PriceList(customerid, 0, result);
                        PriceListCollection.push(pricelist);
                    });
                }
            }
            this.GetPrice = function (customerid, categoryid, design, callback) {
                var pricelist = PriceListCollection.find(function (item) {
                    return item.CustomerID == customerid;
                })
                if (pricelist && pricelist != null) {
                    var price = pricelist.GetPrice(design);
                    callback(price);
                }
                else {
                    pricelist = PriceListCollection.find(function (item) {
                        return item.CategoryID == categoryid;
                    })
                    if (pricelist && pricelist != null) {
                        var price = pricelist.GetPrice(design);
                        callback(price);
                    }
                    else {
                        callback(0);
                    }
                }
            }
            $.each(categorypricelist, function (k, v) {
                var pricelist = PriceListCollection.find(function (item) {
                    return item.CategoryID == v.categoryid;
                });
                if (typeof (pricelist) == 'undefined' || pricelist == null) {
                    pricelist = new PriceList(0, v.categoryid, []);
                    PriceListCollection.push(pricelist);
                }
                pricelist.AddPrice({ 'id': v.design, 'price': v.price });
            });
        }

        $scope.ShowSOAllocation = function (billitem, callback) {
            var dialog = ngDialog.open({
                template: 'SaleOrderAllocation',
                data: [$scope],
                width: '750px',
                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService',
                    function ($scope, $timeout, $http, hotkeys, $rootScope, restService) {
                        $scope.ParentScope = $scope.ngDialogData[0];
                        $scope.Items = [];
                        $scope.TotalQty = billitem.qty;
                        $scope.AllocatedQty = 0;
                        if (typeof (billitem.billattributes) == 'undefined') billitem.billattributes = {};
                        if (typeof (billitem.billattributes.so) == 'undefined') billitem.billattributes.so = [];
                        $.each($scope.ParentScope.SOManager.SaleOrderItems, function (k, v) {
                            var soitem = (JSON.parse(JSON.stringify(v)));
                            soitem.newallocation = 0;
                            $.each(billitem.billattributes.so, function (kk, vv) {
                                if (vv.id == v.soitemid) {
                                    soitem.newallocation += vv.qty;
                                    soitem.allocated -= vv.qty;
                                }
                            });
                            soitem.available = soitem.pendingqty - soitem.allocated + soitem.newallocation;
                            $scope.Items.push(soitem);
                            $scope.AllocatedQty += soitem.newallocation;
                        })
                        $scope.AllocationChanged = function () {
                            $scope.AllocatedQty = 0;
                            $.each($scope.Items, function (k, v) {
                                var allocation = parseFloat(v.newallocation);
                                if (isNaN(allocation)) allocation = 0;
                                $scope.AllocatedQty += allocation;
                            })
                        }
                        $scope.Selected = function () {
                            $scope.closeThisDialog($scope.Items);
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        var newso = [];
                        var allocatedqty = 0;
                        $.each($scope.SOManager.SaleOrderItems, function (k, v) {
                            $.each(billitem.billattributes.so, function (kk, vv) {
                                if (vv.id == v.soitemid) {
                                    v.allocated -= vv.qty;
                                }
                            });
                            var modifiedsoitem = data.value.find(function (item) {
                                return item.soitemid == v.soitemid;
                            });
                            var allocation = parseFloat(modifiedsoitem.newallocation);
                            if (!isNaN(allocation) && allocation > 0) {
                                newso.push({ 'id': v.soitemid, 'qty': allocation });
                                v.allocated += allocation;
                                allocatedqty += allocation;
                            }
                        })
                        billitem.billattributes.so = newso;
                        billitem.billattributes.soqty = allocatedqty;
                        callback(allocatedqty);
                    }
                }
            });
        }

        function SaleOrderManager() {
            this.SaleOrderItems = null;
            this.DesignwiseQty = [];
            this.DesignwiseBarcodeQty = [];
            this.Enabled = false;
            this.Config = null;
            this.Parent = null;
            this.CustomerID = null;
            this.Initialize = function (parantScope) {
                this.SaleOrderItems = null;
                this.ParentScope = parantScope;
                if (typeof ($scope.system_saleorder) == 'undefined' || $scope.system_saleorder == null) {
                    this.Enabled = false;
                }
                else if ($scope.system_saleorder.design && $scope.system_saleorder.design != null) {
                    this.Enabled = true;
                    this.Config = $scope.system_saleorder;
                }
            }
            this.ValidateSO = function (customerid) {
                return !this.Enabled || customerid == this.CustomerID;
            }

            this.SearchSaleOrder = function (order, billitems, callback) {
                if (order == undefined || order == null || order == '') {
                    if (billitems && billitems != null && billitems.length > 0 &&
                        this.ParentScope &&
                        this.ParentScope.IsSaleReturn) {
                        $.each(billitems, function (k, v) {
                            if (v.attributes && v.attributes != null) {
                                v.attributes.so = null;
                            }
                        })
                    }
                }
                this.CustomerID = 0;
                this.SaleOrderItems = null;
                if (typeof (order) != 'undefined' && order != null && order.length > 0) {
                    var service = this;
                    this.DesignwiseQty = [];
                    this.DesignwiseBarcodeQty = [];
                    var DesignwiseQty = this.DesignwiseQty;
                    var DesignwiseBarcodeQty = this.DesignwiseBarcodeQty;
                    var currentbillitems = null;
                    if (typeof (callback) != 'undefined') {
                        currentbillitems = billitems;
                    }
                    restService.Get('SalesorderService/GetSaleOrder/' + $rootScope.LocationInfo.location.id + '/' + $scope.qEncodeURIComponent(order) + '/' + ($scope.IsSaleReturn ? 2 : 0), true).then(function (results) {
                        if (results.length == 0) {
                            $rootScope.ShowAlert('Sales Order', "Can't find the given sales order - " + order);
                        }
                        else if ($scope.IsSaleReturn) {
                            service.SaleOrderItems = results;
                            service.CustomerID = results[0].customerid;
                            if ((currentbillitems == null || currentbillitems.length == 0) &&
                                results[0].customerid != service.ParentScope.CurrentBill.customerid) {
                                service.ParentScope.CurrentBill.customerid = results[0].customerid;
                                if (typeof ($scope.CurrentBill.billattributes) == 'undefined') $scope.CurrentBill.billattributes = {};
                                if (results[0].shipto > 0) {
                                    $scope.CurrentBill.billattributes.hasshiptto = true;
                                    $scope.CurrentBill.billattributes.shiptoid = results[0].shipto;
                                }
                                service.ParentScope.CustomerChanged(function () {
                                    service.ParentScope.LoadReturnSO(results);
                                });
                            }
                            else {
                                service.ParentScope.LoadReturnSO(results);
                            }
                        }
                        else {
                            //$.each(results, function (k, v) {
                            //    v.pendingqty -= v.billedqty;
                            //});
                            if ($scope.CurrentBill && $scope.CurrentBill.id &&
                                $scope.CurrentBill.id > 0 && $scope.CurrentBill.Billitems &&
                                $scope.CurrentBill.Billitems.length > 0) {

                                $.each($scope.CurrentBill.Billitems, function (k, v) {
                                    if (v.id && v.id > 0 &&
                                        v.attributes &&
                                        v.attributes != null &&
                                        v.attributes.so &&
                                        v.attributes.so != null &&
                                        v.attributes.so.length > 0) {

                                        $.each(v.attributes.so, function (kk, vv) {
                                            var billedsoitem = results.find(function (item) {
                                                return item.soitemid == vv.id;
                                            });
                                            if (billedsoitem && billedsoitem != null) {
                                                billedsoitem.pendingqty += vv.qty;
                                            }
                                        });

                                    }
                                })
                            }
                            service.SaleOrderItems = results;
                            service.CustomerID = results[0].customerid;
                            if (currentbillitems == null &&
                                results[0].customerid != service.ParentScope.CurrentBill.customerid) {
                                service.ParentScope.CurrentBill.customerid = results[0].customerid;
                                if (typeof ($scope.CurrentBill.billattributes) == 'undefined') $scope.CurrentBill.billattributes = {};
                                if (results[0].shipto > 0) {
                                    $scope.CurrentBill.billattributes.hasshiptto = true;
                                    $scope.CurrentBill.billattributes.shiptoid = results[0].shipto;
                                }
                                service.ParentScope.CustomerChanged();
                            }

                            if (service.ParentScope.CurrentBill.id && service.ParentScope.CurrentBill.id > 0) {
                                $scope.CurrentBill.billattributes.hasshiptto = false;
                                //currentbillitems = $scope.CurrentBill.Billitems;
                                if ($scope.CurrentBill.billattributes.shiptoid &&
                                    $scope.CurrentBill.billattributes.shiptoid > 0) {
                                    $scope.CurrentBill.billattributes.hasshiptto = true;
                                }
                            }

                            if (currentbillitems && currentbillitems != null) {
                                $.each(currentbillitems, function (bk, bv) {
                                    if (bv.attributes.so && (typeof (bv.attributes.soqty) == 'undefined' || bv.attributes.soqty == 0)) {
                                        bv.attributes.soqty = 0;
                                        $.each(bv.attributes.so, function (sk, sv) {
                                            bv.attributes.soqty += sv.qty;
                                        })
                                    }
                                });
                            }

                            $.each(results, function (k, v) {
                                v.so = [];
                                v.allocated = 0;
                                //if (currentbillitems != null) {
                                //    $.each(currentbillitems, function (bk, bv) {
                                //        if (bv.attributes.so) {
                                //            $.each(bv.attributes.so, function (sk, sv) {
                                //                if (sv.id == v.soitemid) {
                                //                    if (v.designitemcode == 'GPR-1449/2') {
                                //                        var a = 0;
                                //                    }
                                //                    v.pendingqty += sv.qty;
                                //                }
                                //            })
                                //        }
                                //    });
                                //}
                                //if (v.designitemcode == 'GPR-1449/2') {
                                //    var a = 0;
                                //}
                                var dqty = DesignwiseQty.find(function (item) {
                                    return v.designitemcode == item.code;
                                });
                                if (typeof (dqty) == 'undefined' || dqty == null) {
                                    DesignwiseQty.push({ 'qty': v.pendingqty, 'code': v.designitemcode });
                                }
                                else {
                                    dqty.qty += v.pendingqty;
                                }

                                var dqty = DesignwiseBarcodeQty.find(function (item) {
                                    return v.uid == item.uid;
                                });
                                if (typeof (dqty) == 'undefined' || dqty == null) {
                                    DesignwiseBarcodeQty.push({ 'qty': v.pendingqty, 'uid': v.uid });
                                }
                                else {
                                    dqty.qty += v.pendingqty;
                                }
                                
                            });
                            if (typeof (callback) != 'undefined')
                                callback();
                        }
                    });
                }
            }
            this.Clear = function () {
                if (this.SaleOrderItems != null) {
                    $.each(this.SaleOrderItems, function (k, v) {
                        v.so = [];
                        v.allocated = 0;
                    });
                }
                $.each(this.CurrentBill.billitems, function (k, v) {
                    if (typeof (v.attributes) == 'undefined') v.attributes = {};
                    v.attributes.so = [];
                })
            }

            this.RemoveItem = function (billitem) {
                if (!this.Enabled || this.SaleOrderItems == null) return;
                var salesorderitems = this.SaleOrderItems;
                $.each(billitem.attributes.so, function (k, v) {
                    var soitem = salesorderitems.find(function (item) {
                        return item.soitemid == v.id;
                    });
                    if (soitem && soitem != null) {
                        soitem.allocated -= soitem.qty;
                    }
                })
            }

            this.UpdateOrder = function (groupitems) {
                if (this.Enabled && this.SaleOrderItems && this.SaleOrderItems != null) {
                    if ($scope.ApplicationSettings.appsettings.system_posscrtemplate == 4) {
                        var salesorders = this.SaleOrderItems;
                        $.each(groupitems, function (k, v) {
                            var gqty = salesorders.find(function (item) {
                                return item.uid == v.displaycode;
                            })
                            if (gqty && gqty != null)
                                v.soqty = gqty.pendingqty;
                        });
                    }
                    else {
                        var designqty = this.DesignwiseQty;
                        var designbarcodeqty = this.DesignwiseBarcodeQty;
                        $.each(groupitems, function (k, v) {
                            var gqty = designqty.find(function (item) {
                                return item.code == v.designitemcode || item.code == v.printingname;
                            })
                            if (gqty && gqty != null) {
                                v.attributes.somissing = false;
                                v.soqty = gqty.qty;
                                var somissing = v.list.find(function (item) {
                                    return item.attributes.somissing
                                });
                                if (somissing && somissing != null) {
                                    v.attributes.somissing = true;
                                    v.attributes.somismatch = true;
                                }
                                else {
                                    if (v.qty != v.soqty) {
                                        v.attributes.somismatch = true;
                                        v.attributes.somissing = false;
                                    }
                                    if (v.qty > v.soqty) {
                                        v.attributes.somissing = true;
                                        v.attributes.somismatch = false;
                                    }
                                }
                            }
                            else {
                                var vuid = (v.attributes && v.attributes.serialno && v.attributes.serialno != null &&
                                    v.attributes.serialno != '') ? v.attributes.serialno : v.barcode;

                                var gqty = designbarcodeqty.find(function (item) {
                                    return vuid == item.uid;
                                })
                                if (gqty && gqty != null) {
                                    v.attributes.somissing = false;
                                    v.soqty = gqty.qty;
                                }
                                else {
                                    v.attributes.somissing = true;
                                    v.soqty = 0;
                                }
                            }
                        });
                    }
                }
            }


            this.UpdateItem = function (billitem, callback) {
                if (typeof (billitem.attributes) == 'undefined') billitem.attributes = {};
                billitem.attributes.so = [];
                if (this.Enabled) {
                    if (this.SaleOrderItems != null) {
                        var billed = billitem.qty;
                        $.each(billitem.attributes.so, function (k, v) {
                            var soitem = this.SaleOrderItems.find(function (item) {
                                return item.soitemid == v.id;
                            });
                            if (soitem && soitem != null) {
                                if (isNaN(soitem.allocated)) soitem.allocated = 0;
                                soitem.allocated -= soitem.qty;
                            }
                        })
                        billitem.attributes.so = [];
                        var required = 0;
                        var ordered = 0;
                        var allocated = 0;
                        var billedqty = 0;
                        var existsoitems = this.SaleOrderItems.find(function (item) {
                            return item.uid == billitem.attributes.serialno || item.uid == billitem.barcode;
                        });

                        for (var i = 0; i < this.SaleOrderItems.length; i++) {
                            var soitem = this.SaleOrderItems[i];
                            //soitem.productid == billitem.productid &&
                            if (soitem.uid == billitem.attributes.serialno || soitem.uid == billitem.barcode) {
                                if (isNaN(soitem.allocated)) soitem.allocated = 0;
                                ordered += soitem.pendingqty;
                                allocated += soitem.allocated;
                                billedqty += soitem.billedqty;
                            }
                        }
                        if (ordered <= 0) {
                            billitem.attributes.somissing = true;
                            if (billedqty > 0) {
                                if (billitem.attributes.serialno && billitem.attributes.serialno != null && billitem.attributes.serialno!='')
                                    this.ParentScope.Toaster.Info('This SO/UAN already billed ' + billitem.attributes.serialno);
                                else
                                    this.ParentScope.Toaster.Info('This SO/UAN already billed ' + billitem.barcode);
                            }
                            else {
                                if (billitem.attributes.serialno && billitem.attributes.serialno != null && billitem.attributes.serialno != '')
                                    this.ParentScope.Toaster.Info('SO Not available for this UAN - ' + billitem.attributes.serialno);
                                else
                                    this.ParentScope.Toaster.Info('SO Not available for this UAN ' + billitem.barcode);
                            }
                        }
                        else {
                            for (var i = 0; i < this.SaleOrderItems.length && Math.abs(billed) > 0; i++) {
                                var soitem = this.SaleOrderItems[i];
                                //soitem.productid == billitem.productid &&
                                if (soitem.uid == billitem.attributes.serialno || soitem.uid == billitem.barcode) {
                                    if (isNaN(soitem.allocated)) soitem.allocated = 0;
                                    available = soitem.pendingqty - soitem.allocated;
                                    if (available >= Math.abs(billed)) {
                                        billitem.attributes.so.push({
                                            'id': soitem.soitemid,
                                            'qty': Math.abs(billed)
                                        });
                                        allocated += Math.abs(billed);
                                        billed = 0
                                    }
                                    else {
                                        billitem.attributes.so.push({ 'id': soitem.soitemid, 'qty': Math.abs(billed) });
                                        billed -= available;
                                        allocated += available;
                                    }
                                }
                            }
                            billitem.attributes.soqty = Math.abs(billitem.qty) - Math.abs(billed);
                            if (allocated != ordered || allocated != Math.abs(billitem.qty)) {
                                billitem.attributes.somissing = true;
                                if (billitem.attributes.serialno && billitem.attributes.serialno != null && billitem.attributes.serialno!='')
                                    this.ParentScope.Toaster.Info(billitem.attributes.serialno + ' Pending - ' + (ordered - allocated) + '(' + ordered + '), Billed -' + Math.abs(billitem.qty));
                                else
                                    this.ParentScope.Toaster.Info(billitem.barcode + ' Pending - ' + (ordered - allocated) + '(' + ordered + '), Billed -' + Math.abs(billitem.qty));
                            }
                            else {
                                billitem.attributes.somissing = false;
                            }
                        }
                    }
                    else {
                        billitem.attributes.somissing = true;
                    }
                }
                if (typeof (callback) != 'undefined') {
                    callback();
                }
            }
        }

        $scope.SOManager = new SaleOrderManager();
        $scope.priceList = new PriceListManager([]);
        $scope.CreateNew();
    }]);