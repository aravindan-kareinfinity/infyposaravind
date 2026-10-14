var app = angular.module('InfinityLab');

app.controller('SalesOfferController', ['UserDetails', '$scope', '$http', '$location', '$rootScope',
    '$window', 'restService', '$filter', '$timeout', 'localStorageService', '$interval', 'hotkeys', '$stateParams', 'ngDialog', '$sce', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, $timeout, localStorageService,
        $interval, hotkeys, $stateParams, ngDialog, $sce, cntxService) {
        $scope.ShowSearch = false;
        $scope.ShowImage = false;
        $scope.currentimageid = '';
        $scope.SearchResult = [];
        $scope.lastBills = [];
        $scope.IsDiscount = false;
        $scope.IsDaylight = false;
        $scope.DisablePOS = false;
        $scope.dateStyle = '';
        $scope.isoffline = false;
        $scope.AllowSMChange = false;
        $scope.BeginGridTransaction = false;
        $scope.igrid = null;
        $scope.CuttingBarcodeParser = null;
        $scope.WeighingBarcodeParser = null;
        $scope.system_multiplesalemen = false;
        $scope.system_saleunicodepdf = false;
        $scope.system_supportmanualbill = false;
        $scope.system_systemrole = false;
        $scope.system_posmultiprice = 0;
        $scope.settlement_reward_value = 1;
        $scope.settlement_reward_slabed = false;
        $scope.system_expiredpromoexchange = false;
        $scope.system_poshold = false;
        $scope.system_posvoid = false;
        $scope.system_ishypermarketapplication = false;
        $scope.sale_negativestock = true;
        $scope.gft_wocusredemption = false;
        $scope.system_disableposcredit = false;
        $scope.system_disableuniquebarcode = false;
        $scope.system_restrictuniquebarcode = false;
        $scope.system_maxbarcodelength = 0;
        $scope.system_posdisablerefund = false;
        $scope.system_supportcustcreditnote = false;
        $scope.system_salesroundingdigit = 0;
        $scope.system_salesrounding = 0;
        $scope.system_saledirectpdf = false;
        $scope.system_cashierobcb = false;
        $scope.system_salesroundingOnBarcode = false;
        $scope.sale_requestpasscodetosms = false;
        $scope.system_taxrangeautodiscount = false;
        $scope.system_salepricemrpdifference = false;
        $scope.system_salecustomermandatory = false;
        $scope.system_saleautomobileno = false;
        $scope.system_salesmanmandatory = false;
        $scope.system_crsettlementonbill = false;
        $scope.system_multiplesaleqtypopup = false;
        $scope.system_adonfinalprice = false;
        $scope.system_adonschemediscount = false;
        $scope.system_excluderewards = false;
        $scope.sale_canclaimsamebillrewards = false;
        $scope.system_duplicatecoupon = false;
        $scope.system_silentsettlement = false;
        $scope.system_salecollectrefemp = false;
        $scope.system_posexclusivetax = false;
        $scope.system_multipleinvoiceqty = false;
        $scope.system_supportmanualprice = false;
        $scope.hassale_customercardprefix = false;
        $scope.hassale_deliverycardprefix = false;
        $scope.system_supportestimate = false;
        $scope.system_creditbillotp = false;
        $scope.system_disableaddlndisonschemeitems = false;
        $scope.system_disableaddlndisondiscounteditems = false;
        $scope.system_disablecoupondisonaddlndisitems = false;
        $scope.sale_allowchangeofcustomer = false;
        $scope.system_supportdeliveryrack = false;
        $scope.system_disableloadsalebillacross = false;
        $scope.sale_customercardprefix = "";
        $scope.sale_deliverycardprefix = "";
        $scope.DeliveryRackInformation = '';
        $scope.sale_exchangerefund = 0;

        $scope.ConfirmWidow = {
            'enabled': false,
            'Message': function (e) {
                e.preventDefault();
                e.returnValue = '';
            },
            'SetEnable': function (value) {
                if (this.enabled == value) return;
                if (this.enabled = true && value == false)
                    window.removeEventListener('beforeunload', this.Message);
                this.enabled = value;
                if (this.enabled)
                    window.addEventListener('beforeunload', this.Message);
            }
        };
        $scope.LocalContext = cntxService.CreatePageContext('salesoffer');
        $scope.HoldList = {
            'enabled': false,
            'list': [],
            'grid': null,
            'initialize': function () {
                var value = $scope.LocalContext.Get('HoldBills', false);
                if (value && value != null && value.length > 0) {
                    this.list = value;
                }
                var igrid = new InfyGrid('lastBillTable', '450px', '100%', true, false, true,
                    $scope.system_salecollectrefemp ? 420 : 390);
                igrid.options.rowHeight = 30;
                igrid.AddSimpleColumn('k', 'No', 50);
                igrid.AddFormatedColumn('amount', 'Amount', 90, 'N2', 2);
                igrid.AddSimpleColumn('info', 'Detail', 100);
                igrid.AddButtonColumn('load', '', 40, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' class='btn btn-primary btn-xs'><i class='fa fa-download' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                igrid.Initialize();
                this.grid = igrid;
                this.grid.holdmanager = this;
                this.grid.UpdateData(this.list);
                igrid.grid.onClick.subscribe(function (e) {
                    var cell = igrid.grid.getCellFromEvent(e);
                    var item = igrid.grid.getDataItem(cell.row);
                    var command = igrid.grid.getColumns()[cell.cell].id;
                    if (command == 'load') {
                        igrid.holdmanager.list.splice(igrid.holdmanager.list.indexOf(item), 1);
                        $timeout(function () {
                            igrid.holdmanager.grid.UpdateData(igrid.holdmanager.list);
                            $scope.autoCoding.code = '';
                            $scope.CurrentBill = item.value;
                            if ($scope.igrid != null)
                                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                            $scope.ConfirmWidow.SetEnable($scope.CurrentBill.Billitems.length > 0);
                            $scope.$digest();
                        }, 10);
                    }
                });
            },
            'add': function (item) {
                var key = null;
                for (var i = 1; i <= 5; i++) {
                    var exist = this.list.find(function (item) {
                        return item.k == i;
                    })
                    if (exist == undefined || exist == null) {
                        key = i;
                        break;
                    }
                }
                if (key == null) {
                    $rootScope.Toaster.Notice("Maximum 5 limit reached");
                }
                else {
                    var description = [];
                    $.each(item.Billitems, function (k, v) {
                        if (description.indexOf('printingname') < 0)
                            description.push(v.printingname);
                    });
                    this.list.push({
                        'k': key, 'info': description.join(' '), 'value': item,
                        'amount': item.total
                    })
                    $scope.LocalContext.Post('HoldBills', this.list, false);
                    this.grid.UpdateData(this.list);
                }
            },
            'destroy': function () {
                if (this.grid && this.grid != null) {
                    this.grid.destroy();
                    this.enabled = false;
                }
            },
            'get': function (key) {
                var exist = this.list.find(function (item) {
                    return item.k == key;
                })
                if (exist && exist != null) {
                    this.list.splice(this.list.indexOf(exist), 1);
                    $scope.LocalContext.Post('HoldBills', this.list, false);
                    this.grid.UpdateData(this.list);
                    return exist;
                }
                return null;
            }
        };

        $scope.BillNoConfig = {
            'multiple': false, 'prefix': '', 'bulkbillprefix': '', 'bulkbillprefixlist': [], 'prefixlist': [], 'masterprefix': '',
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

                $scope.AD = parseFloat($rootScope.UserInfo.AD);
                if (isNaN($scope.AD)) $scope.AD = 0;
                $scope.system_salecustomermandatory = false || $scope.ApplicationSettings.appsettings.system_salecustomermandatory;
                if ($scope.IsSaleReturn)
                    $scope.system_salecustomermandatory = false || $scope.ApplicationSettings.appsettings.system_srcustomermandatory;
                if ($scope.IsEstimate)
                    $scope.system_salecustomermandatory = false || $scope.ApplicationSettings.appsettings.system_estimatecustmandatory;

                $scope.system_salepricemrpdifference = false || $scope.ApplicationSettings.appsettings.system_salepricemrpdifference;
                $scope.system_taxrangeautodiscount = false || $scope.ApplicationSettings.appsettings.system_taxrangeautodiscount;
                $scope.system_supportmanualbill = false || $scope.ApplicationSettings.appsettings.system_supportmanualbill;
                $scope.system_saleunicodepdf = false || $scope.ApplicationSettings.appsettings.system_saleunicodepdf;
                $scope.system_multiplesalemen = false || $scope.ApplicationSettings.appsettings.system_multiplesalemen;
                $scope.system_systemrole = false || $scope.ApplicationSettings.appsettings.system_systemrole;
                $scope.system_ishypermarketapplication = false || $scope.ApplicationSettings.appsettings.system_ishypermarketapplication;
                $scope.system_posmultiprice = $scope.ApplicationSettings.appsettings.system_posmultiprice;
                $scope.settlement_reward_value = $scope.ApplicationSettings.appsettings.settlement_reward_value;
                $scope.settlement_reward_slabed = false || $scope.ApplicationSettings.appsettings.settlement_reward_slabed;
                $scope.system_expiredpromoexchange = false || $scope.ApplicationSettings.appsettings.system_expiredpromoexchange;
                $scope.system_poshold = false || $scope.ApplicationSettings.appsettings.system_poshold;
                $scope.system_posvoid = false || $scope.ApplicationSettings.appsettings.system_posvoid;
                $scope.system_salesmanmandatory = false || $scope.ApplicationSettings.appsettings.system_salesmanmandatory;
                $scope.system_crsettlementonbill = false || $scope.ApplicationSettings.appsettings.system_crsettlementonbill;
                $scope.system_multiplesaleqtypopup = false || $scope.ApplicationSettings.appsettings.system_multiplesaleqtypopup;
                $scope.system_adonfinalprice = false || $scope.ApplicationSettings.appsettings.system_adonfinalprice;
                $scope.system_adonschemediscount = false || $scope.ApplicationSettings.appsettings.system_adonschemediscount;
                $scope.system_saleautomobileno = false || $scope.ApplicationSettings.appsettings.system_saleautomobileno;
                $scope.system_excluderewards = false || $scope.ApplicationSettings.appsettings.sale_excludeproduct;
                $scope.sale_canclaimsamebillrewards = false || $scope.ApplicationSettings.appsettings.sale_canclaimsamebillrewards;
                $scope.system_duplicatecoupon = false || $scope.ApplicationSettings.appsettings.sale_duplicatecoupon;
                $scope.system_silentsettlement = false || $scope.ApplicationSettings.appsettings.system_silentsettlement;
                $scope.system_disableuniquebarcode = false || $scope.ApplicationSettings.appsettings.system_disableuniquebarcode;
                $scope.system_disableposcredit = false || $scope.ApplicationSettings.appsettings.system_disableposcredit;
                $scope.gft_wocusredemption = false || $scope.ApplicationSettings.appsettings.gft_wocusredemption;
                $scope.system_restrictuniquebarcode = false || $scope.ApplicationSettings.appsettings.system_restrictuniquebarcode;
                $scope.system_salecollectrefemp = false || $scope.ApplicationSettings.appsettings.system_salecollectrefemp;
                $scope.system_posexclusivetax = false || $scope.ApplicationSettings.appsettings.system_posexclusivetax;
                $scope.system_multipleinvoiceqty = false || $scope.ApplicationSettings.appsettings.system_multiplepossaleqty;
                $scope.system_supportmanualprice = false || $scope.ApplicationSettings.appsettings.system_supportmanualprice;
                $scope.sale_negativestock = !$scope.ApplicationSettings.appsettings.sale_negativestock;
                $scope.sale_customercardprefix = $scope.ApplicationSettings.appsettings.sale_customercardprefix;
                $scope.sale_deliverycardprefix = $scope.ApplicationSettings.appsettings.sale_deliverycardprefix;
                $scope.system_supportestimate = false || $scope.ApplicationSettings.appsettings.system_supportestimate;
                $scope.system_creditbillotp = false || $scope.ApplicationSettings.appsettings.system_creditbillotp;
                $scope.system_disableaddlndisonschemeitems = false || $scope.ApplicationSettings.appsettings.system_disableaddlndisonschemeitems;
                $scope.system_disableaddlndisondiscounteditems = false || $scope.ApplicationSettings.appsettings.system_disableaddlndisondiscounteditems;
                if ($scope.system_disableaddlndisondiscounteditems &&
                    $rootScope.UserInfo.p_CANDISCOUNTONDISCOUNT && $rootScope.UserInfo.p_CANDISCOUNTONDISCOUNT.view)
                    $scope.system_disableaddlndisondiscounteditems = false;
                $scope.system_disablecoupondisonaddlndisitems = false || $scope.ApplicationSettings.appsettings.system_disablecoupondisonaddlndisitems;
                $scope.sale_allowchangeofcustomer = false || $scope.ApplicationSettings.appsettings.sale_allowchangeofcustomer;
                $scope.system_deliveryrackcard = false || $scope.ApplicationSettings.appsettings.system_deliveryrackcard;
                $scope.system_supportdeliveryrack = false || $scope.ApplicationSettings.appsettings.system_supportdeliveryrack;
                $scope.system_disableloadsalebillacross = false || $scope.ApplicationSettings.appsettings.system_disableloadsalebillacross;

                if ($scope.sale_customercardprefix != 'undefined' && $scope.sale_customercardprefix != null
                    && $scope.sale_customercardprefix != '' && $scope.sale_customercardprefix != ""
                    && $scope.sale_customercardprefix.length > 0)
                    $scope.hassale_customercardprefix = true;


                if ($scope.system_deliveryrackcard &&
                    $scope.sale_deliverycardprefix != 'undefined' && $scope.sale_deliverycardprefix != null
                    && $scope.sale_deliverycardprefix != '' && $scope.sale_deliverycardprefix != ""
                    && $scope.sale_deliverycardprefix.length > 0)
                    $scope.hassale_deliverycardprefix = true;

                if (!$scope.IsSaleReturn) {
                    $scope.sale_exchangerefund = $scope.ApplicationSettings.appsettings.sale_exchangerefund;
                }

                if ($scope.system_supportmanualbill || $scope.system_ishypermarketapplication) {
                    $scope.LoadProduct2SuggestionList();
                }

                if (typeof ($rootScope.LocationInfo) != 'undefined' && typeof ($rootScope.LocationInfo.billconfig) != 'undefined' &&
                    $rootScope.LocationInfo.billconfig != null) {

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
                        //$timeout(function () {
                        //    $(location).attr('href', 'index.html');
                        //}, 1000);
                        $scope.DisablePOS = true;
                        return;
                    }
                }
                $scope.GridConfig();
                if ($scope.system_poshold) {
                    $scope.HoldList.enabled = true;
                    $scope.HoldList.initialize();
                }

                if ($scope.OnlineCounter || $scope.ShowAdditional) {
                    $scope.AdditionalChargeGridConfig();
                }
                else {
                    $scope.LastBillGridConfig();
                }
                if ($scope.system_posvoid) {
                    var voidbill = $scope.LocalContext.Get('VoidBills', false);
                    if (voidbill && voidbill != null) {
                        $scope.CreateNew(true);
                        $scope.CurrentBill = voidbill;
                        $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                        $scope.LocalContext.Remove('VoidBills');
                    }
                }
                $scope.setFocusautocoding();
            }
        };
        $scope.PromotionEnabled = false;
        $scope.DisablePromotionRerun = false;
        $scope.AdditionalDiscountEnabled = false;
        $scope.EMIEnabled = false;
        $scope.ShowLastBitQty = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.DiscountQueue = false;
        $scope.IsSaleReturn = $stateParams.type == 'SALERETURN';
        $scope.IsEstimate = $stateParams.type == 'ESTIMATESALE';
        $scope.IsBulkSale = $stateParams.type == 'BULKSALE' || $stateParams.type == 'ESTIMATESALE';

        if ($scope.IsEstimate) {
            $scope.Title = 'Estimate Bill';
            $scope.leftPanStyle = '#d4e6f1';
        }
        else {
            $scope.Title = $scope.IsSaleReturn ? 'Sales Return' : ($scope.IsBulkSale ? ($rootScope.ShowDeliveryChallan ? 'Delivery Challan' : 'Sales On Approval') : 'Sales');
            $scope.leftPanStyle = $scope.IsSaleReturn ? '#f7dcdc' : ($scope.IsBulkSale ? '#cff7db' : 'white');
        }

        $scope.IsWarehouse = false;
        $scope.CANAPPROVEDISCOUNT = false;
        $scope.BillKeys = [];
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

                if (!$scope.IsSaleReturn &&
                    !$scope.IsBulkSale &&
                    $rootScope.LocationInfo != null &&
                    $rootScope.LocationInfo.location != null &&
                    $rootScope.LocationInfo.location.additionalinfo != null &&
                    $rootScope.LocationInfo.location.additionalinfo.autosettlement) {
                    $scope.AutoSettlement = true;
                };
            }
        });

        restService.Get('ReferencelistService/BILLKEY', true).then(function (results) {
            $.each(results, function (k, v) {
                v.name = v.name.toUpperCase();
            });
            $.each(results, function (k, v) {
                $scope.BillKeys.push(v);
            });
        });

        $scope.Promotions = restService.Get('ReferencelistService/PROMOTIONS', true).then(function (results) {
            $scope.Promotions = results;
        });

        $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
            $scope.CustomerCategory = results;
        });
        $scope.StateList = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.StateList = results;
        });
        $scope.EmployeeList = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            if (results.length == 0) {
                results.push({ 'id': -1, 'name': '-', 'code': '-' });
            }
            $scope.EmployeeList = results;
        });

        $scope.ChangeTitle = function () {
            if ($scope.IsEstimate)
                $scope.Title = 'Estimate Bill';
            else
                $scope.Title = $scope.IsSaleReturn ? 'Sales Return' : ($scope.IsBulkSale ? ($rootScope.ShowDeliveryChallan ? 'Delivery Challan' : 'Sales On Approval') : 'Sales');

            if ($scope.IsDiscount && !$scope.IsBulkSale)
                $scope.Title += ' (Discount)';
        }
        $scope.ChangeTitle();

        $scope.ValidateCounter = function () {

            if ($scope.ApplicationSettings &&
                $scope.ApplicationSettings != null &&
                $scope.ApplicationSettings.appsettings.system_cashierobcb) {
                $scope.system_cashierobcb = true;
                restService.Get('RetailReportService/Cashierstatus/' +
                    $rootScope.LocationInfo.location.id + '/' + $rootScope.LocationInfo.counter.id
                    , true).then(function (result) {

                        if (result.valid) {
                            $scope.system_cashierobcb = false;
                        }
                        else {
                            $rootScope.ShowAlert('Counter Violation', result.message);
                            $scope.DisablePOS = true;
                        }
                    });
            }
        }

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
                    if ($scope.CurrentBill && $scope.CurrentBill != null) {
                        if ($scope.CurrentBill.attributes == undefined || $scope.CurrentBill.attributes == null)
                            $scope.CurrentBill.attributes = {};
                        $scope.CurrentBill.billattributes.online = $scope.OnlineCounter;
                    }
                    $rootScope.Toaster.Notice("Online Counter..");
                }
            }
        });

        $scope.NonUANItems = null;
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

        $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
            $scope.AdditionalChargeList = results;
        });

        $scope.AdditionalChargeGridConfig = function () {

            if ($scope.HoldList && $scope.HoldList != null && $scope.HoldList.enabled)
                $scope.HoldList.destroy();

            if ($scope.agrid && $scope.agrid != null)
                $scope.agrid.destroy();

            $scope.HoldList = null;

            var igrid = new InfyGrid('additionalchargelist', '450px', '100%', true, false, true, 430);
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

        $scope.ShowAdditional = false;
        $scope.ShowAdditionalCharges = function () {
            if ($scope.ShowAdditional) {
                $scope.AdditionalChargeGridConfig();
            }
            else {
                $scope.LastBillGridConfig();
            }
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

            if ($scope.CurrentBill.additionalchargeslist == undefined ||
                $scope.CurrentBill.additionalchargeslist == null)
                $scope.CurrentBill.additionalchargeslist = [];

            $scope.CurrentBill.additionalchargeslist.push(JSON.parse(JSON.stringify($scope.AdditionalChargeItem)));
            $scope.ComputeAddtionalChargeValues();
            $scope.AdditionalChargeItem.Charge = "";
            $scope.AdditionalChargeItem.Tax = "";
            $scope.AdditionalChargeItem.Amount = "";
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

                if (fromtype && item.additionalinfo.billdiscount != 0) {

                    if (item.additionalinfo.paytype == 'PACKING') {
                        var perpiece = item.additionalinfo.billdiscount;
                        $scope.AdditionalChargeItem.Charge = $scope.CurrentBill.totalqty * perpiece;
                        sourcevalue = $scope.GetNumber($scope.AdditionalChargeItem.Charge);
                    }
                    else if (item.additionalinfo.paytype == 'SERVICE') {
                        var perpiece = item.additionalinfo.billdiscount;
                        $scope.AdditionalChargeItem.Charge = perpiece;
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
                    $scope.AdditionalChargeItem.taxonly = (item.additionalinfo.paytype != 'PACKING' && item.additionalinfo.paytype != 'FRIGHT' && item.additionalinfo.paytype != 'SERVICE');
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
                        $scope.AdditionalChargeItem.Tax = ($scope.AdditionalChargeItem.TaxPercentage * sourcevalue / 100).round(4);
                    }
                }
                else {
                    $scope.AdditionalChargeItem.taxonly = false;
                    $scope.AdditionalChargeItem.TaxPercentage = 0;
                    $scope.AdditionalChargeItem.Tax = 0;
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
                    if (v.chargetype == 'PACKING')
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


        $scope.SalesTax = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.SalesTax = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.Products = null;
        restService.Get('ReferencelistService/ALLPRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.LoadProduct2SuggestionList = function () {
            if ($scope.Products == null || typeof ($scope.Products.length) == 'undefined') {
                $timeout(function () {
                    $scope.LoadProduct2SuggestionList();
                }, 50);
            }
            else {
                $.each($scope.Products, function (k, v) {
                    $scope.BillKeys.push({ 'id': v.id, 'name': v.name, 'code': v.code, 'type': 'PRODUCT' });
                });
            }
        }

        $scope.ReturnReason = restService.Get('ReferencelistService/RETURNREASON', true).then(function (results) {
            $scope.ReturnReason = results;
        });
        $scope.BulkSaleReason = restService.Get('ReferencelistService/BULKSALEREASON', true).then(function (results) {
            $scope.BulkSaleReason = results;
        });
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
            billTax: [],
            billdate: "",
            billtime: "",
            billattributes: {},
            tax: 0.0,
            id: 0,
            isactive: true,
            iscreditbill: false,
            isbulkreturn: false,
            isbulkreturngoods: false,
            discount: 0.0,
            receivable: 0.0,
            additionaldiscount: 0.0,
            customerid: 0,
            phoneno: '',
            approvedby: 'Approved By -',
            customername: '',
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


        $scope.ApplicationSettings = null;
        $scope.LoadApplicationSettings = function () {

            if (!$scope.HasLocationSelected) {
                $rootScope.Triggers.SelectLocation('', function () {
                    $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' &&
                        typeof ($rootScope.LocationInfo.location) != 'undefined')
                    $scope.ShowLocationInfo();
                    $scope.LoadApplicationSettings();
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

                    if ($scope.IsBulkSale) {
                        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
                            $scope.Employees = results;
                        });
                    }

                    $scope.HSNLimit.init($scope.ApplicationSettings.appsettings.sale_hsnlimit);
                    $scope.system_saledirectpdf = $scope.ApplicationSettings.appsettings.system_saledirectpdf;
                    $scope.system_posdisablerefund = $scope.ApplicationSettings.appsettings.system_posdisablerefund;
                    if ($scope.system_posdisablerefund &&
                        $rootScope.UserInfo &&
                        $rootScope.UserInfo.p_SUPPORTPOSREFUND && $rootScope.UserInfo.p_SUPPORTPOSREFUND.view) {
                        $scope.system_posdisablerefund = false;
                    }

                    $scope.system_supportcustcreditnote = false || $scope.ApplicationSettings.appsettings.system_supportcustcreditnote;
                    if ($scope.system_supportcustcreditnote &&
                        $rootScope.UserInfo &&
                        $rootScope.UserInfo.p_SUPPORTCREDITNOTESETTLEMENT && !$rootScope.UserInfo.p_SUPPORTCREDITNOTESETTLEMENT.view) {
                        $scope.system_supportcustcreditnote = false;
                    }

                    $scope.system_maxbarcodelength = $scope.ApplicationSettings.appsettings.system_maxbarcodelength;

                    if ($scope.AutoSettlement) {
                        $scope.ValidateCounter();
                    }



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
                                                    'valid': true,
                                                    'barcode': barcode.substring(0, this.config.barcodelength),
                                                    'qty': parseFloat(intpart + '.' + decpart)
                                                };
                                            }
                                            if (this.config.prefix && this.config.prefix != null &&
                                                this.config.prefix.length > 0 && barcode.startsWith(this.config.prefix)) {
                                                var parts = barcode.substring(this.config.prefix.length).split(this.config.separator);
                                                return { 'valid': parts.length == 2, 'barcode': parts[0], 'qty': parseFloat(parts[1]) };
                                            }
                                            return { 'valid': true, 'barcode': barcode, 'qty': 1 };
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
                                        var wbarcode = barcode.substring(this.config.barcodestart, this.config.barcodestart + this.config.barcodelength);
                                        var wqty = 0;
                                        var kgpart = parseInt(barcode.substring(this.config.kgfrom, this.config.kgfrom + this.config.kglength));
                                        var gmpart = parseFloat("0." + barcode.substring(this.config.grmfrom, this.config.grmfrom + this.config.grmlength));
                                        if (isNaN(kgpart)) kgpart = 0;
                                        if (isNaN(gmpart)) gmpart = 0;
                                        if (gmpart != 0) {
                                            wqty = parseFloat((kgpart + gmpart).toFixed(3));
                                        }
                                        else {
                                            wqty = kgpart;
                                        }
                                        return {
                                            'valid': wbarcode && wbarcode != null && wbarcode != '' && wqty > 0,
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

                    $scope.system_salesroundingdigit = $scope.system_salesrounding == 99 ? 2 : 0;

                    if ($scope.ApplicationSettings.appsettings.system_datechange &&
                        $scope.ApplicationSettings.nextdaycount > 0 &&
                        $filter('date')($scope.ApplicationSettings.currentday, 'dd-MM-yyyy') == $filter('date')(new Date(), 'dd-MM-yyyy')) {
                        $scope.IsDaylight = true;
                        $scope.DaylightCount = $scope.ApplicationSettings.nextdaycount;
                        $scope.dateStyle = '#f7dcdc';
                        $scope.CurrentBill.billdate = $scope.GetBillDate();
                    }

                    if ($scope.ApplicationSettings.appsettings.system_saledatefreeze &&
                        $filter('date')($scope.ApplicationSettings.appsettings.system_saledatefreezeon, 'dd-MM-yyyy') != $filter('date')(new Date(), 'dd-MM-yyyy')) {
                        $scope.dateStyle = '#f7dcdc';
                        $scope.CurrentBill.billdate = $filter('date')($scope.ApplicationSettings.appsettings.system_saledatefreezeon, 'dd-MM-yyyy');
                    }

                    $scope.BillNoConfig.init($scope.ApplicationSettings.appsettings.system_multicompany);
                    $scope.PromotionEnabled = $rootScope.License.promotions && $scope.ApplicationSettings.appsettings.sale_promotion;
                    $scope.DisablePromotionRerun = $rootScope.License.promotions && $scope.ApplicationSettings.appsettings.sale_promotiononlyonestimate;
                    $scope.AdditionalDiscountEnabled = $scope.ApplicationSettings.appsettings.sale_additionaldiscount;
                    $scope.EMIEnabled = $scope.ApplicationSettings.appsettings.sale_emi;
                    $scope.ShowLastBitQty = $scope.ApplicationSettings.appsettings.sale_showlastbitqty;
                    $scope.ExchangeEnabled = $scope.ApplicationSettings.appsettings.sale_exchange;
                    $scope.DiscountQueue = $scope.ApplicationSettings.appsettings.intray_discount;
                    $scope.IsDiscount = $scope.PromotionEnabled && (!$scope.DisablePromotionRerun || $scope.IsEstimate);
                    $scope.ChangeTitle();
                    $scope.ManualRewards = $scope.ApplicationSettings.appsettings.sale_rewardmode == 1;
                    $scope.TemplateCurrentBill.areaid = $scope.ApplicationSettings.appsettings.sale_area;
                    $scope.TemplateCurrentBill.salereasonid = $scope.ApplicationSettings.appsettings.sale_salereason;
                    $scope.TemplateCurrentBill.seasonid = $scope.ApplicationSettings.appsettings.sale_season;




                    if ($scope.ApplicationSettings.appsettings.system_alwayscaps)
                        $scope.OnKeyPress = function (value) { return value.toUpperCase(); };
                }
                else {
                    $rootScope.Toaster.Error("Please save the application setting for the current location");
                }
            }).error(function (result) {
                $rootScope.Toaster.Error("Please save the application setting for the current location");
            });
        };
        $scope.LoadApplicationSettings();

        $scope.GetBillDate = function () {
            if ($scope.IsDaylight == true) {
                var billdate = new Date();
                return billdate.setDate(billdate.getDate() + $scope.DaylightCount);
            }
            return new Date();
        }

        $scope.GetRoundingValue = function () {
            var receivable = $scope.CurrentBill.receivable;
            if ($scope.CurrentBill.additionalcharges &&
                $scope.CurrentBill.additionalcharges != null &&
                $scope.CurrentBill.additionalcharges > 0) {
                receivable += $scope.CurrentBill.additionalcharges;
            }
            if ($scope.system_salesrounding > 0 && $scope.system_salesroundingOnBarcode) {
                return $rootScope.RoundRupee(receivable.round($scope.system_salesroundingdigit), $scope.system_salesrounding) - receivable.round($scope.system_salesroundingdigit);
            }
            else {
                return (receivable.round($scope.system_salesroundingdigit) - receivable).round(2);
            }
        }

        $scope.OnKeyPress = function (value) { return value; };
        $scope.AutoSettlement = false;
        $scope.OnlineCounter = false;

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

        $scope.CreateNew = function (clear) {
            var voiditems = ($scope.CurrentBill && $scope.CurrentBill != null) ? $scope.CurrentBill.voiditems : [];
            $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
            $scope.CurrentBill = (JSON.parse(JSON.stringify($scope.TemplateCurrentBill)));
            $scope.CurrentBill.billdate = $scope.GetBillDate();
            $scope.CurrentBill.billdatedisplay = $filter('date')($scope.CurrentBill.billdate, 'dd-MM-yyyy');
            $scope.CurrentBill.billtimedisplay = $filter('date')($scope.CurrentBill.billdate, 'hh:mm a');
            $scope.CurrentBill.billattributes.online = $scope.OnlineCounter;
            $scope.additionaldiscountamountrequested = 0;
            $scope.additionaldiscountpercentrequested = 0;
            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.lastbit = "";
            $scope.AllowAdditionalDiscount = false;
            $scope.SearchCondition.isbulkreturn = false;
            $scope.SchemeReturnBillItems = [];
            $scope.ShowImage = false;
            $scope.DeliveryRackInformation = '';
            if ($scope.igrid != null)
                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
            $scope.ConfirmWidow.SetEnable($scope.CurrentBill.Billitems.length > 0);

            if ($scope.OnlineCounter || $scope.ShowAdditional) {
                $scope.CurrentBill.additionalchargeslist = [];
                $scope.AdditionalChargeGridConfig();
            }

            if ($scope.system_posvoid && !clear && voiditems &&
                voiditems != null) {
                $scope.CurrentBill.voiditems = voiditems;
            }
        };

        $scope.ManageBillItem = function (action, item) {

            if (action == "Delete") {

                if (item.schemeid > 0 && $scope.IsSaleReturn && $scope.IsDiscount) {
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
                                var ndiscount = ((v.receivable + v.schemediscount) * percentage / 100).round($scope.system_salesroundingdigit);
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

                if ($scope.system_posvoid) {
                    if ($scope.CurrentBill.voiditems == undefined ||
                        $scope.CurrentBill.voiditems == null)
                        $scope.CurrentBill.voiditems = [];
                    item.version = -1;
                    item.isactive = false;
                    $scope.CurrentBill.voiditems.push(item);
                }
                $scope.CurrentBill.Billitems.splice($scope.CurrentBill.Billitems.indexOf(item), 1);
                $scope.CurrentBill.tax = $scope.CurrentBill.tax - item.tax;
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
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
                if ($scope.CurrentBill.Billitems.length == 0)
                    $scope.CurrentBill.isbulkreturn = false;
                else
                    $scope.UpdateOffer();
                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                $scope.setFocusautocoding();
                $scope.ConfirmWidow.SetEnable($scope.CurrentBill.Billitems.length > 0);
                return true;
            }


            if (action == "SMChange") {
                $rootScope.Prompt('Salesman Change', 'Code', function (code) {
                    code = code.toString();
                    if (typeof (code) != 'undefined' && code.length > 0) {
                        restService.Get('EmployeeService/EmployeeCode/' + $scope.UserInfo.OrganizationID + '/' + $rootScope.LocationInfo.location.id + '/' + code.replaceAll('.', ''), true).then(function (results) {
                            item.salesmancode = results.employeecode;
                            item.salesmanname = results.name;
                            item.salesmannameLast = results.name;
                            item.salesmanid = results.id;
                            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                            $scope.setFocusautocoding();
                        });
                    }
                })
            }

            if (action == "SMMChange") {

                if (item.attributes == undefined || item.attributes == null) item.attributes = {};
                if (item.attributes.smcodes == undefined || item.attributes.smcodes == null)
                    item.attributes.smcodes = [];
                $scope.SelectMultipleSalesmen(item,false);
                return;
            }

            if (action == "Edit") {

                if ($scope.IsSaleReturn) {
                    $rootScope.Prompt('Edit', 'Qty', function (count) {
                        var totalcount = $rootScope.GetNumber(count);
                        if (item.sellingmode != 3 && totalcount != totalcount.round(0)) {
                            $rootScope.Toaster.Error("Please Enter valid Qty");
                            return;
                        }

                        if ($scope.system_posvoid) {
                            if ($scope.CurrentBill.voiditems == undefined ||
                                $scope.CurrentBill.voiditems == null)
                                $scope.CurrentBill.voiditems = [];
                            var vitem = (JSON.parse(JSON.stringify(item)));
                            vitem.version = -3;
                            vitem.qty = Math.abs($rootScope.GetNumber(vitem.qty));
                            if (vitem.qty > totalcount) {
                                vitem.qty = vitem.qty - totalcount;
                                $scope.CurrentBill.voiditems.push(vitem);
                            }
                        }

                        if (item.schemeid > 0 && $scope.IsSaleReturn && $scope.IsDiscount) {
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
                                        var ndiscount = ((v.receivable + v.discount) * percentage / 100).round($scope.system_salesroundingdigit);
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
                                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                                $scope.$digest();
                                $scope.setFocusautocoding();
                            }, 10);

                        }

                    }, item.qty * -1);
                }
                else {

                    $rootScope.Prompt('Edit', 'Qty', function (count) {
                        var totalcount = $rootScope.GetNumber(count);
                        if (item.sellingmode != 3 && totalcount != totalcount.round(0)) {
                            $rootScope.Toaster.Error("Please Enter valid Qty");
                            return;
                        }

                        if ($scope.system_posvoid) {
                            if ($scope.CurrentBill.voiditems == undefined ||
                                $scope.CurrentBill.voiditems == null)
                                $scope.CurrentBill.voiditems = [];
                            var vitem = (JSON.parse(JSON.stringify(item)));
                            vitem.version = -2;
                            vitem.isactive = false;
                            vitem.qty = Math.abs($rootScope.GetNumber(vitem.qty));
                            if (vitem.qty > totalcount) {
                                vitem.qty = vitem.qty - totalcount;
                                $scope.CurrentBill.voiditems.push(vitem);
                            }
                        }

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
                                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                                $scope.$digest();
                                $scope.setFocusautocoding();
                            }, 10);
                        }

                    }, item.qty);

                }

                return true;
            }
        };


        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.CurrentBill', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.CurrentBill = value;
            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
            if ($scope.CurrentBill.additionalchargeslist == null)
                $scope.CurrentBill.additionalchargeslist = [];
            $scope.agrid.UpdateData($scope.CurrentBill.additionalchargeslist);
        }

        $scope.UpdateItemInfo = function (saledetail, recompute) {

            if (typeof (recompute) == 'undefined')
                recompute = false;


            saledetail.qty = $scope.GetNumber(saledetail.qty);
            saledetail.piece = $scope.GetNumber(saledetail.piece);

            if (recompute) {
                $scope.CurrentBill.tax = ($scope.CurrentBill.tax - saledetail.tax).round(5);
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - saledetail.discount).round(2);
                $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount - saledetail.schemediscount).round(2);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - saledetail.receivable).round(2);
                $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount - (saledetail.additionaldiscount * saledetail.qty)).round(2);
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
            saledetail.discount = (saledetail.qty * (saledetail.barcodeDetail.discount)).round(2);

            if (typeof (saledetail.additionaldiscount) == 'undefined' || isNaN(saledetail.additionaldiscount))
                saledetail.additionaldiscount = 0;

            if (typeof (saledetail.schemediscount) == 'undefined' || isNaN(saledetail.schemediscount))
                saledetail.schemediscount = 0;

            saledetail.baseprice = saledetail.price - saledetail.additionaldiscount - (saledetail.schemediscount / saledetail.qty).round(5);

            if (Math.abs(saledetail.baseprice.round(2)) < 0.2) {
                saledetail.baseprice = 0;
            }

            if (!$scope.CurrentBill.isinterstatesale) {
                var basePrice = (saledetail.baseprice / (100 + (saledetail.taxpercentage)) * 100);
                var halfTax = (basePrice * (saledetail.taxpercentage / 2) / 100).round(5);
                saledetail.itemtax = (halfTax * 2).round(5);
            }
            else {
                saledetail.itemtax = (saledetail.baseprice / (100 + saledetail.taxpercentage)) * saledetail.taxpercentage;
            }

            //saledetail.salerate = (saledetail.baseprice).round(2);
            saledetail.tax = (saledetail.itemtax * saledetail.qty).round(5);
            saledetail.taxid = saledetail.barcodeDetail.taxid;
            saledetail.rate = (saledetail.baseprice - saledetail.itemtax).round(2);
            saledetail.receivable = (saledetail.baseprice * saledetail.qty).round(2);
            saledetail.stockid = saledetail.barcodeDetail.stockid;
            saledetail.salablegoodsid = saledetail.barcodeDetail.id;
            saledetail.companyid = saledetail.barcodeDetail.companyid;

            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + saledetail.tax).round(5);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + saledetail.discount).round(2);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount + saledetail.schemediscount).round(2);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + saledetail.receivable).round(2);
            $scope.CurrentBill.rounding = $scope.GetRoundingValue();
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + (saledetail.additionaldiscount * saledetail.qty)).round(2);
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
                (typeof ($scope.CurrentBill.ccmargin) == 'undefined' || !$scope.CurrentBill.ccmarginmode)) {

                var product = $scope.Products.find(function (v) {
                    if (v.discountmode == 3 && v.discountvalue > 0) {
                        var bitem = $scope.CurrentBill.Billitems.find(function (vv) {
                            return vv.productid == v.id;
                        });
                        return typeof (bitem) != 'undefined' && bitem != null;
                    }
                    else {
                        return false;
                    }
                });

                if (typeof (product) == 'undefined' || product == null)
                    return;
            }


            if ($scope.CurrentBill.discountotp && !$scope.CurrentBill.discountotpconfirmed) return;


            var list = [];
            $.each($scope.CurrentBill.Billitems, function (k, v) {
                var product = $scope.Products.find(function (item) {
                    return item.id == v.productid;
                });

                if (typeof (product) != 'undefined' && product != null && product.discountmode == 4) {
                    //no bill level discounts.
                }
                else if (typeof (v.returnbillitemid) == 'undefined' ||
                    v.returnbillitemid == null || v.returnbillitemid == 0) {
                    list.push(v);
                }
            });

            $.each(list, function (k, item) {
                if (typeof (item.additionaldiscount) == 'undefined' || isNaN(item.additionaldiscount))
                    item.additionaldiscount = 0;

                if (typeof (item.schemediscount) == 'undefined' || isNaN(item.schemediscount))
                    item.schemediscount = 0;

                if (item.schemediscount == 0) {
                    $scope.CurrentBill.tax = $scope.CurrentBill.tax - item.tax;
                    $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
                    $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                    $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty);

                    if ($scope.CurrentBill.ccmargin > 0 && $scope.CurrentBill.ccmarginmode) {

                        if (item.barcodeDetail.buyingprice > 0) {
                            var newprice = item.barcodeDetail.buyingprice + (item.barcodeDetail.buyingprice * $scope.CurrentBill.ccmargin / 100).round($scope.system_salesroundingdigit);
                            newprice = newprice + (newprice * item.taxpercentage / 100).round(4);

                            var newdiscount = (item.barcodeDetail.price - item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount) - newprice;
                            item.additionaldiscount += newdiscount;
                            item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round(2);
                            item.attributes.customerdiscount = item.additionaldiscount;
                            item.baseprice -= newdiscount;
                        }
                    } else if (item.barcodeDetail.discount == 0 && $scope.CurrentBill.ccloyelty) {
                        var product = $scope.Products.find(function (v) {
                            return v.id == item.productid;
                        });

                        if (typeof (product) != 'undefined' &&
                            product != null &&
                            product.discountmode == 3 &&
                            product.discountvalue > 0) {

                            if (item.additionaldiscount == 0) {
                                var newdiscount = (item.barcodeDetail.price * product.discountvalue / 100).round($scope.system_salesroundingdigit);
                                item.additionaldiscount += newdiscount;
                                item.additionaldiscountpercentage = product.discountvalue;
                                item.attributes.customerdiscount = item.additionaldiscount;
                                item.baseprice -= newdiscount;
                            }
                        }
                        else {
                            var existingDiscountPercentage = (item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount) / item.barcodeDetail.price * 100;

                            if ($scope.CurrentBill.billdiscountinclude) {
                                if (existingDiscountPercentage < $scope.CurrentBill.customerdiscount) {
                                    var addremaindiscount = $scope.CurrentBill.customerdiscount - existingDiscountPercentage;
                                    item.additionaldiscount = (item.barcodeDetail.price * addremaindiscount / 100).round(2);
                                    item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round(2);
                                    item.attributes.customerdiscount = item.additionaldiscount;
                                }
                            }
                            else {
                                var addremaindiscount = $scope.CurrentBill.customerdiscount;
                                item.additionaldiscount = (item.barcodeDetail.price * addremaindiscount / 100).round(2);
                                item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round(2);
                                item.attributes.customerdiscount = item.additionaldiscount;
                            }

                            item.discount = (item.qty * (item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount)).round(2);
                            item.baseprice = (item.price - item.additionaldiscount - (item.schemediscount / item.qty).round(2));
                        }
                    }
                    else {
                        if (typeof (item.attributes) == 'undefined' || item.attributes == null)
                            item.attributes = { 'customerdiscount': 0 };

                        var existingDiscountPercentage = (item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount) / item.barcodeDetail.price * 100;

                        if ($scope.CurrentBill.billdiscountinclude) {
                            if (existingDiscountPercentage < $scope.CurrentBill.customerdiscount) {
                                var addremaindiscount = $scope.CurrentBill.customerdiscount - existingDiscountPercentage;
                                item.additionaldiscount = (item.barcodeDetail.price * addremaindiscount / 100).round(2);
                                item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round(2);
                                item.attributes.customerdiscount = item.additionaldiscount;
                            }
                        }
                        else {
                            var addremaindiscount = $scope.CurrentBill.customerdiscount;
                            item.additionaldiscount = (item.barcodeDetail.price * addremaindiscount / 100).round(2);
                            item.additionaldiscountpercentage = (item.additionaldiscount / item.barcodeDetail.price * 100).round(2);
                            item.attributes.customerdiscount = item.additionaldiscount;
                        }

                        item.discount = (item.qty * (item.barcodeDetail.discount + item.barcodeDetail.additionaldiscount)).round(2);
                        item.baseprice = (item.price - item.additionaldiscount - (item.schemediscount / item.qty).round(2));
                    }



                    if (typeof (item.taxInfo) != 'undefined' && item.taxInfo != null && item.taxInfo.taxsplit != null) {
                        var taxitem = item.taxInfo.taxsplit.findTax(item.baseprice);
                        item.taxpercentage = taxitem.cgst + taxitem.sgst;
                    }

                    if (!$scope.CurrentBill.isinterstatesale) {
                        var basePrice = (item.baseprice / (100 + (item.taxpercentage)) * 100);
                        var halfTax = (basePrice * (item.taxpercentage / 2) / 100).round(5);
                        item.itemtax = (halfTax * 2).round(5);
                    }
                    else {
                        item.itemtax = (item.baseprice / (100 + item.taxpercentage)) * item.taxpercentage;
                    }

                    item.tax = (item.itemtax * item.qty).round(5);
                    item.taxid = item.barcodeDetail.taxid;
                    item.rate = (item.baseprice - item.itemtax).round(2);
                    item.receivable = (item.baseprice * item.qty).round(2);
                    item.saleamount = item.qty * item.rate;

                    $scope.CurrentBill.tax = $scope.CurrentBill.tax + item.tax;
                    $scope.CurrentBill.discount = ($scope.CurrentBill.discount + item.discount).round(2);
                    $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + item.receivable).round(2);
                    $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                    $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                    $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount + (item.additionaldiscount * item.qty);
                }
            });
            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
        }

        $scope.ChangeManualBillItem = function (source) {
            switch (source) {
                case 'price':
                    $scope.autoCoding.discount = ($scope.GetNumber($scope.autoCoding.price) * $scope.GetNumber($scope.autoCoding.discountpercentage) / 100).round($scope.system_salesroundingdigit);
                    break;
                case 'discount%':
                    $scope.autoCoding.discount = ($scope.GetNumber($scope.autoCoding.price) * $scope.GetNumber($scope.autoCoding.discountpercentage) / 100).round($scope.system_salesroundingdigit);
                    break;
                case 'discount':
                    var discount = $scope.GetNumber($scope.autoCoding.discount);
                    if (discount > 0)
                        $scope.autoCoding.discountpercentage = (discount / $scope.GetNumber($scope.autoCoding.price) * 100).round(2);
                    else
                        $scope.autoCoding.discountpercentage = 0;
                    break;
            }
            $scope.autoCoding.rate = $scope.GetNumber($scope.autoCoding.price) -
                $scope.GetNumber($scope.autoCoding.discount);
            $scope.autoCoding.amount = $scope.autoCoding.rate * $scope.GetNumber($scope.autoCoding.qty);
            $scope.SalesDetail.discount = $scope.GetNumber($scope.autoCoding.discount);
            $scope.SalesDetail.discountpercentage = $scope.GetNumber($scope.autoCoding.discountpercentage);
            $scope.SalesDetail.receivable = $scope.autoCoding.amount;
            $scope.SalesDetail.qty = $scope.GetNumber($scope.autoCoding.qty);
            $scope.SalesDetail.actualprice = $scope.GetNumber($scope.autoCoding.price);
        }

        $scope.ShowManualBillItem = function (result, product) {
            $scope.autoCoding.barcodeDetail = result;
            $scope.autoCoding.discountpercentage = product.discountvalue;
            $scope.setFocusqty();
        }

        $scope.AddManualBillItem = function () {
            $scope.SalesDetail.barcodeDetail.discount = $scope.GetNumber($scope.autoCoding.discount);
            $scope.SalesDetail.barcodeDetail.price = $scope.GetNumber($scope.autoCoding.price) - $scope.SalesDetail.barcodeDetail.discount;
            $scope.SalesDetail.qty = $scope.GetNumber($scope.autoCoding.qty);
            $scope.SalesDetail.price = $scope.GetNumber($scope.autoCoding.price) - $scope.SalesDetail.barcodeDetail.discount;
            $scope.SalesDetail.baseprice = $scope.GetNumber($scope.autoCoding.rate);
            $scope.SalesDetail.salerate = $scope.GetNumber($scope.autoCoding.price);
            $scope.SalesDetail.receivable = $scope.autoCoding.amount;

            $scope.autoCoding.lastbit = 0;
            $scope.AddBillItem();
            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.discountpercentage = "";
            $scope.autoCoding.discount = "";
            $scope.autoCoding.price = "";
            $scope.autoCoding.barcodeDetail = null;
            $scope.autoCoding.lastbit = "";
            $scope.setFocusautocoding();
        }

        $scope.ApplyExclusiveTax = function () {
            var price = $scope.SalesDetail.price;
            var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax(price, true);
            $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
            $scope.SalesDetail.etprice = price;
            $scope.SalesDetail.etdiscount = 0;
            $scope.SalesDetail.price = price * (100 + $scope.SalesDetail.taxpercentage) / 100;
            $scope.SalesDetail.salerate = $scope.SalesDetail.price;
            $scope.AddBillItemInternal();
        }

        $scope.AddBillItem = function () {
            if ($scope.system_posexclusivetax) {
                $scope.ApplyExclusiveTax();
            }
            else {
                $scope.AddBillItemInternal();
            }
            $scope.LocalContext.Post('$scope.CurrentBill', $scope.CurrentBill, false);
        }

        $scope.AddBillItemInternal = function () {

            var lastbitValue = Math.abs($scope.GetNumber($scope.autoCoding.lastbit));
            if (lastbitValue != 0) {

                $scope.SalesDetail.journalqty = Math.abs(parseFloat($scope.SalesDetail.qty)).round(2);
                $scope.SalesDetail.qty = lastbitValue;
            }
            else
                $scope.SalesDetail.journalqty = 0;

            if ($scope.IsSaleReturn && $rootScope.GetNumber($scope.SalesDetail.qty) >= 0)
                $scope.SalesDetail.qty = $scope.GetNumber($scope.SalesDetail.qty) * -1;

            if ($scope.IsSaleReturn && $scope.SalesDetail.qty == 0 && $scope.SalesDetail.stockisbit
                && $scope.SalesDetail.sellingmode == 3 && $scope.SalesDetail.oqty > 0) {
                $scope.SalesDetail.qty = $scope.SalesDetail.oqty * -1;
            }

            if ($scope.SalesDetail.sellingmode == 3) {
                $scope.SalesDetail.piece = $scope.IsSaleReturn ? -1 : 1;
                if (!$scope.sale_negativestock && $rootScope.GetNumber($scope.SalesDetail.stock).round(3) < $rootScope.GetNumber($scope.SalesDetail.qty)) {
                    $rootScope.Toaster.Error("Negative stock billing not allowed, available qty - " + $rootScope.GetNumber($scope.SalesDetail.stock));
                    return;
                }
                else if ($scope.sale_negativestock && !$scope.IsSaleReturn
                    && $scope.SalesDetail.stockisbit && $rootScope.GetNumber($scope.SalesDetail.qty) <= 0) {
                    $scope.SalesDetail.qty = $rootScope.GetNumber($scope.SalesDetail.oqty)
                }
            }
            else
                $scope.SalesDetail.piece = parseInt($scope.SalesDetail.qty);

            $scope.SalesDetail.additionaldiscount = 0;
            $scope.SalesDetail.additionaldiscountpercentage = 0;

            if (typeof ($scope.CurrentBill.customerid) != 'undefined' && $scope.CurrentBill.customerid > 0 &&
                typeof ($scope.CurrentBill.ccmargin) != 'undefined' && $scope.CurrentBill.ccmargin) {

                if (!$scope.CurrentBill.discountotp || $scope.CurrentBill.discountotpconfirmed) {

                    if (typeof ($scope.SalesDetail.attributes) == 'undefined' || $scope.SalesDetail.attributes == null)
                        $scope.SalesDetail.attributes = { 'customerdiscount': 0 };

                    if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                        $scope.SalesDetail.additionaldiscount = 0;

                    var newprice = $scope.SalesDetail.barcodeDetail.buyingprice + ($scope.SalesDetail.barcodeDetail.buyingprice * $scope.CurrentBill.ccmargin / 100).round($scope.system_salesroundingdigit);
                    newprice = newprice + (newprice * $scope.SalesDetail.taxpercentage / 100).round(4);
                    var newdiscount = ($scope.SalesDetail.barcodeDetail.price - $scope.SalesDetail.barcodeDetail.discount + $scope.SalesDetail.barcodeDetail.additionaldiscount) - newprice;

                    $scope.SalesDetail.additionaldiscount += newdiscount;
                }
            }

            if (typeof ($scope.CurrentBill.customerid) != 'undefined' && $scope.CurrentBill.customerid > 0 &&
                typeof ($scope.CurrentBill.customerdiscount) != 'undefined' && $scope.CurrentBill.customerdiscount > 0) {


                if (!$scope.CurrentBill.discountotp || $scope.CurrentBill.discountotpconfirmed) {

                    if (typeof ($scope.SalesDetail.attributes) == 'undefined' || $scope.SalesDetail.attributes == null)
                        $scope.SalesDetail.attributes = { 'customerdiscount': 0 };

                    var existingDiscountPercentage = ($scope.SalesDetail.barcodeDetail.discount + $scope.SalesDetail.barcodeDetail.additionaldiscount) / $scope.SalesDetail.barcodeDetail.price * 100;

                    var product = $scope.Products.find(function (item) {
                        return item.id == $scope.SalesDetail.productid;
                    });
                    if (typeof (product) != 'undefined' && product != null && product.discountmode == 4) {
                        //no bill level discounts.
                    }
                    else {
                        if ($scope.CurrentBill.billdiscountinclude) {
                            if (existingDiscountPercentage < $scope.CurrentBill.customerdiscount) {
                                var addremaindiscount = $scope.CurrentBill.customerdiscount - existingDiscountPercentage;
                                $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.barcodeDetail.price * addremaindiscount / 100).round(2);
                                $scope.SalesDetail.additionaldiscountpercentage = ($scope.SalesDetail.additionaldiscount / $scope.SalesDetail.barcodeDetail.price * 100).round(2);
                                $scope.SalesDetail.attributes.customerdiscount = $scope.SalesDetail.additionaldiscount;
                            }
                        }
                        else {
                            var addremaindiscount = $scope.CurrentBill.customerdiscount;
                            $scope.SalesDetail.additionaldiscount = ($scope.SalesDetail.barcodeDetail.price * addremaindiscount / 100).round(2);
                            $scope.SalesDetail.additionaldiscountpercentage = ($scope.SalesDetail.additionaldiscount / $scope.SalesDetail.barcodeDetail.price * 100).round(2);
                            $scope.SalesDetail.attributes.customerdiscount = $scope.SalesDetail.additionaldiscount;
                        }
                    }
                }
            }

            if ($scope.SalesDetail.barcodeDetail.discount == 0 && $scope.CurrentBill.ccloyelty) {
                var product = $scope.Products.find(function (item) {
                    return item.id == $scope.SalesDetail.productid;
                });

                if (typeof (product) != 'undefined' &&
                    product != null &&
                    product.discountmode == 3 &&
                    product.discountvalue > 0) {

                    var newdiscount = ($scope.SalesDetail.price * product.discountvalue / 100).round(2);
                    $scope.SalesDetail.additionaldiscount += newdiscount;
                    $scope.SalesDetail.additionaldiscountpercentage = product.discountvalue;
                    $scope.SalesDetail.attributes.customerdiscount = $scope.SalesDetail.additionaldiscount;
                    $scope.SalesDetail.baseprice -= newdiscount;
                }
            }
            else {
                var product = $scope.Products.find(function (item) {
                    return item.id == $scope.SalesDetail.productid;
                });

                if (typeof (product) != 'undefined' &&
                    product != null &&
                    product.discountmode == 5 &&
                    product.discountvalue > 0) {

                    if ($scope.GetNumber(product.discountvalue) > $scope.GetNumber($scope.SalesDetail.discountpercentage)) {
                        var addlndiscount = $scope.GetNumber(product.discountvalue) - $scope.GetNumber($scope.SalesDetail.discountpercentage);
                        var newdiscount = ($scope.SalesDetail.price * addlndiscount / 100).round(2);
                        $scope.SalesDetail.additionaldiscount += newdiscount;
                        $scope.SalesDetail.additionaldiscountpercentage = addlndiscount;
                        $scope.SalesDetail.attributes.customerdiscount = $scope.SalesDetail.additionaldiscount;
                        $scope.SalesDetail.baseprice -= newdiscount;
                    }
                }
            }

            $scope.SalesDetail.discount = ($scope.SalesDetail.qty * ($scope.SalesDetail.barcodeDetail.discount + $scope.SalesDetail.barcodeDetail.additionaldiscount)).round(2);

            if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                $scope.SalesDetail.additionaldiscount = 0;

            if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount))
                $scope.SalesDetail.schemediscount = 0;

            $scope.SalesDetail.baseprice = ($scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount - ($scope.SalesDetail.schemediscount / $scope.SalesDetail.qty).round(2));


            if (typeof ($scope.SalesDetail.taxInfo) != 'undefined' && $scope.SalesDetail.taxInfo != null &&
                $scope.SalesDetail.taxInfo.taxsplit != null) {
                var taxitem = $scope.SalesDetail.taxInfo.taxsplit.findTax($scope.SalesDetail.baseprice);
                $scope.SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
            }

            if (!$scope.CurrentBill.isinterstatesale) {
                var basePrice = ($scope.SalesDetail.baseprice / (100 + ($scope.SalesDetail.taxpercentage)) * 100);
                var halfTax = (basePrice * ($scope.SalesDetail.taxpercentage / 2) / 100).round(5);
                $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
            }
            else {
                $scope.SalesDetail.itemtax = ($scope.SalesDetail.baseprice / (100 + $scope.SalesDetail.taxpercentage)) * $scope.SalesDetail.taxpercentage;
            }

            $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round(5);
            $scope.SalesDetail.taxid = $scope.SalesDetail.barcodeDetail.taxid;
            $scope.SalesDetail.rate = ($scope.SalesDetail.baseprice - $scope.SalesDetail.itemtax).round(2);
            //$scope.SalesDetail.salerate = ($scope.SalesDetail.rate + $scope.SalesDetail.itemtax).round(2);
            $scope.SalesDetail.receivable = ($scope.SalesDetail.baseprice * $scope.SalesDetail.qty).round(2);
            $scope.SalesDetail.stockid = $scope.SalesDetail.barcodeDetail.stockid;
            $scope.SalesDetail.salablegoodsid = $scope.SalesDetail.barcodeDetail.id;
            $scope.SalesDetail.companyid = $scope.SalesDetail.barcodeDetail.companyid;
            $scope.SalesDetail.qty = parseFloat($scope.SalesDetail.qty);
            $scope.SalesDetail.schemediscount = 0;
            $scope.SalesDetail.schemereward = 0;
            $scope.SalesDetail.schemediscountcoupen = 0;

            if (isNaN($scope.SalesDetail.qty))
                $scope.SalesDetail.qty = 0;

            $scope.SalesDetail.saleamount = $scope.SalesDetail.qty * $scope.SalesDetail.rate;

            if ($scope.SalesDetail.qty == 0) {
                $rootScope.Toaster.Error("Zero Qty Can't be added");
                return;
            }
            if (!$scope.HSNLimit.validate($scope.CurrentBill.Billitems, $scope.SalesDetail)) {
                $rootScope.Toaster.Error("Can't add more than " + $scope.HSNLimit.max + ' hsn codes in single bill');
                return;
            }
            $scope.CalcualteTotal($scope.SalesDetail);
            //$scope.SalesDetail.additionaldiscount = 0;
            //$scope.CurrentBill.Billitems.push($scope.SalesDetail);
            $scope.CurrentBill.Billitems.splice(0, 0, $scope.SalesDetail);
            var insertedbillitem = $scope.SalesDetail;
            $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
            if (!$scope.BeginGridTransaction)
                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
            $scope.ConfirmWidow.SetEnable($scope.CurrentBill.Billitems.length > 0);

            if ($scope.autoCoding && $scope.autoCoding.issingleprice) {
                $scope.UpdateOffer();
            }
            else if (insertedbillitem.attributes &&
                insertedbillitem.attributes.ismultiprice) {
                $scope.ShowMultiplePrice();
            }
            else {
                $scope.UpdateOffer();
            }
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
                $scope.HasPromotionApplied = true;
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
                                v.schemediscountpercentage = ((v.schemediscount / v.qty) / (v.price + (v.discount / v.qty)) * 100).round($scope.system_salesroundingdigit);
                                if (typeof ($scope.Promotions) != 'undefined' && $scope.Promotions.length > 0) {
                                    var pinfo = $scope.Promotions.find(function (item) {
                                        return item.id == v.schemeid;
                                    });
                                    if (typeof (pinfo) != 'undefined' && pinfo != null)
                                        v.scheme = pinfo.name;
                                }
                                if (offerbillitem.attributes && offerbillitem.attributes != null &&
                                    offerbillitem.attributes.aschemeid && offerbillitem.attributes.aschemeid > 0) {
                                    if (v.attributes == undefined || v.attributes == null)
                                        v.attributes = {};
                                    v.attributes.aschemeid = offerbillitem.attributes.aschemeid;
                                    v.attributes.aschemediscount = offerbillitem.attributes.aschemediscount;
                                }

                                if (offerbillitem.attributes && offerbillitem.attributes != null &&
                                    offerbillitem.attributes.freeitems && offerbillitem.attributes.freeitems != null) {
                                    v.attributes.freeitems = offerbillitem.attributes.freeitems;
                                }

                            }
                            else if (typeof (offerbillitem) != 'undefined' && offerbillitem.schemeid > 0 &&
                                offerbillitem.attributes && offerbillitem.attributes.freeitems &&
                                offerbillitem.attributes.freeitems != null) {
                                v.schemediscount = 0;
                                v.schemereward = 0;
                                v.schemediscountcoupen = 0;
                                v.schemeid = offerbillitem.schemeid;
                                if (v.attributes == undefined || v.attributes == null)
                                    v.attributes = {};
                                v.attributes.freeitems = offerbillitem.attributes.freeitems;
                                v.style = { "background-color": "lightgrey" };
                                v.schemediscountpercentage = 0;
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

                                if (typeof (v.attributes) != 'undefined' &&
                                    v.attributes != null &&
                                    typeof (v.attributes.customerdiscount) != 'undefined' &&
                                    v.attributes.customerdiscount > 0) {

                                    var item = v;

                                    $scope.CurrentBill.tax = $scope.CurrentBill.tax - item.tax;
                                    $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
                                    $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                                    $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount - (item.additionaldiscount * item.qty);


                                    item.attributes.customerdiscount = 0;
                                    item.additionaldiscount = 0;
                                    item.additionaldiscountpercentage = 0;
                                    item.baseprice = (item.price - item.additionaldiscount - (item.schemediscount / item.qty).round(2));
                                    if (typeof (item.taxInfo) != 'undefined' && item.taxInfo != null && item.taxInfo.taxsplit != null) {
                                        var taxitem = item.taxInfo.taxsplit.findTax(item.baseprice);
                                        item.taxpercentage = taxitem.cgst + taxitem.sgst;
                                    }

                                    if (!$scope.CurrentBill.isinterstatesale) {
                                        var basePrice = (item.baseprice / (100 + (item.taxpercentage)) * 100);
                                        var halfTax = (basePrice * (item.taxpercentage / 2) / 100).round(5);
                                        item.itemtax = (halfTax * 2).round(5);
                                    }
                                    else {
                                        item.itemtax = (item.baseprice / (100 + item.taxpercentage)) * item.taxpercentage;
                                    }

                                    item.tax = (item.itemtax * item.qty).round(5);
                                    item.taxid = item.barcodeDetail.taxid;
                                    item.rate = (item.baseprice - item.itemtax).round(2);
                                    item.receivable = (item.baseprice * item.qty).round(2);
                                    item.saleamount = item.qty * item.rate;

                                    $scope.CurrentBill.tax = $scope.CurrentBill.tax + item.tax;
                                    $scope.CurrentBill.discount = ($scope.CurrentBill.discount + item.discount).round(2);
                                    $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + item.receivable).round(2);
                                    $scope.CurrentBill.additionaldiscount = $scope.CurrentBill.additionaldiscount + (item.additionaldiscount * item.qty);
                                    $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                                    $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                                }

                                $scope.UpdateTax(v);
                                $scope.UpdateItemInfo(v, true);
                            }
                        });
                    }
                    $scope.CurrentBill.schemediscount = result.schemediscount;
                    $scope.CurrentBill.schemereward = result.schemereward;
                    $scope.CurrentBill.schemediscountcoupen = result.schemediscountcoupen;
                    $scope.CurrentBill.HasPendingPromotion = ($scope.IsBulkSale && $scope.CurrentBill.HasPendingPromotion ? true : result.HasPendingPromotion);
                    $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                    $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
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
            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + saleitem.tax).round(5);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + saleitem.discount).round(2);
            $scope.CurrentBill.schemediscount = ($scope.CurrentBill.schemediscount + saleitem.schemediscount).round(2);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + saleitem.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + (saleitem.additionaldiscount * saleitem.qty)).round(2);
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
            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
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

            if ($scope.system_posdisablerefund && !$scope.system_supportcustcreditnote && !$scope.IsSaleReturn) {
                if ($scope.CurrentBill.receivable < 0 && Math.abs($scope.CurrentBill.receivable) >= 1) {
                    $rootScope.Toaster.Error("Refund disabled, please contact your manager.");
                    return false;
                }
            }

            if ($scope.system_salecustomermandatory) {
                if ((typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0)
                    && (typeof ($scope.CurrentBill.phoneno) == 'undefined' || $scope.CurrentBill.phoneno == '')) {
                    $rootScope.Toaster.Error('Please select the customer');
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
                    $rootScope.Toaster.Error('Please select the salesman code for all items.');
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

                if (!$scope.IsEstimate) {
                    if (
                        typeof ($scope.CurrentBill.billattributes) == 'undefined' ||
                        $scope.CurrentBill.billattributes == null ||
                        typeof ($scope.CurrentBill.billattributes.approvedby) == 'undefined' ||
                        $scope.CurrentBill.billattributes.approvedby == null ||
                        $scope.CurrentBill.billattributes.approvedby <= 0) {
                        $rootScope.Toaster.Error('Please select approver');
                        $scope.setFocusbulkapprovedby();
                        return false;
                    }

                    if ($scope.BulkSaleReason.length > 0 &&
                        (typeof ($scope.CurrentBill.returnreasonid) == 'undefined' ||
                            $scope.CurrentBill.returnreasonid == null ||
                            $scope.CurrentBill.returnreasonid <= 0)) {
                        $rootScope.Toaster.Error('Please select reason');
                        return false;
                    }
                    //if (!$rootScope.ShowDeliveryChallan) {
                    if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                        $rootScope.Toaster.Error('Please select the customer');
                        $scope.ShowCustomerDialog();
                        return false;
                    }
                    //}
                }
                else {
                    $scope.CurrentBill.billtype = 20;
                }

            }
            else {
                if ($scope.CurrentBill.iscreditbill && (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0)) {
                    $rootScope.Toaster.Error('Please select the customer');
                    $scope.ShowCustomerDialog();
                    return false;
                }
                $scope.CurrentBill.billtype = $scope.CurrentBill.iscreditbill ? 1 : 0;
                if ($scope.sale_exchangerefund > 0 && $scope.CurrentBill.receivable < 0) {
                    if (Math.abs($scope.CurrentBill.receivable) > $scope.sale_exchangerefund) {
                        $rootScope.Toaster.Error('Exchange refund only allowed upto ' + $scope.sale_exchangerefund + '.!');
                        return false;
                    }
                }
            }

            if ($scope.CurrentBill.isinterstatesale) {
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

                if ($scope.CompanyStateID == null || $scope.CompanyStateID <= 0) {
                    $rootScope.Toaster.Error('Please update company state for IGST bill');
                    $scope.CurrentBill.isinterstatesale = false;
                    return false;
                }

                if ($scope.CurrentBill.customerstateid == 0) {
                    $rootScope.Toaster.Error('Customer state cant be null for IGST bill');
                    return false;
                }

                if ($scope.CurrentBill.customerstateid == $scope.CompanyStateID) {
                    $rootScope.Toaster.Error('Customer state and current company state are same cant allow to create IGST bill');
                    return false;
                }

                if ($scope.CurrentBill.customergst != null && $scope.CurrentBill.customergst != '') {
                    if ($scope.CurrentBill.customergst.substring(0, 2) == $scope.CompanyGSTNO.substring(0, 2)) {
                        $rootScope.Toaster.Error('Customer gst and current company gst are belongs to same state cant allow to create IGST bill');
                        return;
                    }
                }
            }
            else {
                if ($scope.CurrentBill.customerid != null
                    && $scope.CurrentBill.customerid > 0) {

                    /*if ($scope.CurrentBill.customerstateid > 0 &&
                        $scope.CurrentBill.customerstateid != $scope.CompanyStateID) {
                        $rootScope.Toaster.Notice('Customer state and current company state are different cant allow to create non IGST bill');
                        return;
                    }*/

                    if ($scope.CurrentBill.customergst != null && $scope.CurrentBill.customergst != '') {
                        if ($scope.CurrentBill.customergst.substring(0, 2) != $scope.CompanyGSTNO.substring(0, 2)) {
                            $rootScope.Toaster.Error('Customer gst and current company gst are not beloongs to same state cant allow to create non IGST bill');
                            return;
                        }
                    }
                }
            }

            if ($scope.system_taxrangeautodiscount &&
                !$scope.IsSaleReturn) {

                var customerdiscount = 0;
                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    var tax = v.tax;
                    if (v.taxid && v.taxid != null &&
                        v.taxpercentage == 5 || v.taxpercentage == 12) {
                        if (v.taxInfo == undefined || v.taxInfo == null) {
                            v.taxInfo = $scope.SalesTax.find(function (arritem) {
                                return arritem.id == SalesDetail.taxid;
                            });
                        }
                        if (v.taxInfo.type == 2 &&
                            v.taxInfo.taxsplit.avoidrangefrom != 0 &&
                            v.taxInfo.taxsplit.avoidrangeto != 0) {
                            var price = (v.receivable / v.qty);
                            if (v.taxInfo.taxsplit.avoidrangefrom < price &&
                                v.taxInfo.taxsplit.avoidrangeto >= price) {
                                var newaddiscount = ((price - v.taxInfo.taxsplit.avoidrangefrom) / v.qty).round(2);
                                customerdiscount += newaddiscount;
                                v.additionaldiscount += newaddiscount;
                                v.additionaldiscountpercentage = ((v.additionaldiscount / v.price) * 100).round(2);
                                v.receivable = v.receivable - (newaddiscount * v.qty);
                                $scope.UpdateTax(v);
                            }
                        }
                    }
                })

                if (customerdiscount > 0) {
                    $scope.CurrentBill.tax = 0;
                    $scope.CurrentBill.discount = 0;
                    $scope.CurrentBill.receivable = 0;
                    $scope.CurrentBill.rounding = 0;
                    $scope.CurrentBill.total = 0;
                    $scope.CurrentBill.additionaldiscount = 0;
                    $.each($scope.CurrentBill.Billitems, function (k, v) {
                        $scope.CurrentBill.tax += v.tax;
                        $scope.CurrentBill.discount += v.discount;
                        $scope.CurrentBill.additionaldiscount += (v.qty * v.additionaldiscount);
                        $scope.CurrentBill.receivable += v.receivable;
                    });
                    $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                    $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                    $rootScope.Toaster.Notice('Auto discount for customer issued - ' + customerdiscount.round(0));
                    $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                }
            }

            $scope.CurrentBill.locationid = $rootScope.LocationInfo.location.id;
            $scope.CurrentBill.counterid = $rootScope.LocationInfo.counter.id;
            $scope.CurrentBill.countername = $scope.CounterName;
            $scope.CurrentBill.floorname = $scope.floorname;
            $scope.CurrentBill.floorid = $rootScope.LocationInfo.floorid;

            if ($scope.system_creditbillotp &&
                ($scope.CurrentBill.creditotpconfirmed == undefined
                || $scope.CurrentBill.creditotpconfirmed == null
                || $scope.CurrentBill.creditotpconfirmed != true)) {

                var request = {
                    "Item": $scope.CurrentBill,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BillService/SendCreditOTP",
                    data: srequest
                }).success(function (result) {
                    $rootScope.Prompt('OTP Approval', 'Enter OTP', function (value) {
                        $scope.ConfirmCreditOTP(result,value);
                    });
                });

                return false;
            }
            return true;
        }

        $scope.ConfirmCreditOTP = function (issuedotp,otp) {
            if (typeof (otp) == 'undefined' || otp == '' || otp.length < 4) {
                $rootScope.Toaster.Notice('Customer discount disabled');
                $scope.CurrentBill.creditotpconfirmed = false;
                return;
            }
            restService.Get('BillService/ConfirmCreditOTP/' + otp + '/' + issuedotp, true).then(function (results) {
                if (results) {
                    $scope.CurrentBill.creditotpconfirmed = true;
                    $scope.Save(false, true);
                }
                else {
                    $rootScope.Prompt('OTP Approval', 'Enter OTP', function (value) {
                        $scope.ConfirmCreditOTP(issuedotp, value);
                    });
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

        $scope.Save = function (requestapproval, print) {

            if (!$scope.HasPromotionApplied && (!$scope.DisablePromotionRerun || $scope.IsEstimate || $scope.IsBulkSale)) {
                $rootScope.Toaster.Notice('Wait for offer updations.');
                $scope.FindOffers(function () {
                    $scope.HasPromotionApplied = true;
                    $rootScope.Toaster.Info('You can save now.');
                    $scope.setFocusautocoding();
                });
                return;
            }


            if (typeof ($scope.CurrentBill.Billitems) == 'undefined' ||
                $scope.CurrentBill.Billitems == null ||
                $scope.CurrentBill.Billitems.length == 0) {
                return;
            }


            if (!$scope.ValidateBill())
                return;

            
            if ($scope.CurrentBill.id > 0 && (!$scope.DisablePromotionRerun || $scope.IsEstimate || $scope.IsBulkSale)) {

                if ($scope.IsSaleReturn || !$scope.system_supportbilledit) {

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

                    $.each($scope.CurrentBill.Billitems, function (k, v) {
                        if (typeof (v.barcodeDetail) == 'undefined') {
                            if ($scope.GetNumber(v.discount) > 0 && $scope.GetNumber($scope.CurrentBill.discount) > 0) {
                                v.discount = v.discount - v.schemediscount;
                                $scope.CurrentBill.discount = $scope.CurrentBill.discount - v.schemediscount;
                                v.barcodeDetail = {
                                    'discount': v.discount / v.qty, 'additionaldiscount': v.additionaldiscount,
                                    'taxid': v.taxid, 'stockid': v.stockid, 'salablegoodsid': v.salablegoodsid,
                                    'companyid': v.companyid
                                };
                            }
                        }
                    });

                    if ($scope.CurrentBill.billno != '' && $scope.IsSaleReturn) {
                        $scope.CurrentBill.returnbillno = $scope.CurrentBill.billno;
                        $scope.CurrentBill.returnbillid = $scope.CurrentBill.id;
                    }

                    $scope.CurrentBill.billno = '';
                    $scope.CurrentBill.id = 0;

                    if ($scope.IsBulkSale) {
                        if ($scope.CurrentBill.isbulkreturngoods) {
                            if ($scope.CurrentBill.total > 0)
                                $scope.ReverseBillValue();
                        }
                        else {
                            $scope.CurrentBill.returnbillno = '';
                        }
                    }
                }
            }

            if ($scope.system_supportdeliveryrack && $scope.system_deliveryrackcard) {
                if ($scope.IsSaleReturn) {
                    $scope.CurrentBill.billattributes.deliveryrackcard = null;
                    $scope.CurrentBill.billattributes.deliveryrack = null;
                }
                else if (($scope.CurrentBill.billattributes == undefined || $scope.CurrentBill.billattributes == null) ||
                    ($scope.CurrentBill.billattributes.deliveryrack == undefined || $scope.CurrentBill.billattributes.deliveryrack == null) ||
                    ($scope.CurrentBill.billattributes.deliveryrackcard == undefined || $scope.CurrentBill.billattributes.deliveryrackcard == null)) {

                    var haspossitiveqty = $scope.CurrentBill.Billitems.find(function (item) {
                        return item.qty > 0;
                    });

                    if (haspossitiveqty && haspossitiveqty != null) {
                        $rootScope.Toaster.Error('Delivery rack number missing');
                        return;
                    }
                }
            }

            if ($scope.CurrentBill.id == undefined ||
                $scope.CurrentBill.id == null ||
                $scope.CurrentBill.id == '' ||
                $scope.CurrentBill.id == 0) {
                $scope.CurrentBill.uniqueuid = $scope.CurrentBill.uniqueuid == undefined ? $scope.CreateUID() : $scope.CurrentBill.uniqueuid;
            }

            if ($scope.AutoSettlement && (!$scope.IsSaleReturn) && (!$scope.IsBulkSale) &&
                (!$scope.CurrentBill.iscreditbill || $scope.system_crsettlementonbill)) {
                $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                $scope.ShowSettlement($scope.CurrentBill);
                return;
            };


            var request = {
                "Item": $scope.CurrentBill
            }

            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;

            request.Item.printtemplateid = $scope.PrinterData.TemplateID;
            request.Item.papersize = $scope.PrinterData.PaperSize;
            request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
            request.Item.saledirectpdf = $scope.PrinterData.PaperSize == 9 || $scope.system_saledirectpdf;
            request.Item.directpdf = $scope.PrinterData.PaperSize == 9 || $scope.system_saledirectpdf;
            $http({
                method: 'POST',
                url: "BillService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {

                $scope.SaveRequest = false;
                if (result.id > 0) {
                    $scope.DeliveryRackInformation = '';
                    $scope.ErrorMessage = "Successfully saved";
                    //if (print) {
                    //    $scope.Print(result.Bills[0]);
                    //}

                    if (print) {
                        if (result.printdocument == null || result.printdocument == '') {
                            $.each(result.Bills, function (k, v) {
                                $scope.Print(v);
                                $scope.PrintCopy.Send(v.billcopy);
                            });
                        }
                        else {
                            $scope.Print(result);
                            $scope.PrintCopy.Send(result.billcopy);
                        }
                    }

                    if ($scope.DiscountQueue && !$scope.discounttimerenabled && $scope.CurrentBill.additionaldiscount > 0) {
                        $scope.discounttimerenabled = true;
                        $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                    }

                    $scope.AddBillHistory(result, false, true)
                    $scope.CreateNew(true);

                    if ($scope.DiscountQueue) {
                        $scope.AllowAdditionalDiscount = false;
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

        $scope.SearchCustomerByWalkinToken = function () {
            $scope.CurrentBill.billattributes.remarks
            if ($scope.CurrentBill.billattributes == 'undefined' || $scope.CurrentBill.billattributes == null
                || $scope.CurrentBill.billattributes.remarks == "") {
                return;
            }

            restService.Get('CustomerService/SearchByWalkinToken?id=' + $scope.CurrentBill.billattributes.remarks + '&lid=' + $rootScope.LocationInfo.location.id, true).then(function (results) {
                if (results != 'undefined' && results != null) {
                    $scope.CurrentBill.phoneno = results.mobileno;
                    $scope.ApplyCustomerInfo(results);

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
                    if ($scope.UserInfo.p_SUPPORTCREDITBILL.view)
                        $scope.setFocuscreditbill();
                    else
                        $scope.setFocuscustomername();
                }
            });
        }

        $scope.SearchCustomer = function () {
            if ($scope.CurrentBill.phoneno == null || $scope.CurrentBill.phoneno == "") {
                $scope.CurrentBill.customername = "";
                $scope.setFocuscreditbill();
                return;
            }
            if (!$scope.sale_allowchangeofcustomer && $scope.CurrentBill.customerid != 'undefined' && $scope.CurrentBill.customerid != null
                && $scope.CurrentBill.customerid != 0 && $scope.CurrentBill.Billitems.length > 0) {
                $rootScope.Toaster.Error("Customer Already selected, Please clear all the barcode if you want to change.");
                return;
            }

            restService.Get('CustomerService/SearchByPhoneNumber?id=' + $scope.CurrentBill.phoneno + '&lid=' + $rootScope.LocationInfo.location.id, true).then(function (results) {
                $scope.ApplyCustomerInfo(results);

                if ($scope.CurrentBill.billattributes == undefined ||
                    $scope.CurrentBill.billattributes == null) {
                    $scope.CurrentBill.billattributes = {};
                }

                $scope.CurrentBill.billattributes.remarks = results.gstno;
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
                if (results && results.id && results.id > 0 && $scope.UserInfo.p_SUPPORTCREDITBILL.view)
                    $scope.setFocuscreditbill();
                else
                    $scope.setFocuscustomername();

                if (results && results != null && results.id > 0 &&
                    !$scope.IsEstimate &&
                    !$scope.hassale_deliverycardprefix &&
                    $scope.system_supportdeliveryrack) {
                    $scope.LoadEstimateBillFromDeliveryRack(results);
                }
            });
        }

        $scope.LoadEstimateBillFromDeliveryRack = function (customer) {

            var request = {
                "Item": {
                    'customerid': customer.customerid ? customer.customerid : customer.id,
                    'id': customer.rackid,
                    'locationid': $rootScope.LocationInfo.location.id,
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DeliveryrackService/SearchBill",
                data: srequest
            }).success(function (result) {

                if (result.length == 0) return;

                var currentbillattribute = $scope.CurrentBill.billattributes;
                var DeliveryRackInformation = $scope.DeliveryRackInformation;
                $scope.CreateNew(false);
                $scope.DeliveryRackInformation = DeliveryRackInformation;
                $scope.CurrentBill.billattributes = currentbillattribute;
                $scope.CurrentBill.billattributes.ebillno = [];
                $scope.CurrentBill.billattributes.tailoringorderno = result[0].billattributes.tailoringorderno;
                $scope.CurrentBill.billattributes.tailoringorderid = result[0].billattributes.tailoringorderid;

                $.each(result, function (billkey, bill) {
                    $scope.CurrentBill.billattributes.ebillno.push({ 'billno': bill.billno, 'countername': bill.billattributes.countername });
                    bill.isMasterBill = true;
                    var billitems = (JSON.parse(JSON.stringify(bill.Billitems)));
                    $.each(billitems, function (k, v) {
                        v.returnbillitemid = v.id;
                        v.id = 0;
                        if (v.attributes && v.attributes.serialno &&
                            v.attributes.serialno != null && v.attributes.serialno != '') {
                            v.displaycode = v.attributes.serialno;
                        }
                        else {
                            v.displaycode = v.barcode;
                        }
                        $scope.CurrentBill.Billitems.push(v);
                        $scope.CalcualteTotal(v);
                    });
                })

                $scope.ShowAdditional = (result[0].additionalcharges && result[0].additionalcharges > 0);
                if ($scope.OnlineCounter || $scope.ShowAdditional) {
                    $scope.CurrentBill.additionalchargeslist = result[0].additionalchargeslist;
                    $.each(result[0].additionalchargeslist, function (k, v) {
                        var item = $scope.AdditionalChargeList.find(function (Sitem) {
                            return Sitem.id == v.additionalchargestypeid;
                        });
                        v.Name = item.name;
                    })
                    $scope.AdditionalChargeGridConfig();
                    $scope.ComputeAddtionalChargeValues();
                }

                $scope.CurrentBill.phoneno = customer.mobileno;
                $scope.ApplyCustomerInfo(customer);

                $scope.setFocusautocoding();
                $timeout(function () {
                    $scope.FocusAutoBarcode();
                }, 200);

            });
        }

        $scope.SearchCustomerByLoyaltyNo = function () {
            if ($scope.CurrentBill.loyeltyno == null || $scope.CurrentBill.loyeltyno == "") {
                $scope.CurrentBill.customername = "";
                $scope.setFocuscreditbill();
                return;
            }
            restService.Get('CustomerService/SearchByLoyaltyNo?id=' + $scope.CurrentBill.loyeltyno + '&lid=' + $rootScope.LocationInfo.location.id, true).then(function (results) {
                $scope.ApplyCustomerInfo(results);
                $scope.CurrentBill.phoneno = results.mobileno;

                if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                    $scope.CurrentBill.billattributes = {};
                $scope.CurrentBill.billattributes.remarks = results.loyeltyno;

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
                if ($scope.UserInfo.p_SUPPORTCREDITBILL.view)
                    $scope.setFocuscreditbill();
                else
                    $scope.setFocuscustomername();
            });
        }

        $scope.ApplyCustomerInfo = function (results) {
            $scope.CurrentBill.customername = results.name;
            $scope.CurrentBill.customerid = results.id;
            $scope.CurrentBill.customergst = results.gstno;
            $scope.CurrentBill.customerstateid = results.stateid;
            $scope.CurrentBill.customerdiscount = results.discount;
            $scope.CurrentBill.disableloyalty = results.disableloyalty;

            $scope.CurrentBill.ccloyelty = !results.disableloyalty && results.loyeltyno != null &&
                results.loyeltyno != '' && results.loyeltyno.length > 1;
            $scope.CurrentBill.ccmargin = results.ccmargin;
            $scope.CurrentBill.ccmarginmode = results.ccmarginmode;

            $scope.CurrentBill.rewardunit = results.rewardunit;
            $scope.CurrentBill.billdiscountinclude = results.billdiscountinclude;
            $scope.CurrentBill.discountotp = results.discountotp;

            if (results.categoryid != null && results.categoryid > 0) {
                var itm = $scope.CustomerCategory.find(function (arritem) {
                    return arritem.id == results.categoryid;
                });

                if (itm != 'undefined' && itm != null) {
                    $rootScope.Toaster.Info(itm.name + ' Customer selected');
                }
            }
            if ($scope.CurrentBill.disableloyalty != 'undefined' && $scope.CurrentBill.disableloyalty != null
                && $scope.CurrentBill.disableloyalty == true) {
                $rootScope.Toaster.Info('Loyalty Points disabled.');
                return;
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

        $scope.SearchBarcode = function () {
            if (($scope.SalesDetail.barcode == "0")
                && $scope.CurrentBill.Billitems.length > 0) {
                $scope.setFocusSave();
                return;
            }
            if ($scope.SalesDetail.barcode == null || $scope.SalesDetail.barcode == "") return;

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
                if ($scope.CurrentBill.Billitems[index].barcode == $scope.SalesDetail.barcode) {

                    if (($scope.CurrentBill.Billitems[index].qty < 0 && $scope.IsSaleReturn) ||
                        ($scope.CurrentBill.Billitems[index].qty > 0 && !$scope.IsSaleReturn)) {

                        if (!$scope.CurrentBill.Billitems[index].qtyeditable) {
                            $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[index]);
                        }
                        $scope.SalesDetail.barcode = "";
                        return;
                    }
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
                }

                if (results.id != 0) {

                    if (typeof (results.imageid) != 'undefined' && results.imageid != null && results.imageid.length > 0) {
                        $scope.currentimageid = results.imageid;
                        $scope.ShowImage = true;
                    }
                    else {
                        $scope.ShowImage = false;
                    }

                    $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty + results.jqty));
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.SalesDetail.attributes = {
                        'serialno': results.serialno, 'designid': results.designid,
                        'size': results.sizename, 'brand': results.brandname, 'typename': results.typename,
                        'colourname': results.colourname,
                        'materialname': results.materialname,
                        'stylename': results.stylename,
                        'patternname': results.patternname,
                        'mrp': results.mrp,
                        'ismultiprice': results.ismultiprice
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

                        $scope.AddBarcode(results);
                        //results.iqty <= 1 &&
                        if (results.price != 0)
                            $scope.setFocusSalesman();
                    }
                }
            });
        }

        $scope.UpdateTax = function (SalesDetail) {
            if (typeof (SalesDetail.taxInfo) == 'undefined') {
                SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                    return arritem.id == SalesDetail.taxid;
                });
            }
            if (SalesDetail.taxInfo != null) {
                if (SalesDetail.taxInfo.taxsplit == null) {
                    SalesDetail.taxInfo.taxsplit = { 'taxpercentage': SalesDetail.taxInfo.taxpercentage };
                }
                SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
            }

            if (typeof (SalesDetail.taxInfo) != 'undefined' &&
                SalesDetail.taxInfo != null) {
                SalesDetail.taxpercentage = SalesDetail.taxInfo.taxpercentage;
                SalesDetail.taxname = SalesDetail.taxInfo.name;
                if (typeof (SalesDetail.additionaldiscount) == 'undefined' || isNaN(SalesDetail.additionaldiscount))
                    SalesDetail.additionaldiscount = 0;

                if (typeof (SalesDetail.schemediscount) == 'undefined' || isNaN(SalesDetail.schemediscount))
                    SalesDetail.schemediscount = 0;

                var baseprice = (SalesDetail.price - SalesDetail.additionaldiscount - (SalesDetail.schemediscount / SalesDetail.qty).round(2));
                if (SalesDetail.taxInfo != null && SalesDetail.taxInfo.taxsplit != null) {
                    var taxitem = SalesDetail.taxInfo.taxsplit.findTax(baseprice);
                    SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                }

                if (!$scope.CurrentBill.isinterstatesale) {
                    var basePrice = (SalesDetail.qty * baseprice / (100 + (SalesDetail.taxpercentage)) * 100);
                    var halfTax = (basePrice * (SalesDetail.taxpercentage / 2) / 100).round(5);
                    $scope.SalesDetail.itemtax = (halfTax * 2).round(5);
                }
                else {
                    SalesDetail.tax = (SalesDetail.qty * baseprice / (100 + SalesDetail.taxpercentage) * SalesDetail.taxpercentage).round(5);
                    $scope.SalesDetail.itemtax = (SalesDetail.tax / SalesDetail.qty).round(5);
                }
            }
        };

        $scope.findTax = function (amount, incost) {

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
            $scope.SalesDetail.additionaldiscountpercentage = 0;
            $scope.SalesDetail.discountpercentage = (results.discount / results.price * 100).round($scope.system_salesroundingdigit);
            $scope.SalesDetail.schemediscountpercentage = 0;
            if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                $scope.SalesDetail.additionaldiscount = 0;

            if (typeof ($scope.SalesDetail.schemediscount) == 'undefined' || isNaN($scope.SalesDetail.schemediscount))
                $scope.SalesDetail.schemediscount = 0;

            $scope.SalesDetail.locationid = $rootScope.LocationInfo.location.id;
            if (typeof (results.isuploadqty) != 'undefined' && results.isuploadqty != null && results.isuploadqty) {
                $scope.SalesDetail.qty = results.qty;
            } else if (typeof (results.isexchange) == 'undefined' || !results.isexchange) {
                $scope.SalesDetail.qty = $scope.SalesDetail.stock > 0 || !autofocus ? 1 : 0;

                if (results.stockisbit) {
                    $scope.SalesDetail.qty = results.retailqty;
                }

                if ($scope.IsSaleReturn) {
                    $scope.SalesDetail.qty = $scope.SalesDetail.qty * 1;
                }
            }

            $scope.SalesDetail.baseprice = ($scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount - ($scope.SalesDetail.schemediscount / $scope.SalesDetail.qty).round(2));
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

                if ($scope.SalesDetail.taxInfo != null) {
                    if ($scope.SalesDetail.taxInfo.taxsplit == null) {
                        $scope.SalesDetail.taxInfo.taxsplit = { 'taxpercentage': $scope.SalesDetail.taxInfo.taxpercentage };
                    }
                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                }
                results.taxname = $scope.SalesDetail.taxname;
                $scope.UpdateTax($scope.SalesDetail);
            }
            $scope.ProductDetails = [];

            if ($scope.Products && $scope.Products != null) {
                $.each($scope.Products, function (k, v) {
                    if (v.id == results.productid)
                        $scope.ProductDetails.push(v);
                });
            }

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
                $scope.CreateNew(false);
                $scope.lastBills = [];
                $rootScope.Toaster.Notice('Cancellation completed, Bill No - ' + result[0].billno);
            }).error(function (result) {
            });
        };

        $scope.PrintPDF = function (item) {
            $rootScope.Download("BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.id + ($scope.IsBulkSale ? "/true" : "/false"), "bill_" + item.billid + ".pdf", 'application/pdf');
            return;
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "bill_" + item.id + ".pdf";
                link.href = "BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.id + ($scope.IsBulkSale ? "/true" : "/false");
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.DownloadExcel = function (item) {

            $rootScope.Download("BillService/DownloadExcel/" + $scope.UserInfo.OrganizationID + '/' + item.id + ($scope.IsBulkSale ? "/true" : "/false"),
                "bill_" + item.id + ".xlsx", 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
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
                    'billtype': item.billtype,
                    'isMasterBill': typeof (item.isMasterBill) == 'undefined' ? false : item.isMasterBill,
                    'isbulkreturn': item.billtype == 10 || item.billtype == 20
                },
                "SecurityToken": "String content"
            }

            request.Item.printtemplateid = $scope.PrinterData.TemplateID;
            request.Item.papersize = $scope.PrinterData.PaperSize;
            request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
            request.Item.saledirectpdf = $scope.PrinterData.PaperSize == 9 || $scope.system_saledirectpdf;
            request.Item.directpdf = $scope.PrinterData.PaperSize == 9 || $scope.system_saledirectpdf;
            request.Item.frompos = true;
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {

                if (result != null && result.length == 1) {
                    $scope.PrintCopy.Send(result[0].billcopy);
                    if (typeof (result[0].directpdf) != 'undefined' && result[0].directpdf) {
                        $scope.Sent2PrinterPDF(result[0].printdocument);
                    }
                    else {
                        $scope.Sent2Printer(result[0].printdocument);
                    }
                }
                else
                    $rootScope.Toaster.Error(result.Message);
            });
        };

        $scope.Sent2PrinterPDF = function (printdocument) {
            if ($scope.PrinterData == null) {
                return;
            }
            $rootScope.Sent2Printer(printdocument, $scope.PrinterData, 'PDF', $scope.system_saleunicodepdf ? 'UNICODEBASE64' : 'BASE64');
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
            if ($scope.PrinterData == null) {
                return;
            }
            $rootScope.Sent2Printer(printdocument, $scope.PrinterData, 'RAW', $scope.system_saleunicodepdf ? 'UNICODEBASE64' : 'BASE64');
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
            $rootScope.Sent2Printer(printdocument);
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
                    'isreloadsalebill': $scope.SearchCondition.isreloadsalebill,
                    'locationid': $rootScope.LocationInfo.location.id,
                    'removecancelled': true,
                    'frompos': true
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

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
                    if (bill == 'undefined' || bill == null)
                        return;

                    if (bill.additionalcharges && bill.additionalcharges != 0) {
                        //bill.receivable -= bill.additionalcharges;
                        //bill.total -= bill.additionalcharges;
                        $scope.ShowAdditional = true;
                        $scope.AdditionalChargeGridConfig();
                        $scope.agrid.UpdateData(bill.additionalchargeslist);
                    }

                    if ($scope.IsSaleReturn && bill.billtype == 9)
                        return;
                    if ($scope.IsSaleReturn && !bill.settled) {
                        $rootScope.Toaster.Error("Unsettled bill can't be loaded in Sales Return.");
                        return;
                    }
                    if (!$scope.IsSaleReturn && bill.billtype == 8 && bill.settled) {
                        $rootScope.Toaster.Error("Settled return bill can't be loaded in Sales.");
                        return;
                    }

                    if (bill.billattributes != undefined && bill.billattributes != null) {
                        bill.billattributes.rewardamount = 0;
                        bill.billattributes.spentreward = 0;
                        bill.billattributes.newreward = 0;
                        bill.billattributes.rewardpoint = 0;
                        bill.billattributes.earnedreward = 0;
                        bill.billattributes.remainingreward = 0;
                        bill.billattributes.deliveryno = '';
                        bill.billattributes.manualrewardpoint = 0;
                        bill.billattributes.manualrewardamount = 0;
                        bill.billattributes.walkintoken = '';
                        bill.billattributes.gift = false;
                        bill.billattributes.giftcoupon = false;
                        bill.billattributes.online = false;
                        bill.billattributes.ecom = null;
                        bill.billattributes.couponid = 0;
                        bill.billattributes.coupons = null;
                        bill.billattributes.couponcode = '';
                        bill.billattributes.couponamount = 0;
                        bill.billattributes.discountamount = 0;
                    }

                    $.each(bill.Billitems, function (k, v) {
                        if (v.schemeid > 0) {
                            v.discount -= v.schemediscount;
                        }
                    });

                    $scope.HasPromotionApplied = false;
                    if (typeof (callback) != 'undefined')
                        callback(bill);

                    if (!bill.settled && bill.billtype == 8) {
                        if (bill.customer != 'undefined' && bill.customer != null) {
                            $scope.CurrentBill.phoneno = bill.customer.mobileno;
                            $scope.ApplyCustomerInfo(bill.customer);
                        }
                        var billitems = (JSON.parse(JSON.stringify(bill.Billitems)));
                        $.each(billitems, function (k, v) {
                            var existingitem = $scope.CurrentBill.Billitems.find(function (item) {
                                return typeof (item.returnbillitemid) != 'undefined' && item.returnbillitemid == v.id;
                            });
                            if (typeof (existingitem) == 'undefined' || existingitem == null) {
                                v.returnbillitemid = v.id;
                                v.id = 0;
                                if (v.attributes && v.attributes.serialno &&
                                    v.attributes.serialno != null && v.attributes.serialno != '') {
                                    v.displaycode = v.attributes.serialno;
                                }
                                else {
                                    v.displaycode = v.barcode;
                                }
                                $scope.CurrentBill.Billitems.push(v);
                                $scope.CalcualteTotal(v);
                            }
                        });
                        $scope.CurrentBill.returnbillid = bill.id;
                    }
                    else {

                        if (bill.billtype == 20) {
                            if (bill.customer != 'undefined' && bill.customer != null && bill.customerid != $scope.CurrentBill.customerid) {
                                $scope.CurrentBill.phoneno = bill.customer.mobileno;
                                $scope.ApplyCustomerInfo(bill.customer);
                            }
                            if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                $scope.CurrentBill.billattributes = {};
                            if (typeof ($scope.CurrentBill.billattributes.ebillno) == 'undefined')
                                $scope.CurrentBill.billattributes.ebillno = [];

                            if (bill.billattributes != 'undefined' && bill.billattributes != null) {
                                var existingitem = $scope.CurrentBill.billattributes.ebillno.find(function (item) {
                                    return item.billno == bill.billno;
                                });
                                if (typeof (existingitem) != 'undefined' || existingitem != null) {
                                    $rootScope.Toaster.Info("Given Estimate Bill - " + bill.billno + " already added for billing");
                                    return;
                                }
                                $scope.CurrentBill.billattributes.ebillno.push({ 'billno': bill.billno, 'countername': bill.billattributes.countername });
                            }

                            var billitems = (JSON.parse(JSON.stringify(bill.Billitems)));
                            $.each(billitems, function (k, v) {
                                v.id = 0;
                                if (v.attributes && v.attributes.serialno &&
                                    v.attributes.serialno != null && v.attributes.serialno != '') {
                                    v.displaycode = v.attributes.serialno;
                                }
                                else {
                                    v.displaycode = v.barcode;
                                }
                                $scope.CurrentBill.Billitems.push(v);
                                $scope.CalcualteTotal(v);
                            });
                            $scope.UpdateOffer();
                        }
                        else {
                            $scope.CurrentBill = bill;
                            if ((bill.billtype == 10 || bill.billtype == 20) && (!$scope.IsEstimate && !$scope.IsBulkSale))
                                bill.id = 0;

                            if (bill.customer != 'undefined' && bill.customer != null) {
                                $scope.CurrentBill.phoneno = bill.customer.mobileno;
                                $scope.ApplyCustomerInfo(bill.customer);
                            }
                            $scope.additionaldiscountamountrequested = bill.additionaldiscount;
                            $.each(bill.Billitems, function (k, v) {
                                if (v.attributes && v.attributes.serialno &&
                                    v.attributes.serialno != null && v.attributes.serialno != '') {
                                    v.displaycode = v.attributes.serialno;
                                }
                                else {
                                    v.displaycode = v.barcode;
                                }
                            });
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
                        }
                    }

                    if ($scope.IsSaleReturn) {
                        $scope.ReverseBillValue();
                        $scope.CurrentBill.returnbillno = $scope.CurrentBill.billno;
                        $scope.CurrentBill.returnbillid = $scope.CurrentBill.id;
                        $scope.CurrentBill.billno = '';
                        $scope.CurrentBill.id = 0;
                        $scope.autoCoding.code = '';
                        $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
                    }
                    $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                }
            }).error(function (result) {
            });
        };

        $scope.ReverseBillValue = function () {
            $scope.CurrentBill.totalqty = 0;
            $scope.CurrentBill.totalpiece = 0;
            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;
            $scope.CurrentBill.schemediscount = 0;
            if (!$scope.IsBulkSale)
                $scope.CurrentBill.billtype = 9;
            $scope.SchemeReturnBillItems = [];
            for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {

                //var addist = $scope.CurrentBill.Billitems[i].qty * ($scope.CurrentBill.Billitems[i].additionaldiscount + $scope.CurrentBill.Billitems[i].schemediscount);
                //alert(addist);

                if ($scope.CurrentBill.Billitems[i].schemediscount > 0) {
                    //$scope.CurrentBill.Billitems[i].schemediscountpercentage = (($scope.CurrentBill.Billitems[i].schemediscount / $scope.CurrentBill.Billitems[i].qty) / $scope.CurrentBill.Billitems[i].price * 100).round($scope.system_salesroundingdigit);
                    $scope.CurrentBill.Billitems[i].schemediscountpercentage = (($scope.CurrentBill.Billitems[i].schemediscount / $scope.CurrentBill.Billitems[i].qty) / ($scope.CurrentBill.Billitems[i].price + ($scope.CurrentBill.Billitems[i].discount / $scope.CurrentBill.Billitems[i].qty)) * 100).round($scope.system_salesroundingdigit);
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
                $scope.CurrentBill.Billitems[i].rtnsrcitemid = $scope.CurrentBill.Billitems[i].id;

                //var addist = $scope.CurrentBill.Billitems[i].qty * ($scope.CurrentBill.Billitems[i].additionaldiscount + $scope.CurrentBill.Billitems[i].schemediscount);
                //alert(addist);
                $scope.CalcualteTotal($scope.CurrentBill.Billitems[i]);

                if ($scope.CurrentBill.Billitems[i].schemeid > 0)
                    $scope.SchemeReturnBillItems.push($scope.CurrentBill.Billitems[i]);
            }
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
            request.Item.printtemplateid = $scope.PrinterData.TemplateID;
            request.Item.papersize = $scope.PrinterData.PaperSize;
            request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
            request.Item.saledirectpdf = $scope.PrinterData.PaperSize == 9 || $scope.system_saledirectpdf;
            request.Item.directpdf = $scope.PrinterData.PaperSize == 9 || $scope.system_saledirectpdf;
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

            if (typeof (result.Bills) != 'undefined') {
                $.each(result.Bills, function (k, v) {
                    v.printdocument = null;
                });
            }
            else {
                $.each(result, function (k, v) {
                    v.printdocument = null;
                });
            }

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
            if (typeof ($scope.agrid) != 'undefined' && $scope.agrid != null)
                $scope.agrid.UpdateData($scope.lastBills);
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

                var list = [];
                $.each($scope.CurrentBill.Billitems, function (k, v) {
                    list.push({ 'id': v.salablegoodsid, 'qty': v.qty });
                });

                var request =
                {
                    Item:
                    {
                        'customerid': $scope.CurrentBill.customerid,
                        'locationid': $rootScope.LocationInfo.location.id,
                        'barcodes': barcodelist,
                        'returngoods': $scope.IsSaleReturn,
                        'usedstock': list
                    }
                };

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
                                        if (!$scope.sale_negativestock && grpexist.barcode != results.serialno &&
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
                                    $rootScope.Toaster.Error("Missing Barcode found - " + results.barcode);
                                    continue;
                                }

                                if (!$scope.IsSaleReturn && results.qty <= 0) {
                                    $rootScope.Toaster.Error("In-sufficient barcode Qty - " + results.barcode);
                                    continue;
                                }
                            }

                            $scope.SalesDetail = (JSON.parse(JSON.stringify(baseSalesDetail)));
                            $scope.SalesDetail.barcode = results.barcode;
                            $scope.SalesDetail.qty = results.qty;
                            $scope.SalesDetail.stock = results.retailqty;
                            $scope.SalesDetail.hsncode = results.hsncode;
                            $scope.SalesDetail.attributes = {
                                'serialno': results.serialno,
                                'designid': results.designid,
                                'sizegroup': results.sizegroup,
                                'colourname': results.colourname,
                                'size': results.sizename,
                                'brand': results.brandname,
                                'typename': results.typename,
                                'materialname': results.materialname,
                                'stylename': results.stylename,
                                'patternname': results.patternname,
                                'mrp': results.mrp
                            };
                            results.priceextax = results.priceextax;
                            if ($scope.CurrentBill.pricesource == 1) {
                                results.price = results.dealerprice;
                                results.priceextax = results.dealerpriceextax;
                                results.discount = 0;
                                results.discountpercentage = 0;
                                results.additionaldiscount = 0;
                                results.additionaldiscountpercentage = 0;
                            }
                            if ($scope.CurrentBill.pricesource == 2) {
                                results.price = results.wsrate;
                                results.priceextax = results.wsrateextax;
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

                            results.isuploadqty = true;
                            $scope.AddBarcode(results, false);
                            var product = $scope.Products.find(function (item) {
                                return item.id == results.productid;
                            });
                            $scope.SalesDetail.sellingmode = product.sellingmode;
                            $scope.AddBillItem(false);
                        }
                        $scope.BeginGridTransaction = false;
                        $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                        $scope.setFocusautocoding();
                    }

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
                    $.each($scope.UploadedMissingBarcodeMissing, function (k, v) {
                        if (v.qty > 0)
                            $scope.UploadedMissingBarcodeMissing_Shortage += v.rqty - v.qty;
                        else
                            $scope.UploadedMissingBarcodeMissing_Missing += v.rqty;
                    });

                }).error(function (result) {
                    $scope.BeginGridTransaction = false;
                    $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                    $scope.setFocusautocoding();
                });
            }
        }
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
                    $scope.CreateNew(false);
                });
            });
        };


        $scope.HasProductSuggest = function (currentcode) {
            var products = $scope.BillKeys.find(function (item) {
                return item.type == 'PRODUCT' && item.name.startsWith(currentcode);
            });
            return products;
        }
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
                    //case 'PRODUCTS':
                    //    return true;
                    ////    $scope.CurrentBill.seasonid = $scope.suggest_selected.id;
                    ////    $scope.CurrentBill.season = $scope.suggest_selected.name;
                    ////    break;
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

        $scope.ResolveRack = function (rackcode, cardno, callback) {

            restService.Get('CustomerService/SearchByRackCard/' + $scope.qEncodeURIComponent(rackcode) +
                '/' + $scope.qEncodeURIComponent(cardno) +
                '/' + $rootScope.LocationInfo.location.id, true).then(function (results) {
                    if (results != 'undefined' && results != null) {
                        if ($scope.CurrentBill.billattributes == undefined ||
                            $scope.CurrentBill.billattributes == null) {
                            $scope.CurrentBill.billattributes = {};
                        }
                        $scope.CurrentBill.phoneno = results.mobileno;
                        $scope.ApplyCustomerInfo(results);
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
                        if (results && results != null && results.id > 0 &&
                            !$scope.IsEstimate && !$scope.hassale_deliverycardprefix &&
                            $scope.system_supportdeliveryrack) {
                            $scope.LoadEstimateBillFromDeliveryRack(results);
                        }
                        if (callback && callback != null)
                            callback(results);
                    }
                    else {
                        if (callback && callback != null)
                            callback({});
                        $rootScope.Toaster.Notice('Cant find the valid token');
                    }

                });
        }

        $scope.SearchAutoCoding = function ($event) {

            if ($scope.autoCoding.code == null || $scope.autoCoding.code == "") return;

            if ($scope.system_supportdeliveryrack &&
                $scope.autoCoding.code.length > 2 &&
                $scope.autoCoding.code.startsWith('#') &&
                $scope.autoCoding.code.endsWith('#')) {

                $scope.DeliveryRackInformation = '';

                $scope.ResolveRack($scope.autoCoding.code.substring(1, $scope.autoCoding.code.length - 1), '', function (result) {
                    $scope.CurrentBill.billattributes.deliveryrackcard = result.cardno;
                    $scope.CurrentBill.billattributes.deliveryrack = result.rackno;
                    $scope.CurrentBill.billattributes.deliveryrackid = result.rackid;

                    if (result.cardno && result.cardno != null && result.cardno != '')
                        $scope.DeliveryRackInformation = result.cardno;

                    if (result.rackno && result.rackno != null && result.rackno != '')
                        $scope.DeliveryRackInformation = $scope.DeliveryRackInformation == '' ? result.rackno : $scope.DeliveryRackInformation + ' - ' + result.rackno;

                    if (!$scope.IsEstimate)
                        $scope.LoadEstimateBillFromDeliveryRack(result);

                });
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.hassale_deliverycardprefix) {
                if ($scope.autoCoding.code.startsWith($scope.sale_deliverycardprefix)) {
                    var code = $scope.autoCoding.code;
                    $scope.autoCoding.code = '';
                    $scope.DeliveryRackInformation = '';
                    $scope.ResolveRack('', code, function (result) {

                        $scope.CurrentBill.billattributes.deliveryrackcard = result.cardno;
                        $scope.CurrentBill.billattributes.deliveryrack = result.rackno;
                        $scope.CurrentBill.billattributes.deliveryrackid = result.rackid;

                        if (result.cardno && result.cardno != null && result.cardno != '')
                            $scope.DeliveryRackInformation = result.cardno;

                        if (result.rackno && result.rackno != null && result.rackno != '')
                            $scope.DeliveryRackInformation = $scope.DeliveryRackInformation == '' ? result.rackno : $scope.DeliveryRackInformation + ' - ' + result.rackno;

                        if (!$scope.IsEstimate)
                            $scope.LoadEstimateBillFromDeliveryRack(result);

                    });
                    return;
                }
            }

            if ($scope.system_ishypermarketapplication && $scope.autoCoding.code.length > 2) {
                var product = $scope.HasProductSuggest($scope.autoCoding.code);
                if (typeof (product) != 'undefined' && product != null) {
                    $scope.autoCoding.code = product.code;
                    e.stopPropagation();
                    e.preventDefault();
                }
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

            if ($scope.autoCoding.code == "A" || $scope.autoCoding.code == "a") {
                $timeout($scope.GetApprovedBy(), 50);
                return;
            }

            if ($scope.UserInfo.p_SALES.print && ($scope.autoCoding.code == "P" || $scope.autoCoding.code == "p")) {
                if ($scope.CurrentBill.id > 0) {
                    $scope.Print($scope.CurrentBill);
                    $scope.PrintCopy.Send($scope.CurrentBill.billcopy);
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

            if ($scope.autoCoding.code == "#"
                && $scope.system_posmultiprice != 0) {
                if ($scope.CurrentBill.Billitems == undefined ||
                    $scope.CurrentBill.Billitems == null ||
                    $scope.CurrentBill.Billitems.length == 0) return;

                if ($scope.CurrentBill.Billitems[0].attributes &&
                    $scope.CurrentBill.Billitems[0].attributes != null &&
                    $scope.CurrentBill.Billitems[0].ismultiprice) {
                    $scope.autoCoding.code = '';
                    $scope.ShowMultiplePrice();
                }
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

                var product = $scope.Products.find(function (item) {
                    return item.id == $scope.CurrentBill.Billitems[0].productid;
                });

                if (typeof (product) != 'undefined' && product != null && product.discountmode == 4) {
                    $rootScope.Toaster.Notice("This product doesn't support additional discount");
                    return;
                }

                if (lastdiscountisvalue) {
                    var dpercentage = lastdiscount / $scope.CurrentBill.Billitems[0].receivable * 100;
                    if (dpercentage > parseFloat($rootScope.UserInfo.AD)) {
                        $rootScope.Toaster.Notice("Your allowed discount - " + dpercentage + '%');
                        $scope.autoCoding.code = '';
                        return;
                    }
                }
                else if (lastdiscount > parseFloat($rootScope.UserInfo.AD)) {
                    $rootScope.Toaster.Notice("Your allowed discount - " + lastdiscount + '%');
                    $scope.autoCoding.code = '';
                    return;
                }

                $scope.ApplySingleRowDiscount($scope.CurrentBill.Billitems[0], lastdiscountisvalue ? '' : '%', lastdiscount);
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code == "*" ||
                $scope.autoCoding.code == "N" || $scope.autoCoding.code == "n") {
                $scope.CreateNew(false);
                return;
            }

            if ($scope.system_poshold &&
                $scope.autoCoding.code.startsWith("/")) {
                if ($scope.autoCoding.code == '/') {
                    if ($scope.CurrentBill.total &&
                        $scope.CurrentBill.total > 0) {
                        $scope.HoldList.add($scope.CurrentBill)
                        $scope.CreateNew(true);
                    }
                }
                else {
                    var currentbill = $scope.HoldList.get($scope.autoCoding.code.substring(1));
                    if (currentbill && currentbill != null) {
                        $scope.autoCoding.code = '';
                        $scope.CurrentBill = currentbill.value;
                        if ($scope.igrid != null)
                            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                        $scope.ConfirmWidow.SetEnable($scope.CurrentBill.Billitems.length > 0);
                    }
                }
                return;
            }

            if (!$scope.system_saleautomobileno &&
                $scope.autoCoding.code.length == 10) {
                var letters = /^[0-9]+$/;
                if ($scope.autoCoding.code.match(letters)) {

                    if (!$scope.sale_allowchangeofcustomer && $scope.CurrentBill.customerid != 'undefined' && $scope.CurrentBill.customerid != null
                        && $scope.CurrentBill.customerid != 0 && $scope.CurrentBill.Billitems.length > 0) {
                        $rootScope.Toaster.Error("Customer Already selected, Please clear all the barcode if you want to change.");
                        return;
                    }

                    $scope.CurrentBill.phoneno = $scope.autoCoding.code;
                    $scope.autoCoding.code = '';
                    $scope.SearchCustomer();
                    return;
                }
            }


            if ($scope.hassale_customercardprefix) {
                var letter = $scope.sale_customercardprefix;
                if ($scope.autoCoding.code.startsWith(letter)) {
                    $scope.CurrentBill.loyeltyno = $scope.autoCoding.code;
                    $scope.autoCoding.code = '';
                    $scope.SearchCustomerByLoyaltyNo();
                    return;
                }
            }

            if ($scope.autoCoding.code == 'C' || $scope.autoCoding.code == 'c') {

                if (!$scope.sale_allowchangeofcustomer && $scope.CurrentBill.customerid != 'undefined' && $scope.CurrentBill.customerid != null
                    && $scope.CurrentBill.customerid != 0 && $scope.CurrentBill.Billitems.length > 0) {
                    $rootScope.Toaster.Error("Customer Already selected, Please clear all the barcode if you want to change.");
                    return;
                }

                $scope.ShowCustomerDialog();
                return;
            }

            if ($scope.autoCoding.code.startsWith('$') &&
                $scope.autoCoding.code.endsWith('$')) {
                if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                    $scope.CurrentBill.billattributes = {};
                $scope.CurrentBill.billattributes.walkintoken = $scope.autoCoding.code.substring(1, $scope.autoCoding.code.length - 1);
                $rootScope.Toaster.Info("Customer Entry Token added - " + $scope.CurrentBill.billattributes.walkintoken);
                $scope.CurrentBill.billattributes.remarks = $scope.CurrentBill.billattributes.walkintoken;
                $scope.autoCoding.code = '';
                $scope.SearchCustomerByWalkinToken();
                return;
            }

            if ($scope.autoCoding.code.startsWith('$') && !$scope.autoCoding.code.endsWith('$')) {
                $scope.SearchPackingSlip($scope.autoCoding.code.substring(1));
                return;
            }
            if (!$scope.autoCoding.code.startsWith('$') && $scope.autoCoding.code.endsWith('$')) {
                $scope.SearchPackBarcode($scope.autoCoding.code);
                return;
            }


            if ($scope.autoCoding.code == 'L' || $scope.autoCoding.code == 'l') {

                if (!$scope.gft_wocusredemption) {
                    if (typeof ($scope.CurrentBill.customerid) == 'undefined' || $scope.CurrentBill.customerid == 0) {
                        $rootScope.Toaster.Notice('Please select the customer');
                        return;
                    }
                }

                if ($scope.CurrentBill.disableloyalty != 'undefined' && $scope.CurrentBill.disableloyalty != null
                    && $scope.CurrentBill.disableloyalty == true) {
                    $rootScope.Toaster.Notice('Loyalty Points disabled.');
                    return;
                }

                if ($scope.CurrentBill.receivable == 0) {
                    $rootScope.Toaster.Notice('Please enter the barcode');
                    return;
                }

                $scope.ApplyRewardDialog();
                return;
            }

            $scope.autoCoding.autoenter = false;
            $scope.autoCoding.issingleprice = false;

            if ($scope.autoCoding.code.startsWith('?')) {
                $scope.ShowNonuanDialog($scope.autoCoding.code);
                return;
            }

            if ($scope.CuttingBarcodeParser &&
                $scope.CuttingBarcodeParser != null &&
                $scope.CuttingBarcodeParser.valid($scope.autoCoding.code)) {
                var parsebarcode = $scope.CuttingBarcodeParser.parse($scope.autoCoding.code);
                if (parsebarcode && parsebarcode.valid) {
                    $scope.autoCoding.code = parsebarcode.barcode;
                    $scope.autoCoding.qty = parsebarcode.qty;
                    $scope.autoCoding.ecode = parsebarcode.ecode;
                    $scope.autoCoding.autoenter = true;
                }
            }

            if ($scope.WeighingBarcodeParser &&
                $scope.WeighingBarcodeParser != null &&
                $scope.WeighingBarcodeParser.valid($scope.autoCoding.code)) {
                var parsebarcode = $scope.WeighingBarcodeParser.parse($scope.autoCoding.code);
                if (parsebarcode && parsebarcode.valid) {
                    $scope.autoCoding.code = parsebarcode.barcode;
                    $scope.autoCoding.qty = parsebarcode.qty;
                    $scope.autoCoding.issingleprice = true;
                    $scope.autoCoding.autoenter = true;
                }
            }


            if ($scope.system_multiplesalemen && $scope.autoCoding.code == '.') {
                $scope.SelectMultipleSalesmen({}, true);
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code.startsWith('.') && $scope.autoCoding.code.length > 0) {

                if($scope.system_multiplesalemen) {
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
                        $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
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
                            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
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
                            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                        }
                    }

                    $scope.LocalContext.Post('$scope.CurrentBill', $scope.CurrentBill, false);
                });
                return;
            }

            for (var index = 0; !$scope.system_supportmanualbill && index < $scope.CurrentBill.Billitems.length; index++) {

                if ($scope.CurrentBill.Billitems[index].additionaldiscountpercentage &&
                    $scope.CurrentBill.Billitems[index].additionaldiscountpercentage != null &&
                    $scope.CurrentBill.Billitems[index].additionaldiscountpercentage > 0) {
                    continue;
                }

                if ($scope.CurrentBill.Billitems[index].barcode == $scope.autoCoding.code &&
                    (typeof ($scope.CurrentBill.Billitems[index].salesmanid) == 'undefined'
                        || $scope.CurrentBill.Billitems[index].salesmanid == 0)) {
                    // || $scope.CurrentBill.Billitems[index].sellingmode == 1 -- Removed the condition 

                    if (($scope.CurrentBill.Billitems[index].qty < 0 && $scope.IsSaleReturn) ||
                        ($scope.CurrentBill.Billitems[index].qty > 0 && !$scope.IsSaleReturn)) {

                        if ($scope.CurrentBill.Billitems[index].sellingmode == 1) {

                            if (!$scope.system_disableuniquebarcode &&
                                typeof ($scope.CurrentBill.Billitems[index].stock) != 'undefined' &&
                                $scope.CurrentBill.Billitems[index].stock <= 1) {
                                if ($scope.system_restrictuniquebarcode) {
                                    $scope.autoCoding.code = "";
                                    $rootScope.Toaster.Notice('Already Added');
                                    return;
                                }
                                else {
                                    if ($scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[index]))
                                        $rootScope.Toaster.Notice('Deleted');

                                    $scope.autoCoding.code = "";
                                    $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                                    return;
                                }
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
                                $rootScope.Toaster.Notice('Piece Barcode Qty increased to ' + $scope.CurrentBill.Billitems[index].piece);

                                if ($scope.CurrentBill.Billitems[index].piece == 10 && $scope.system_multiplesaleqtypopup) {
                                    $timeout(function () {
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
                                                $scope.UpdateOffer();
                                                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                                            }
                                        }, $scope.CurrentBill.Billitems[index].piece);

                                    }, 25);

                                }
                                else {
                                    $scope.UpdateOffer();
                                }
                                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                                return;
                            }
                        }
                        if ($scope.CurrentBill.Billitems[index].sellingmode == 2) {

                            var newQty = $scope.IsSaleReturn ? -1 : 1;

                            $scope.CurrentBill.Billitems[index].qty += newQty;

                            if (newQty > 0 || $scope.IsSaleReturn) {
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
                            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                            $rootScope.Toaster.Notice('Qty increased to ' + $scope.CurrentBill.Billitems[index].qty);


                            if ($scope.CurrentBill.Billitems[index].piece == 3 && $scope.system_multiplesaleqtypopup) {

                                $timeout(function () {


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
                                            $scope.UpdateOffer();
                                            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                                        }
                                    }, $scope.CurrentBill.Billitems[index].piece);
                                }, 25);
                            }
                            else {
                                $scope.UpdateOffer();
                            }
                            return;
                        }
                    }
                }
                else if ($scope.CurrentBill.Billitems[index].barcode == $scope.autoCoding.code &&
                    (typeof ($scope.CurrentBill.Billitems[index].salesmanid) != 'undefined'
                        && $scope.CurrentBill.Billitems[index].salesmanid > 0)) {

                    if (($scope.CurrentBill.Billitems[index].qty < 0 && $scope.IsSaleReturn) ||
                        ($scope.CurrentBill.Billitems[index].qty > 0 && !$scope.IsSaleReturn)) {

                        if ($scope.CurrentBill.Billitems[index].sellingmode == 1) {
                            if (!$scope.system_disableuniquebarcode &&
                                typeof ($scope.CurrentBill.Billitems[index].stock) != 'undefined' &&
                                $scope.CurrentBill.Billitems[index].stock <= 1) {
                                if ($scope.system_restrictuniquebarcode) {
                                    $scope.autoCoding.code = "";
                                    $rootScope.Toaster.Notice('Already Added');
                                    return;
                                }

                            }
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
                if ($scope.BillNoHandling())
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

                    $scope.SalesDetail.barcode = results.barcode;
                    $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty + results.jqty));
                    $scope.SalesDetail.stockisbit = results.stockisbit;
                    $scope.SalesDetail.iqty = results.iqty;
                    $scope.SalesDetail.oqty = results.oqty;
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.SalesDetail.attributes = {
                        'serialno': results.serialno, 'designid': results.designid,
                        'size': results.sizename, 'brand': results.brandname, 'typename': results.typename,
                        'colourname': results.colourname,
                        'materialname': results.materialname,
                        'stylename': results.stylename,
                        'patternname': results.patternname,
                        'mrp': results.mrp,
                        'ismultiprice': results.ismultiprice
                    };
                    $scope.AddBarcode(results, false);

                    if ($scope.Products && $scope.Products != null) {
                        var product = $scope.Products.find(function (item) {
                            return item.id == results.productid;
                        });
                        $scope.SalesDetail.sellingmode = product.sellingmode;
                    }

                    if ($scope.system_supportmanualbill) {
                        $scope.ShowManualBillItem(results, product);
                    }
                    else {
                        if ($scope.SalesDetail.sellingmode == 3 && !results.stockisbit) {
                            $scope.setFocusautocodingqty();
                            if ($scope.autoCoding.autoenter) {
                                $scope.autoCoding.autoenter = false;
                                $scope.LastBiteEnter();
                                if ($scope.autoCoding.ecode &&
                                    $scope.autoCoding.ecode != null) {
                                    $scope.autoCoding.code = '.' + $scope.autoCoding.ecode;
                                    $scope.autoCoding.ecode = null;
                                    $scope.SearchAutoCoding();
                                }
                            }
                        }
                        else {
                            if ($scope.autoCoding.autoenter) {
                                $scope.autoCoding.autoenter = false;
                                $scope.LastBiteEnter();
                                if ($scope.autoCoding.ecode &&
                                    $scope.autoCoding.ecode != null) {
                                    $scope.autoCoding.code = '.' + $scope.autoCoding.ecode;
                                    $scope.autoCoding.ecode = null;
                                }
                                $scope.SearchAutoCoding();
                            }
                            else if ($scope.system_multipleinvoiceqty) {
                                $scope.setFocusmultipleinvoiceqty();
                            }
                            else if ($scope.system_supportmanualprice) {
                                if ($scope.IsSaleReturn)
                                    $scope.autoCoding.price = $scope.SalesDetail.price;
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
                }
                else if (!fromBillSearch) {

                    if (!$scope.system_saleautomobileno &&
                        $scope.autoCoding.code.length == 10) {
                        var letters = /^[0-9]+$/;
                        if ($scope.autoCoding.code.match(letters)) {

                            if (!$scope.sale_allowchangeofcustomer && $scope.CurrentBill.customerid != 'undefined' && $scope.CurrentBill.customerid != null
                                && $scope.CurrentBill.customerid != 0 && $scope.CurrentBill.Billitems.length > 0) {
                                $rootScope.Toaster.Error("Customer Already selected, Please clear all the barcode if you want to change.");
                                return;
                            }

                            $scope.CurrentBill.phoneno = $scope.autoCoding.code;
                            $scope.autoCoding.code = '';
                            $scope.SearchCustomer();
                        }
                    }
                    $scope.BillNoHandling();
                }
            });
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
                        'barcodes': barcodes
                    }
                };

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SalablegoodsService/SearchBarcode",
                    data: srequest
                }).success(function (barcoderesults) {
                    $scope.AddGroupItem(barcoderesults);
                    $scope.autoCoding.code = ''
                }).error(function (result) {
                });
            });
        }

        $scope.AddGroupItem = function (availableitems) {
            if (availableitems.length > 0) {
                var baseSalesDetail = (JSON.parse(JSON.stringify($scope.SalesDetail)));
                baseSalesDetail.isuploadqty = true;
                $scope.BeginGridTransaction = true;
                var barcodelistgroup = [];
                for (var i = 0; i < availableitems.length; i++) {
                    var results = availableitems[i];
                    $scope.SalesDetail = (JSON.parse(JSON.stringify(baseSalesDetail)));
                    $scope.SalesDetail.barcode = results.barcode;
                    $scope.SalesDetail.qty = results.qty;
                    $scope.SalesDetail.stock = results.retailqty;
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.SalesDetail.attributes = {
                        'serialno': results.serialno,
                        'designid': results.designid,
                        'sizegroup': results.sizegroup,
                        'colourname': results.colourname,
                        'size': results.sizename,
                        'brand': results.brandname,
                        'typename': results.typename,
                        'materialname': results.materialname,
                        'stylename': results.stylename,
                        'patternname': results.patternname,
                        'mrp': results.mrp
                    };
                    results.priceextax = results.priceextax;
                    if ($scope.CurrentBill.pricesource == 1) {
                        results.price = results.dealerprice;
                        results.priceextax = results.dealerpriceextax;
                        results.discount = 0;
                        results.discountpercentage = 0;
                        results.additionaldiscount = 0;
                        results.additionaldiscountpercentage = 0;
                    }
                    if ($scope.CurrentBill.pricesource == 2) {
                        results.price = results.wsrate;
                        results.priceextax = results.wsrateextax;
                        results.discount = 0;
                        results.discountpercentage = 0;
                        results.additionaldiscount = 0;
                        results.additionaldiscountpercentage = 0;
                    }
                    $scope.SalesDetail.pricewithouttax = results.priceextax;
                    $scope.SalesDetail.pricewithtax = results.price;
                    $scope.AddBarcode(results, false);
                    var product = $scope.Products.find(function (item) {
                        return item.id == results.productid;
                    });
                    $scope.SalesDetail.sellingmode = product.sellingmode;
                    $scope.AddBillItem(false);
                }
                $scope.BeginGridTransaction = false;
                $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                $scope.setFocusautocoding();
            }
        }

        $scope.SearchPackingSlip = function (barcode) {
            restService.Get('PackingslipService/SearchPackingNo/' + $rootScope.LocationInfo.location.id + '/' + $scope.qEncodeURIComponent(barcode)).then(function (results) {

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
                        'usedstock': $scope.CurrentBillConfig.stockqty()
                    }
                };

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SalablegoodsService/SearchBarcode",
                    data: srequest
                }).success(function (barcoderesults) {
                    $scope.AddGroupItem(barcoderesults);
                    $scope.autoCoding.code = ''
                }).error(function (result) {
                });

            });
        }

        $scope.BillNoHandling = function () {
            //($scope.billprefix != null && $scope.autoCoding.code.startsWith($scope.billprefix) && $scope.autoCoding.code.length > 8) || ($scope.autoCoding.code.startsWith('#'))
            if ($scope.BillNoConfig.isbillno($scope.autoCoding.code)) {
                if ($scope.IsEstimate) {
                    $rootScope.Toaster.Info("Please Use POS Sales screen for Reloading of Sale bill.")
                    $scope.SearchBarcodeDirect(true);
                    return true;
                }
                $scope.SearchCondition.billno = ($scope.autoCoding.code.startsWith('#') ? $scope.autoCoding.code.substring(1) : $scope.autoCoding.code);
                $scope.SearchCondition.billid = 0;
                $scope.SearchCondition.isbulkreturn = false;
                $scope.SearchCondition.isreloadsalebill = ($scope.IsSaleReturn && !$scope.system_disableloadsalebillacross ? false : true);
                $scope.SearchBill(function (bill) {

                    if (bill != null) {
                        $scope.autoCoding.code = "";
                        $scope.autoCoding.qty = "";
                        $scope.autoCoding.lastbit = "";
                        $scope.CurrentBill.isbulkreturn = false;
                        $scope.CurrentBill.returnbillno = bill.billno;
                        bill.returnbillno = bill.billno;
                        var oldbill = $scope.lastBills.find(function (item) {
                            return item.id == bill.id;
                        });

                        if (typeof ($scope.agrid) != 'undefined' && $scope.agrid != null &&
                            (typeof (oldbill) == 'undefined' || oldbill == null)) {
                            $scope.lastBills.push((JSON.parse(JSON.stringify(bill))));
                            $scope.agrid.UpdateData($scope.lastBills);
                        }
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

                        var oldbill = $scope.lastBills.find(function (item) {
                            return item.id == bill.id;
                        });

                        if (typeof (oldbill) == 'undefined' || oldbill == null) {
                            if (typeof ($scope.agrid) != 'undefined' && $scope.agrid != null) {
                                $scope.lastBills.push((JSON.parse(JSON.stringify(bill))));
                                $scope.agrid.UpdateData($scope.lastBills);
                            }
                        }
                        if (!$scope.IsEstimate) {
                            if ($scope.IsBulkSale && !$scope.IsEstimate) {
                                $rootScope.Confirm('Return goods', 'Do you want to return the previous approval goods without billing?',
                                    function () {
                                        if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                            $scope.CurrentBill.billattributes = {};
                                        $scope.CurrentBill.isbulkreturngoods = true;
                                    });
                            }
                            else if (bill.billtype == 10) {
                                $rootScope.Confirm('Return goods', 'Did you received products except billing products?',
                                    function () {
                                        if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                            $scope.CurrentBill.billattributes = {};
                                        $scope.CurrentBill.isbulkreturngoods = true;
                                    });
                            }
                            else {
                                $scope.CurrentBill.isbulkreturngoods = false;
                                $scope.CurrentBill.isbulkreturn = false;
                                bill.isbulkreturn = false;
                            }
                        }
                        else if ($scope.system_supportdeliveryrack) {

                            if ($scope.CurrentBill.billattributes == undefined ||
                                $scope.CurrentBill.billattributes == null)
                                $scope.CurrentBill.billattributes = {};

                            $scope.DeliveryRackInformation = '';

                            if (bill.billtype == 20 && !$scope.IsSaleReturn) {
                                $scope.CurrentBill.billattributes.deliveryrack = bill.billattributes.deliveryrack;
                                $scope.CurrentBill.billattributes.deliveryrackcard = bill.billattributes.deliveryrackcard;
                                $scope.CurrentBill.customerid = bill.customerid;
                                $scope.CurrentBill.phoneno = bill.phoneno;
                                $scope.CurrentBill.customername = bill.customername;

                                if (bill.billattributes.deliveryrackcard && bill.billattributes.deliveryrackcard != null && bill.billattributes.deliveryrackcard != '')
                                    $scope.DeliveryRackInformation = bill.billattributes.deliveryrackcard;

                                if (bill.billattributes.deliveryrack && bill.billattributes.deliveryrack != null && bill.billattributes.deliveryrack != '')
                                    $scope.DeliveryRackInformation = $scope.DeliveryRackInformation == '' ? bill.billattributes.deliveryrack : $scope.DeliveryRackInformation + ' - ' + bill.billattributes.deliveryrack;
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
            else if ($scope.BillNoConfig.isbillmaster($scope.autoCoding.code)) {
                request = {
                    "Item": {
                        'isMasterBill': true,
                        'isreloadsalebill': true,
                        'frompos': true,
                        'locationid': $rootScope.LocationInfo.location.id,
                        'billno': $scope.autoCoding.code
                    },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BillService/Search",
                    data: srequest
                }).success(function (result) {

                    $scope.CreateNew(false);
                    $scope.lastBills = result;
                    $.each(result, function (billkey, bill) {
                        bill.isMasterBill = true;
                        var billitems = (JSON.parse(JSON.stringify(bill.Billitems)));
                        $.each(billitems, function (k, v) {
                            var existingitem = $scope.CurrentBill.Billitems.find(function (item) {
                                return typeof (item.returnbillitemid) != 'undefined' && item.returnbillitemid == v.id;
                            });
                            if (typeof (existingitem) == 'undefined' || existingitem == null) {
                                v.returnbillitemid = v.id;
                                v.id = 0;
                                if (v.attributes && v.attributes.serialno &&
                                    v.attributes.serialno != null && v.attributes.serialno != '') {
                                    v.displaycode = v.attributes.serialno;
                                }
                                else {
                                    v.displaycode = v.barcode;
                                }
                                $scope.CurrentBill.Billitems.push(v);
                                $scope.CalcualteTotal(v);
                            }
                        });
                        $scope.CurrentBill.returnbillid = bill.id;
                    })
                    if (result[0] != 'undefined' && result[0] != null &&
                        result[0].customer != 'undefined' && result[0].customer != null) {
                        $scope.CurrentBill.phoneno = result[0].customer.mobileno;
                        $scope.ApplyCustomerInfo(result[0].customer);
                    }
                    if (typeof ($scope.agrid) != 'undefined' && $scope.agrid != null)
                        $scope.agrid.UpdateData($scope.lastBills);
                });

                return true;
            }

            return false;
        }

        $scope.CreateNew(true);

        $scope.RequestCustomerOTP = function () {
            var request = {
                "Item": {
                    'id': $scope.CurrentBill.customerid,
                    'mobileno': $scope.CurrentBill.phoneno
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
                $scope.CurrentBill.discountotpconfirmed = false;
                return;
            }

            var request = {
                "Item": {
                    'id': $scope.CurrentBill.customerid,
                    'mobileno': $scope.CurrentBill.phoneno,
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
                    $scope.CurrentBill.discountotpconfirmed = true;
                    $timeout(function () {
                        $scope.setFocusautocoding();
                    }, 50)
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


        $scope.CheckCreditFocus = function () {
            if (!$scope.CurrentBill.iscreditbill &&
                $scope.CurrentBill.customername != null &&
                $scope.CurrentBill.customername.length > 0) {
                $scope.setFocusautocoding();
            }
            else {
                $scope.setFocuscustomername();
            }
        }

        $scope.LastBiteEnter = function () {
            var islastbit = false;
            var ismultiqty = false;
            var multiqty = 0;
            var retailqty = 0;
            var enteredqty = 0;
            var lastbit = 0.0;

            if ($scope.SalesDetail.barcodeDetail != 'undefined' && $scope.SalesDetail.barcodeDetail != null &&
                $scope.GetNumber($scope.SalesDetail.barcodeDetail.retailqty) > 0)
                retailqty = $scope.GetNumber($scope.SalesDetail.barcodeDetail.retailqty);

            $scope.autoCoding.lastbit = '';
            if ($scope.autoCoding.qty.toString().indexOf('-') > 0) {
                var bitinfo = $scope.autoCoding.qty.split('-');
                var qty = $scope.GetNumber(bitinfo[0]);
                lastbit = $scope.GetNumber(bitinfo[1]);
                if (qty < lastbit) {
                    $rootScope.Toaster.Error('last bit discount cant exceed actual');
                    return;
                }
                var journalqty = qty - lastbit;
                $scope.autoCoding.lastbit = journalqty.toString();
                $scope.autoCoding.qty = qty.toString();
                islastbit = true;
                enteredqty = qty;
            } else if ($scope.autoCoding.qty.toString().indexOf('*') > 0) {
                ismultiqty = true;
                var bitinfo = $scope.autoCoding.qty.split('*');
                var qty = $scope.GetNumber(bitinfo[0]);
                multiqty = $scope.GetNumber(bitinfo[1]);
                $scope.autoCoding.qty = qty.toString();
            }

            var bqty = Math.abs($scope.GetNumber($scope.autoCoding.qty));
            $scope.SalesDetail.qty = bqty;

            if (islastbit && $scope.SalesDetail.attributes != 'undefined' && $scope.SalesDetail.attributes != null) {
                $scope.SalesDetail.attributes.islastbit = true;
                $scope.SalesDetail.attributes.lastbitqty = lastbit;

                if (lastbit > 0.5) {
                    $rootScope.Toaster.Error("Last Bit Quantity can't be morethan 0.5 Mtrs.");
                    return;
                }
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
            $scope.autoCoding.lastbit = "";
            if (islastbit) {
                if (retailqty != enteredqty)
                    $rootScope.Toaster.Info('last bit added with Insufficient qty');
                else
                    $rootScope.Toaster.Notice('last bit added');
            }
            $scope.setFocusautocoding();
        };

        hotkeys.add('alt+p', 'Print', function (event) {
            event.preventDefault();
            if ($scope.UserInfo.p_SALES.print) {
                $scope.Print($scope.CurrentBill);
                $scope.PrintCopy.Send($scope.CurrentBill.billcopy);
            }
        });

        $scope.ApplyRewardDialog = function () {
            var dialog = ngDialog.open({
                template: 'ApplyReward',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', function ($scope, $timeout, $http, $rootScope) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.system_duplicatecoupon = $scope.Parent.system_duplicatecoupon;
                    $scope.settlement_reward_slabed = $scope.Parent.settlement_reward_slabed;
                    $scope.reward_point = 0;
                    $scope.reward_convertable = 0;
                    $scope.reward_amount = 0;
                    $scope.coupon_amount = 0;
                    $scope.couponcode = '';
                    $scope.coupon_convertable = 0;
                    $scope.coupon_value = 0;
                    $scope.couponid = 0;
                    $scope.coupon_maxamount = 0;
                    $scope.coupons = [];
                    $scope.oldcouponsamount = 0;
                    if (typeof ($scope.Parent.CurrentBill.billattributes) != 'undefined' &&
                        typeof ($scope.Parent.CurrentBill.billattributes.couponamount) != 'undefined') {
                        $scope.coupon_amount = $scope.Parent.CurrentBill.billattributes.couponamount;
                        $scope.couponcode = $scope.Parent.CurrentBill.billattributes.couponcode;
                        $scope.coupon_convertable = $scope.Parent.CurrentBill.billattributes.couponamount;
                        $scope.coupon_value = $scope.Parent.CurrentBill.billattributes.couponamount;
                        $scope.coupon_maxamount = $scope.Parent.CurrentBill.billattributes.couponamount;
                        $scope.coupons = $scope.Parent.CurrentBill.billattributes.coupons;
                        $scope.oldcouponsamount = $scope.coupon_amount;
                    }
                    $scope.reason = $scope.Parent.ApplicationSettings.appsettings.sale_customerOTP ? 'Require OTP to Claim Rewards.' : '';
                    $scope.reward_maxamount = 0;
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
                        //$scope.closeThisDialog({ 'type': 'clear', 'amount': 0 });
                        $scope.closeThisDialog();
                    }

                    $scope.ApplyReward = function ($event) {


                        if ($rootScope.GetNumber($scope.reward_amount) < 0 ||
                            ($rootScope.GetNumber($scope.reward_amount) > $rootScope.GetNumber($scope.reward_maxamount)
                                && $rootScope.GetNumber($scope.reward_maxamount) > 0)) {
                            $rootScope.Toaster.Notice('Amount cant exceed ' + $scope.reward_maxamount);
                            return;
                        }

                        var rewardpoints = $scope.reward_point;
                        if (!$scope.Parent.sale_canclaimsamebillrewards)
                            rewardpoints = ($scope.reward_point - $scope.reward_new);

                        var requiredpoints = (rewardpoints / $scope.reward_convertable) * $scope.reward_amount;

                        if ($scope.Parent.ApplicationSettings.appsettings.sale_customerOTP) {
                            var request = {
                                "Item": {
                                    'id': $scope.Parent.CurrentBill.customerid,
                                    'mobileno': $scope.Parent.CurrentBill.phoneno,
                                    'name': $scope.Parent.CurrentBill.customername,
                                    'claimamount': $rootScope.GetNumber($scope.reward_amount),
                                    'claimpoint': requiredpoints//$rootScope.GetNumber($scope.reward_point) - $rootScope.GetNumber($scope.reward_new)
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
                            if (requiredpoints < rewardpoints)
                                $scope.reward_new = 0;
                            else
                                $scope.reward_new = requiredpoints - ($scope.reward_point - $scope.reward_new);

                            $scope.closeThisDialog({
                                'type': 'reward', 'amount': $scope.reward_amount, 'newreward': $scope.reward_new,
                                'rewards': requiredpoints
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

                            if (result.Success && result.Item) {

                                //var requiredpoints = ($scope.reward_point / $scope.reward_convertable) * $scope.reward_amount;
                                var rewardpoints = $scope.reward_point;
                                if (!$scope.Parent.sale_canclaimsamebillrewards)
                                    rewardpoints = ($scope.reward_point - $scope.reward_new);
                                var requiredpoints = (rewardpoints / $scope.reward_convertable) * $scope.reward_amount;
                                if (requiredpoints < rewardpoints)
                                    $scope.reward_new = 0;
                                else
                                    $scope.reward_new = requiredpoints - ($scope.reward_point - $scope.reward_new);

                                $scope.closeThisDialog({ 'type': 'reward', 'amount': $scope.reward_amount, 'newreward': $scope.reward_new, 'rewards': requiredpoints });
                            }
                            else
                                $rootScope.Toaster.Notice(result.ErrorMessage == null ? 'OTP Mismatch' : result.ErrorMessage);
                        });
                    }

                    $scope.ApplyCoupon = function () {
                        if ($scope.coupon_amount == undefined || $scope.coupon_amount == null || isNaN($scope.coupon_amount))
                            $scope.coupon_amount = 0;

                        if ($scope.coupon_amount < 0 || ($scope.coupon_amount > $scope.coupon_maxamount && $scope.coupon_maxamount > 0)) {
                            $rootScope.Toaster.Notice('Amount cant exceed ' + $scope.coupon_maxamount);
                            return;
                        }
                        if ($rootScope.GetNumber($scope.coupon_value) > 0
                            && $rootScope.GetNumber($scope.coupon_value) > $rootScope.GetNumber($scope.coupon_amount))
                            $rootScope.Toaster.Error('Coupon value is morethan the redeem Value. Balance Amount Can not be redeem again.');

                        $scope.closeThisDialog({
                            'type': 'coupon',
                            'amount': $scope.coupon_amount,
                            'coupon_code': $scope.couponcode,
                            'coupon_id': $scope.couponid,
                            'coupons': $scope.coupons,
                            'barcodes': $scope.coupon_barcodes
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
                        if ($scope.Parent.CurrentBill == undefined ||
                            $scope.Parent.CurrentBill == null ||
                            $scope.Parent.CurrentBill.customerid == undefined ||
                            $scope.Parent.CurrentBill.customerid == null ||
                            $scope.Parent.CurrentBill.customerid == '' ||
                            $scope.Parent.CurrentBill.customerid == 0)
                            return;

                        var _billvalue = $scope.Parent.CurrentBill.receivable;
                        if ($scope.Parent.system_excluderewards) {
                            _billvalue = 0;
                            $.each($scope.Parent.CurrentBill.Billitems, function (k, v) {
                                var product = $scope.Parent.Products.find(function (item) {
                                    return item.id == v.productid;
                                });

                                if (typeof (product) != 'undefined' && product != null
                                    && product.attributes != 'undefined' && product.attributes != null
                                    && product.attributes.excludereward == true) {
                                    //no rewards for this product.
                                }
                                else {
                                    _billvalue += v.receivable;
                                }
                            });
                        }
                        //$scope.receivable = $scope.Parent.CurrentBill.receivable;
                        $scope.receivable = _billvalue;
                        var srequest = JSON.stringify({
                            "Item": {
                                'customerid': $scope.Parent.CurrentBill.customerid,
                                'locationid': $rootScope.LocationInfo.location.id,
                                'billvalue': _billvalue,
                                'bill': $scope.Parent.CurrentBill
                            },
                            "securitytoken": "String content"
                        });

                        $http({
                            method: 'POST',
                            url: "CustomerrewardsService/SearchReward",
                            data: srequest
                        }).success(function (result) {
                            if (result != null && typeof (result) != 'undefined') {
                                $scope.reward_point = $rootScope.GetNumber(result.rewards);
                                $scope.reward_convertable = $rootScope.GetNumber(result.available);
                                $scope.reward_amount = $rootScope.GetNumber(result.convertable);
                                $scope.reward_maxamount = $rootScope.GetNumber(result.convertable);
                                $scope.manual_point = $rootScope.GetNumber(result.rewards);
                                $scope.reward_new = $rootScope.GetNumber(result.billrewards);
                                //if (!$scope.Parent.sale_canclaimsamebillrewards) {
                                //    $scope.reward_convertable = $scope.reward_convertable - ($scope.reward_new / $scope.Parent.settlement_reward_value);
                                //    $scope.reward_maxamount = $scope.reward_maxamount - ($scope.reward_new / $scope.Parent.settlement_reward_value);
                                //    $scope.reward_amount = $scope.reward_amount - ($scope.reward_new / $scope.Parent.settlement_reward_value);
                                //}
                                if (isNaN($scope.reward_convertable))
                                    $scope.reward_convertable = 0;
                                if (isNaN($scope.reward_maxamount))
                                    $scope.reward_maxamount = 0;
                                if (isNaN($scope.reward_amount))
                                    $scope.reward_amount = 0;

                                if ($scope.showmanual)
                                    $scope.setFocusmanual_point();
                            }
                        }).error(function (result) {

                        });
                    }();


                    $scope.DeleteCoupon = function (result, coupons) {
                        $scope.coupon_value -= result.available;
                        $scope.coupon_convertable -= result.convertable;
                        $scope.coupon_amount -= result.convertable;
                        $scope.coupon_maxamount -= result.available;
                        coupons.splice(coupons.indexOf(result), 1);
                    }

                    $scope.ApplyCouponOTP = function (result) {
                        $rootScope.Prompt('OTP Approval', result.reason, function (value) {

                            var request = {
                                "Item": {
                                    'id': result.otpcustomerid,
                                    'otp': value
                                },
                                "SecurityToken": "String content"
                            };
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "CustomerService/ConfirmOTP",
                                data: srequest
                            }).success(function (otpresult) {
                                if (otpresult.Success && otpresult.Item) {
                                    if ($scope.system_duplicatecoupon) {
                                        $scope.coupon_value += result.available;
                                        $scope.coupon_convertable += result.convertable;
                                        $scope.coupon_amount += result.convertable;
                                        $scope.coupon_maxamount += result.available;
                                        $scope.coupon_lastid = result.couponid;
                                        if ($scope.coupon_barcodes == undefined || $scope.coupon_barcodes == null)
                                            $scope.coupon_barcodes = [];

                                        if (result.eligiblebarcodes && result.eligiblebarcodes != null) {
                                            $.each(result.eligiblebarcodes, function (k, v) {
                                                $scope.coupon_barcodes.push(v);
                                            });
                                        }

                                        $scope.coupons.push({
                                            'id': result.couponid,
                                            'code': $scope.couponcode,
                                            'barcodes': result.eligiblebarcodes,
                                            'amount': result.convertable,
                                            'available': result.available,
                                            'applied': false
                                        });
                                        $scope.couponcode = '';
                                    }
                                    else {
                                        $scope.coupon_value = result.available;
                                        $scope.coupon_convertable = result.convertable;
                                        $scope.coupon_amount = result.convertable;
                                        $scope.coupon_maxamount = result.convertable;
                                        $scope.coupon_barcodes = result.eligiblebarcodes;
                                        $scope.reason = result.reason;
                                        $scope.couponid = result.couponid;
                                        $scope.FocusAutoBarcode('applycoupon');
                                    }
                                }
                                else {
                                    $rootScope.Toaster.Notice(otpresult.ErrorMessage == null ? 'OTP Mismatch' : otpresult.ErrorMessage);
                                    $scope.ApplyCouponOTP(result)
                                }
                            });
                        });
                    }

                    $scope.FocusAutoBarcode = function (element) {
                        if (!document.activeElement ||
                            document.activeElement == null ||
                            document.activeElement.name != element) {
                            $scope['setFocus' + element]();
                            $timeout(function () {
                                $scope.FocusAutoBarcode(element);
                            }, 100);
                        }
                    }

                    $scope.FindCoupon = function () {

                        var _billvalue = $scope.Parent.CurrentBill.receivable;

                        $.each($scope.Parent.CurrentBill.Billitems, function (k, v) {
                            if (typeof (v.attributes.coupondiscount) == 'undefined') {
                                v.attributes.coupondiscount = 0;
                                v.attributes.coupondiscountpercentage = 0;
                            }

                            if (v.attributes.coupondiscount > 0 && v.additionaldiscount >= v.attributes.coupondiscount) {
                                _billvalue += (v.attributes.coupondiscount * v.qty);
                            }

                            // Coupon discount not applied for the product marked No Addln Discount
                            //var product = $scope.Parent.Products.find(function (item) {
                            //    return item.id == v.productid;
                            //});
                            //if (typeof (product) != 'undefined' && product != null && product.discountmode == 4) {
                            //    _billvalue -= v.receivable;
                            //}

                            // Coupon discount not applicable if Disable Coupon discount on Addlin discount is enabled.
                            if ($scope.Parent.system_disablecoupondisonaddlndisitems) {
                                if (v.schemeid > 0 || (v.additionaldiscount > 0 && v.attributes.coupondiscount == 0)) {
                                    _billvalue -= v.receivable;
                                }
                            }
                        });


                        if ($scope.system_duplicatecoupon && $scope.coupons &&
                            $scope.coupons != null && $scope.coupons.length > 0) {
                            var exist = $scope.coupons.find(function (searchitem) {
                                return searchitem.code == $scope.couponcode;
                            });

                            if (exist && exist != null &&
                                exist.id > 0) {
                                $rootScope.Toaster.Notice('Already added ' + $scope.couponcode + ' Amount ' + exist.amount);
                                return;
                            }
                        }

                        $scope.Parent.CurrentBill.locationid = $rootScope.LocationInfo.location.id;
                        var srequest = JSON.stringify({
                            "Item": {
                                'acceptablelocationid': $rootScope.LocationInfo.location.id,
                                'code': $scope.couponcode,
                                'billvalue': _billvalue,
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
                                if (result.couponid > 0 && result.otpcustomerid && result.otpcustomerid > 0) {
                                    $scope.ApplyCouponOTP(result);
                                }
                                else {
                                    $scope.reason = result.reason;
                                    if (result.couponid > 0) {
                                        if ($scope.system_duplicatecoupon) {
                                            $scope.coupon_value += result.available;
                                            $scope.coupon_convertable += result.convertable;
                                            $scope.coupon_amount += result.convertable;
                                            $scope.coupon_maxamount += result.available;
                                            $scope.coupon_lastid = result.couponid;
                                            if ($scope.coupon_barcodes == undefined || $scope.coupon_barcodes == null)
                                                $scope.coupon_barcodes = [];

                                            if (result.eligiblebarcodes && result.eligiblebarcodes != null) {
                                                $.each(result.eligiblebarcodes, function (k, v) {
                                                    $scope.coupon_barcodes.push(v);
                                                });
                                            }

                                            $scope.coupons.push({
                                                'id': result.couponid,
                                                'code': $scope.couponcode,
                                                'barcodes': result.eligiblebarcodes,
                                                'amount': result.convertable,
                                                'available': result.available,
                                                'applied': false
                                            });
                                            $scope.couponcode = '';
                                        }
                                        else {
                                            $scope.coupon_value = result.available;
                                            $scope.coupon_convertable = result.convertable;
                                            $scope.coupon_amount = result.convertable;
                                            $scope.coupon_maxamount = result.convertable;
                                            $scope.coupon_barcodes = result.eligiblebarcodes;
                                            $scope.reason = result.reason;
                                            $scope.couponid = result.couponid;
                                            $scope.FocusAutoBarcode('applycoupon');
                                        }
                                    }
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
                        var applyfor = data.value.type == 'coupon' ? 'COUPON' : 'REWARD';
                        var barcodes = null;

                        if (data.value.type == 'coupon') {
                            barcodes = data.value.barcodes;
                            //    if (data.value.coupons == undefined || data.value.coupons == null)
                            //        barcodes = data.value.barcodes;
                            //    else {
                            //        var coupon = data.value.coupons.find(function (item) {
                            //            return item.id == data.value.coupon_id;
                            //        })
                            //        if (coupon && coupon != null) {
                            //            barcodes = coupon.barcodes;
                            //        }
                            //    }
                        }
                        $scope.ApplyAdditionalDiscount(parseFloat(data.value.amount), applyfor, barcodes);
                        $scope.CurrentBill.HasCouponApplied = false;

                        if ($scope.CurrentBill.rewardapplicable != 'undefined'
                            && $scope.CurrentBill.rewardapplicable != null
                            && $scope.CurrentBill.rewardapplicable == false)
                            data.value.amount = 0;

                        if (data.value.amount == 0) {
                            if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                                $scope.CurrentBill.billattributes = {};
                            $scope.CurrentBill.billattributes.mode = '';
                            $scope.CurrentBill.billattributes.discountamount = 0;
                            $scope.CurrentBill.billattributes.rewardpoint = 0;
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
                            $scope.CurrentBill.billattributes.rewardpoint = 0;
                            $scope.CurrentBill.billattributes.newreward = 0;
                            $scope.CurrentBill.billattributes.couponamount = 0;
                            $scope.CurrentBill.billattributes.couponid = 0;
                            $scope.CurrentBill.billattributes.coupons = [];
                            $scope.CurrentBill.billattributes.mode = data.value.type;
                            $scope.CurrentBill.billattributes.discountamount = data.value.amount;
                            if (data.value.type == 'reward') {
                                $scope.CurrentBill.billattributes.rewardamount = data.value.amount;
                                $scope.CurrentBill.billattributes.rewardpoint = data.value.rewards;
                                $scope.CurrentBill.billattributes.newreward = data.value.newreward;
                                if (!$scope.sale_canclaimsamebillrewards) {
                                    $scope.CurrentBill.billattributes.rewardpoint = (data.value.rewards - data.value.newreward);
                                }
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
                    $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                    $scope.autoCoding.code = '';

                    $scope.setFocusautocoding();
                    $timeout(function () {
                        $scope.FocusAutoBarcode();
                    }, 500);
                }
            });
        };



        $scope.FocusAutoBarcode = function () {
            if (!document.activeElement ||
                document.activeElement == null ||
                document.activeElement.name != 'barcodeautocoding') {
                $scope.setFocusautocoding();
                $timeout(function () {
                    $scope.FocusAutoBarcode();
                }, 500);
            }
        }

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
                        'discount': (v.discount / v.qty),
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
            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
        }

        $scope.GetApprovedBy = function () {
            $rootScope.Prompt('Approved By', 'Code', function (code) {
                code = code.toString();
                if (typeof (code) != 'undefined' && code.length > 0) {
                    restService.Get('EmployeeService/EmployeeCode/' + $scope.UserInfo.OrganizationID + '/' + $rootScope.LocationInfo.location.id + '/' + code.replaceAll('.', ''), true).then(function (results) {
                        $scope.CurrentBill.approvedby = results.name;
                        if (typeof ($scope.CurrentBill.billattributes) == 'undefined' || $scope.CurrentBill.billattributes == null)
                            $scope.CurrentBill.billattributes = {};

                        $scope.CurrentBill.billattributes.approvedby = results.id;
                    });
                }
            })
        }
        $scope.ApplyAdditionalDiscount = function (newvalue, applymode, barcodes) {

            $scope.PopupList = [];
            $.each($scope.CurrentBill.Billitems, function (k, v) {

                if (typeof (v.attributes) == 'undefined' || v.attributes == null)
                    v.attributes = {
                        'coupondiscount': 0,
                        'coupondiscountpercentage': 0,
                        'rewarddiscount': 0,
                        'rewarddiscountpercentage': 0,
                    };

                if (typeof (v.attributes.rewarddiscount) == 'undefined') {
                    v.attributes.rewarddiscount = 0;
                    v.attributes.rewarddiscountpercentage = 0;
                }

                if (typeof (v.attributes.coupondiscount) == 'undefined') {
                    v.attributes.coupondiscount = 0;
                    v.attributes.coupondiscountpercentage = 0;
                }

                if (v.attributes.coupondiscount > 0 && v.additionaldiscount >= v.attributes.coupondiscount) {
                    v.additionaldiscount -= v.attributes.coupondiscount;
                    v.additionaldiscountpercentage -= v.attributes.coupondiscountpercentage;
                    v.receivable += (v.attributes.coupondiscount * v.qty);
                }

                var product = $scope.Products.find(function (item) {
                    return item.id == v.productid;
                });

                if (typeof (v.schemeid) == 'undefined' || v.schemeid == null)
                    v.schemeid = 0;
                //if (typeof (product) != 'undefined' && product != null && product.discountmode == 4) {
                //    //no bill level discounts.
                //}
                //else {
                // Coupon discount not applicable if Disable Coupon discount on Addlin discount is enabled.
                if (!$scope.system_disablecoupondisonaddlndisitems || (v.additionaldiscount == 0 && v.schemeid == 0)) {
                    if (!$scope.IsSaleReturn && v.qty > 0) {
                        $scope.PopupList.push({
                            'core': v,
                            'additionaldiscount': v.additionaldiscount,
                            'additionaldiscountpercentage': v.additionaldiscountpercentage,
                            'receivable': v.receivable
                        });
                    }
                }
                else if (applymode == "REWARD") {
                    if (!$scope.IsSaleReturn && v.qty > 0) {
                        $scope.PopupList.push({
                            'core': v,
                            'additionaldiscount': v.additionaldiscount,
                            'additionaldiscountpercentage': v.additionaldiscountpercentage,
                            'receivable': v.receivable
                        });
                    }
                }
                //}
            });

            var avail = newvalue;
            $scope.TotalValue = 0;

            //filtered barcode for coupons
            if (barcodes != 'undefined' && barcodes != null && barcodes.length > 0) {
                var filterlist = [];
                $.each($scope.PopupList, function (k, v) {
                    if (barcodes.indexOf(v.core.barcode) >= 0) {
                        filterlist.push(v);
                    }
                })
                $scope.PopupList = filterlist;
            }

            $.each($scope.PopupList, function (k, v) {
                if ($rootScope.GetNumber(v.core.receivable) > 0)
                    $scope.TotalValue += v.core.receivable;
            });

            if (Math.abs($scope.TotalValue) == 0) {
                avail = 0;
                $scope.CurrentBill.rewardapplicable = false;
            }

            var percent = (avail / Math.abs($scope.TotalValue) * 100).round(2);
            for (var index = 0; index < $scope.PopupList.length; index++) {
                if (index == ($scope.PopupList.length - 1)) {
                    $scope.PopupList[index].additionaldiscount = (avail / $scope.GetNumber($scope.PopupList[index].core.qty)).round(2);
                    if ($scope.GetNumber($scope.PopupList[index].core.receivable) < $scope.GetNumber($scope.PopupList[index].additionaldiscount)
                        && $scope.GetNumber($scope.PopupList[index].core.receivable) < $scope.GetNumber($scope.PopupList[index].additionaldiscount) * $scope.GetNumber($scope.PopupList[index].core.qty)) {
                        $scope.PopupList[index].additionaldiscount = $scope.PopupList[index].core.receivable;
                    }
                    $scope.PopupList[index].additionaldiscountpercentage = ((($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty) / ($scope.PopupList[index].core.receivable + $scope.PopupList[index].core.discount + $scope.PopupList[index].core.schemediscount)) * 100).round(2);
                }
                else {
                    $scope.PopupList[index].additionaldiscount = ($scope.PopupList[index].core.receivable * percent / 100 / $scope.PopupList[index].core.qty).round(2);
                    $scope.PopupList[index].additionaldiscountpercentage = ((($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty) / ($scope.PopupList[index].core.receivable + $scope.PopupList[index].core.discount + $scope.PopupList[index].core.schemediscount)) * 100).round(2);
                    avail -= ($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty);
                }
                if (applymode != 'COUPON') {
                    $scope.PopupList[index].core.attributes.rewarddiscount = $scope.PopupList[index].additionaldiscount;
                    $scope.PopupList[index].core.attributes.rewarddiscountpercentage = $scope.PopupList[index].additionaldiscountpercentage;
                }
                $scope.PopupList[index].core.attributes.coupondiscount = $scope.PopupList[index].additionaldiscount;
                $scope.PopupList[index].core.attributes.coupondiscountpercentage = $scope.PopupList[index].additionaldiscountpercentage;

                $scope.PopupList[index].receivable -= ($scope.PopupList[index].additionaldiscount * $scope.PopupList[index].core.qty);
                $scope.PopupList[index].receivable = $scope.PopupList[index].receivable.round(2);
                if (Math.abs($scope.PopupList[index].receivable) < 0.5) {
                    $scope.PopupList[index].receivable = 0;
                }
            }

            $.each($scope.PopupList, function (k, v) {

                v.core.additionaldiscount += $scope.GetNumber(v.additionaldiscount);
                v.core.additionaldiscountpercentage += $scope.GetNumber(v.additionaldiscountpercentage);

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

            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;
            $scope.CurrentBill.schemediscount = 0;
            $.each($scope.CurrentBill.Billitems, function (k, v) {
                $scope.CurrentBill.tax += v.tax;
                $scope.CurrentBill.discount += v.discount;
                $scope.CurrentBill.additionaldiscount += (v.qty * v.additionaldiscount);
                $scope.CurrentBill.receivable += v.receivable;
                $scope.CurrentBill.schemediscount += v.schemediscount;
            });

            $scope.CurrentBill.rounding = $scope.GetRoundingValue();
            if (isNaN($scope.CurrentBill.rounding)) {
                $scope.CurrentBill.rounding = 0;
                $scope.CurrentBill.receivable = $scope.CurrentBill.receivable.round($scope.system_salesroundingdigit);
            }
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
            $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
        }

        $scope.ShowCustomerDialog = function () {
            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',

                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', function ($scope, $timeout, $http, hotkeys, $rootScope) {

                    $scope.MobileNo10Digits = false || $scope.ngDialogData[0].ApplicationSettings.appsettings.system_supportcustomer10digits;
                    $scope.CustomerMandatory = false || $scope.ngDialogData[0].ApplicationSettings.appsettings.system_salecustomermandatory;
                    $scope.ReligionList = restService.Get('ReferencelistService/RELIGION', true).then(function (results) {
                        $scope.ReligionList = results;
                    });
                    $scope.CustomerCategoryList = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
                        $scope.CustomerCategoryList = results;
                    });
                    $scope.StateList = restService.Get('ReferencelistService/STATE', true).then(function (results) {
                        $scope.StateList = results;
                    });
                    $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
                        $scope.CityList = results;
                    });
                    $scope.AreaList = restService.Get('ReferencelistService/AREA', true).then(function (results) {
                        $scope.AreaList = results;
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
                        if (!$scope.contactno || $scope.contactno == null || $scope.contactno == '') {
                            $rootScope.Toaster.Error('Please enter the customer contact no');
                            $scope.setFocuscustomer_contact();
                            return;
                        }
                        if ($scope.MobileNo10Digits && $scope.contactno.length != 10) {
                            $rootScope.Toaster.Error('Please enter 10 digits customer contact no');
                            $scope.setFocuscustomer_contact();
                            return;
                        }
                        if ($scope.CustomerMandatory) {
                            if (!$scope.name || $scope.name == null || $scope.name == '') {
                                $rootScope.Toaster.Error('Please enter the customer name');
                                $scope.setFocuscustomer_name();
                                return;
                            }
                        }
                        if ($scope.gstno != 'undefined' && $scope.gstno != null
                            && $scope.gstno != '') {
                            var gstinformat = new RegExp('^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$');
                            if (!gstinformat.test($scope.gstno)) {
                                $scope.setFocuscustomer_gstno();
                                $rootScope.Toaster.Error("Please Enter valid GST number.");
                                return;
                            }
                        }

                        var request = {
                            "Item": {
                                'id': $scope.id, 'version': $scope.version, 'isactive': $scope.isactive,
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno,
                                'registredlocationid': $rootScope.LocationInfo.location.id,
                                'religionid': $scope.religionid, 'customertype': 4,
                                'cityid': $scope.cityid, 'dateofbirth': $scope.dateofbirth,
                                'area': $scope.area, 'billingname': $scope.billingname,
                                'emailid': $scope.emailid, 'address': $scope.address,
                                'categoryid': $scope.categoryid, 'stateid': $scope.stateid,
                                'locationid': $rootScope.LocationInfo.location.id
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Save",
                            data: srequest
                        }).success(function (result) {
                            if (result.id > 0)
                                $scope.closeThisDialog(result);
                            else
                                $rootScope.Toaster.Error(result.ErrorMessage);

                        });
                    };

                    $scope.hasvalue = function (val) {
                        return typeof (val) != 'undefined' && val != null &&
                            val != '' && val.length > 0;
                    }

                    $scope.SetFocusByFlow = function (focusid) {
                        if (UserDetails.Info.p_HIDECUSTOMERCAT.view)
                            $scope['setFocus' + 'customer_religion']();
                        else
                            $scope['setFocus' + focusid]();
                        return;
                    };

                    $scope.ManageSearchItem = function (action, item) {
                        if (action == "Edit") {
                            $scope.id = item.id;
                            $scope.name = item.name;
                            $scope.area = item.area;
                            $scope.contactno = item.mobileno;
                            $scope.gstno = item.gstno;
                            $scope.hasgst = item.hasgst;
                            $scope.loyeltyno = item.loyeltyno;
                            $scope.religionid = item.religionid;
                            $scope.customertype = item.customertype;
                            $scope.cityid = item.cityid;
                            $scope.dateofbirth = item.dateofbirth;
                            $scope.area = item.area;
                            $scope.stateid = item.stateid;
                            $scope.billingname = item.billingname;
                            $scope.emailid = item.emailid;
                            $scope.address = item.address;
                            $scope.categoryid = item.categoryid;
                            $scope.version = item.version;
                            $scope.isactive = item.isactive;
                        }
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
                                'likemobileno': true,
                                'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno,
                                'locationid': $rootScope.LocationInfo.location.id
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

                        if (!$scope.sale_allowchangeofcustomer && $scope.CurrentBill.customerid != 'undefined' && $scope.CurrentBill.customerid != null
                            && $scope.CurrentBill.customerid != 0 && $scope.CurrentBill.Billitems.length > 0) {
                            $rootScope.Toaster.Error("Customer Already selected, Please clear all the barcode if you want to change.");
                            return;
                        }

                        $scope.CurrentBill.customerid = data.value.id;
                        $scope.CurrentBill.code = '';
                        $scope.CurrentBill.phoneno = data.value.mobileno;
                        $scope.CurrentBill.customername = data.value.name;
                        $scope.CurrentBill.customergst = data.value.gstno;
                        $scope.CurrentBill.customerstateid = data.value.stateid;
                        $scope.CurrentBill.customergst = data.value.gstno;

                        if (typeof ($scope.CurrentBill.billattributes) == 'undefined')
                            $scope.CurrentBill.billattributes = {};
                        $scope.CurrentBill.billattributes.remarks = data.value.gstno;

                        $scope.CurrentBill.disableloyalty = data.value.disableloyalty;
                        $scope.CurrentBill.customerdiscount = data.value.discount;
                        $scope.CurrentBill.ccmargin = data.value.ccmargin;
                        $scope.CurrentBill.ccloyelty = !data.value.disableloyalty && data.value.loyeltyno != null
                            && data.value.loyeltyno != '' && data.value.loyeltyno.length > 1;
                        $scope.CurrentBill.ccmarginmode = data.value.ccmarginmode;
                        $scope.CurrentBill.billdiscountinclude = data.value.billdiscountinclude;
                        $scope.CurrentBill.rewardunit = data.value.rewardunit;
                        $scope.CurrentBill.discountotp = data.value.discountotp;
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
                }
            });
        }

        $scope.qEncodeURIComponent = function (value) {
            if (value == undefined || value == '')
                value = '-';
            if (value.indexOf('.') >= 0)
                value = value.replaceAll('.', '~dot~');
            if (value.indexOf('/') >= 0)
                value = value.replaceAll('/', '~sls~');
            if (value.indexOf('*') >= 0)
                value = value.replaceAll('*', '~str~');
            return encodeURIComponent(value);
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
                                    $scope.EMIScheme.dbdvalue = ($scope.BillValue * scheme.dbdpercentage / 100).round($scope.system_salesroundingdigit);
                                }
                                else {
                                    $scope.EMIScheme.dbdvalue = scheme.dbdvalue;
                                }
                                if (scheme.documentchargepercentage > 0) {
                                    $scope.EMIScheme.documentcharge = ($scope.BillValue * scheme.documentchargepercentage / 100).round($scope.system_salesroundingdigit);
                                }
                                else {
                                    $scope.EMIScheme.documentcharge = scheme.documentcharge;
                                }
                                $scope.EMIScheme.emicardcharge = scheme.emicardcharge;
                            }

                            var marginmoney = $scope.ParentScop.GetNumber($scope.EMIScheme.marginmoney);
                            var advanceamt = (($scope.BillValue - marginmoney) / $scope.EMIScheme.tenure * $scope.EMIScheme.advancemonth).round($scope.system_salesroundingdigit);
                            advanceamt += $scope.ParentScop.GetNumber($scope.EMIScheme.marginmoney);
                            var loanAmount = ($scope.BillValue - advanceamt);
                            var numberOfMonths = $scope.EMIScheme.tenure - $scope.EMIScheme.advancemonth;

                            var monthlyInterestRatio = ($scope.EMIScheme.rateofinterest / 100) / 12;
                            var top = Math.pow((1 + monthlyInterestRatio), numberOfMonths);
                            var bottom = top - 1;
                            var sp = top / bottom;
                            var emi = (monthlyInterestRatio <= 0 ? loanAmount / numberOfMonths : ((loanAmount * monthlyInterestRatio) * sp)).round($scope.system_salesroundingdigit);
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
            if (action == 'remove') {
                var selectedrows = $scope.igrid.GetSelectedRows();

                $rootScope.Confirm('Remove', 'Do you want to remove all the selected items - ' + selectedrows.length, function () {
                    $.each(selectedrows, function (k, v) {
                        $scope.ManageBillItem('Delete', v);
                    });
                });
            }
            else {
                $scope.autoCoding.code = key;
                $scope.SearchAutoCoding();
            }
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
                //else if (!$scope.IsSaleReturn && v.receivable + $scope.GetNumber(v.additionaldiscount) <= 0)
                //{
                //    //no discount allowed for free scheme goods
                //}
                else {
                    if ($scope.system_disableaddlndisondiscounteditems && $scope.system_disableaddlndisonschemeitems) {
                        if ((v.discount == 'undefined' || v.discount == null || v.discount == 0)
                            && (v.schemeid == 'undefined' || v.schemeid == null || v.schemeid == 0)) {
                            list.push({
                                'core': v, 'additionaldiscount': v.additionaldiscount,
                                'additionaldiscountpercentage': v.additionaldiscountpercentage,
                                'receivable': v.receivable
                            });
                        }
                    }
                    else if ($scope.system_disableaddlndisondiscounteditems && !$scope.system_disableaddlndisonschemeitems) {
                        if (v.discount == 'undefined' || v.discount == null || v.discount == 0) {
                            list.push({
                                'core': v, 'additionaldiscount': v.additionaldiscount,
                                'additionaldiscountpercentage': v.additionaldiscountpercentage,
                                'receivable': v.receivable
                            });
                        }
                    }
                    else if (!$scope.system_disableaddlndisondiscounteditems && $scope.system_disableaddlndisonschemeitems) {
                        if (v.schemeid == 'undefined' || v.schemeid == null || v.schemeid == 0) {
                            list.push({
                                'core': v, 'additionaldiscount': v.additionaldiscount,
                                'additionaldiscountpercentage': v.additionaldiscountpercentage,
                                'receivable': v.receivable
                            });
                        }
                    }
                    else {
                        list.push({
                            'core': v, 'additionaldiscount': v.additionaldiscount,
                            'additionaldiscountpercentage': v.additionaldiscountpercentage,
                            'receivable': v.receivable
                        });
                    }
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
                    $scope.selectallitems = false;
                    $scope.passcodeApprovalamount = 0;
                    $scope.CanApprove = $rootScope.UserInfo.p_CANAPPROVEDISCOUNT.view;
                    $scope.system_salesroundingdigit = $scope.ngDialogData[0].system_salesroundingdigit;
                    $scope.sale_requestpasscodetosms = $scope.ngDialogData[0].sale_requestpasscodetosms;
                    $scope.system_salesrounding = $scope.ngDialogData[0].system_salesrounding;
                    $scope.system_adonfinalprice = $scope.ngDialogData[0].system_adonfinalprice;
                    $scope.system_adonschemediscount = $scope.ngDialogData[0].system_adonschemediscount;
                    $scope.system_posexclusivetax = $scope.ngDialogData[0].system_posexclusivetax;

                    $scope.CanAskPasscode = !$scope.sale_requestpasscodetosms;
                    $scope.PassCodeRequested = false;
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
                            item.additionaldiscount = (item.core.salerate * parseFloat(discountparts[0]) / 100).round($scope.system_salesroundingdigit);
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round($scope.system_salesroundingdigit);
                            item.additionaldiscount += (item.receivable * parseFloat(discountparts[1]) / 100 / item.core.qty).round($scope.system_salesroundingdigit);
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round(2);
                            item.additionaldiscountpercentage = ((item.additionaldiscount / item.core.salerate) * 100).round(2);
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
                                $scope.passcodeApprovalamount = $scope.discountvalue;
                                if (isNaN($scope.AD)) $scope.AD = 0;
                                $scope.CanAskPasscode = true;
                                $scope.PassCodeRequested = true;
                                $timeout(function () {
                                    $scope.setFocuspasscode();
                                    $scope.$digest();
                                }, 10);

                            });
                        }
                        else
                            $rootScope.Toaster.Notice("Pass Code requeste not enabled.");
                    }

                    $scope.RequestPermission = function () {

                        if ($scope.CanApprove) {
                            $scope.Approve();
                            return;
                        }

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

                            if ($scope.passcodeApprovalamount > 0 && $scope.passcodeApprovalamount != $scope.discountvalue) {
                                $rootScope.Toaster.Error("There is a mismatch in Requested Discount and Current Discount Value");
                                return;
                            }

                            $rootScope.Toaster.Notice(result.employee + " approved for discount");
                            $scope.AD = parseFloat(result.alloweddiscount);
                            if (isNaN($scope.AD)) $scope.AD = 0;
                            //$scope.CanApprove = true;
                            $scope.Approve();
                        });
                    }

                    $scope.Approve = function () {
                        if ($scope.GetNumber($scope.discountpercentage) == isNaN || $scope.GetNumber($scope.discountpercentage) < 0) {
                            $rootScope.Toaster.Error("Please enter valid discount percentage ");
                            return;
                        }
                        var hasExcess = 0;
                        $.each($scope.PopupList, function (k, v) {
                            var adp = parseFloat(v.additionaldiscountpercentage);
                            if (isNaN(adp)) adp = 0;
                            if (adp > $scope.AD) {
                                hasExcess++;
                            }
                        });
                        if (hasExcess > 0) {
                            $rootScope.Toaster.Error("Your allowed discount maximum @" + $scope.AD + '%');
                            return;
                        }
                        $scope.closeThisDialog({ 'items': $scope.PopupList, 'status': 'approved', 'rounding': $scope.RoundingValue });
                    };

                    $scope.CanGiveDiscount = function () {
                        if (!$scope.CanApprove) {
                            $rootScope.Toaster.Error("You dont' have rights to give additional discount!");
                            return;
                        }

                        if ($scope.GetNumber($scope.discountpercentage) == isNaN || $scope.GetNumber($scope.discountpercentage) < 0) {
                            $rootScope.Toaster.Error("Please enter valid discount percentage ");
                            return;
                        }
                        var hasExcess = 0;
                        $.each($scope.PopupList, function (k, v) {
                            var adp = parseFloat(v.additionaldiscountpercentage);
                            if (isNaN(adp)) adp = 0;
                            if (adp > $scope.AD) {
                                hasExcess++;
                            }
                        });
                        if (hasExcess > 0) {
                            $rootScope.Toaster.Error("Your allowed discount maximum @" + $scope.AD + '%');
                            $scope.CanApprove = false;
                            return;
                        }
                        $scope.CanApprove = true;
                    };

                    $scope.Request = function () {
                        $scope.closeThisDialog({ 'items': $scope.PopupList, 'status': 'request', 'rounding': $scope.RoundingValue });
                    };

                    $scope.CompleteDiscount = function (option) {
                        if ($scope.PassCodeRequested) return;
                        if (option == '%' && $scope.discountpercentage.toString().indexOf('+') >= 0) {

                            var discountparts = $scope.discountpercentage.toString().split('+');
                            if (discountparts[0].length == 0) discountparts[0] = '0';
                            var totaldiscount = 0;
                            $.each($scope.GetSelectedList(), function (k, v) {
                                if ($scope.IsSaleReturn || v.core.qty > 0) {
                                    var baserate = v.core.salerate - ($scope.system_adonfinalprice ? (v.core.discount / v.core.qty) : 0);
                                    v.additionaldiscount = (baserate * parseFloat(discountparts[0]) / 100).round($scope.system_salesroundingdigit);
                                    v.receivable = (v.core.receivable + (v.core.additionaldiscount * v.core.qty)) - (v.additionaldiscount * v.core.qty);
                                    v.additionaldiscount += (v.receivable * parseFloat(discountparts[1]) / 100 / v.core.qty).round($scope.system_salesroundingdigit);
                                    v.additionaldiscountpercentage = ((v.additionaldiscount / baserate) * 100).round(2);
                                    v.receivable = (v.core.receivable + (v.core.additionaldiscount * v.core.qty)) - (v.additionaldiscount * v.core.qty);
                                    v.receivable = v.receivable.round(2);
                                    if (!$scope.IsSaleReturn && v.receivable < 0) {
                                        v.additionaldiscount = v.core.additionaldiscount;
                                        v.additionaldiscountpercentage = v.core.additionaldiscountpercentage;
                                        v.receivable = v.core.receivable;
                                    }
                                    totaldiscount += (v.additionaldiscount * v.core.qty);
                                    $scope.discountpercentage = v.additionaldiscountpercentage;
                                }
                            });
                            $scope.discountvalue = totaldiscount;
                            $scope.ComputeTotal();
                            return;
                        } else if (option == '%' && $scope.discountpercentage.toString().toLowerCase() == 'r') {

                            var plist = $scope.GetSelectedList();
                            for (var index = 0; index < plist.length; index++) {

                                plist[index].additionaldiscount = (plist[index].core.discount * -1 / plist[index].core.qty).round(2);
                                plist[index].additionaldiscountpercentage = plist[index].core.discountpercentage * -1;

                                plist[index].receivable = plist[index].core.receivable - plist[index].core.discount;
                                plist[index].receivable = plist[index].receivable.round(2);
                            }

                        }

                        $scope.CanGiveDiscount();

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
                        if ($scope.system_posexclusivetax) {
                            if (type == '%') {
                                item.etdiscount = (item.core.etprice * newvalue / 100).round(2);
                                item.etdiscountpercentage = newvalue.round(2);
                            }
                            else {
                                item.etdiscount = newvalue.round(2);
                                item.etdiscountpercentage = ((item.etdiscount / item.core.etprice) * 100).round(2);
                            }
                            var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);
                            item.additionaldiscountpercentage = item.etdiscountpercentage;
                            item.additionaldiscount = (baserate * item.etdiscountpercentage / 100).round(2);
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round(2);
                        }
                        else {
                            var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);
                            if (type == '%') {
                                item.additionaldiscount = (baserate * newvalue / 100).round(2);
                                item.additionaldiscountpercentage = newvalue.round(2);
                            }
                            else {
                                item.additionaldiscount = newvalue.round(2);
                                item.additionaldiscountpercentage = ((newvalue / baserate) * 100).round(2);
                            }
                            item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round(2);
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

                        

                        if (type == 'F') {
                            $scope.TotalValue = 0;
                            $.each($scope.GetSelectedList(), function (k, v) {
                                if ($scope.IsSaleReturn || v.core.qty > 0) {
                                    if ($scope.system_posexclusivetax)
                                        $scope.TotalValue += v.core.qty * v.core.etprice;
                                    else {
                                        $scope.TotalValue += v.core.receivable;                                        
                                    }
                                }
                            });
                            if ($scope.TotalValue < newvalue) {
                                $rootScope.Toaster.Error("Discount lesser than total bill value");
                                return;
                            }
                            newvalue = $scope.TotalValue - newvalue;
                            type = 'V';
                            $scope.discountvalue = newvalue;
                        } else {
                            $scope.TotalValue = 0;
                            $.each($scope.GetSelectedList(), function (k, v) {
                                if ($scope.IsSaleReturn || v.core.qty > 0) {
                                    if ($scope.system_posexclusivetax)
                                        $scope.TotalValue += v.core.qty * v.core.etprice;
                                    else {
                                        $scope.TotalValue += v.core.receivable;
                                        if (!$scope.system_adonfinalprice) {
                                            $scope.TotalValue += v.core.discount;
                                        }
                                    }
                                }
                            });
                        }


   
                        if (type == '%') {
                            var totaldiscount = 0;
                            $.each($scope.GetSelectedList(), function (k, v) {
                                if ($scope.IsSaleReturn || v.core.qty > 0) {
                                    if ($scope.system_posexclusivetax) {
                                        v.etdiscount = (v.core.etprice * newvalue / 100).round(2);
                                        v.etdiscountpercentage = ((v.etdiscount / v.core.etprice) * 100).round(2);
                                        var baserate = v.core.salerate - ($scope.system_adonfinalprice ? (v.core.discount / v.core.qty) : 0);
                                        v.additionaldiscountpercentage = v.etdiscountpercentage;
                                        v.additionaldiscount = (baserate * v.etdiscountpercentage / 100).round(2);
                                        v.receivable = ((v.core.receivable + (v.core.additionaldiscount)) - (v.additionaldiscount * v.core.qty)).round(2);
                                    }
                                    else {
                                        var baserate = v.core.salerate - ($scope.system_adonfinalprice ? (v.core.discount / v.core.qty) : $scope.system_adonschemediscount ? (v.core.schemediscount / v.core.qty) : 0);
                                        //var _baserate = v.core.salerate - ($scope.system_adonfinalprice ? (v.core.discount / v.core.qty) : (v.core.schemediscount / v.core.qty));
                                        v.additionaldiscount = (baserate * newvalue / 100).round(2);
                                        v.additionaldiscountpercentage = newvalue;
                                        v.receivable = (v.core.receivable + (v.core.additionaldiscount * v.core.qty)) - (v.additionaldiscount * v.core.qty);
                                        v.receivable = v.receivable.round(2);
                                    }
                                    if (!$scope.IsSaleReturn && v.receivable < 0) {
                                        v.additionaldiscount = v.core.additionaldiscount;
                                        v.additionaldiscountpercentage = v.core.additionaldiscountpercentage;
                                        v.receivable = v.core.receivable;
                                    }
                                    totaldiscount += (v.additionaldiscount * v.core.qty);
                                }
                            });
                            $scope.discountvalue = totaldiscount;
                        }
                        else {
                            var avail = newvalue;
                            var percent = (avail / Math.abs($scope.TotalValue) * 100).round(2);
                            $scope.discountpercentage = percent;
                            var plist = $scope.GetSelectedList();
                            for (var index = 0; index < plist.length; index++) {
                                var item = plist[index];

                                if ($scope.IsSaleReturn || item.core.qty > 0) {

                                    if ($scope.system_posexclusivetax) {
                                        if (index == (plist.length - 1)) {
                                            item.etdiscount = (avail / Math.abs(item.core.qty)).round(2);
                                            item.etdiscountpercentage = ((item.etdiscount / (item.core.etprice)) * 100).round(2);
                                        }
                                        else {
                                            item.etdiscountpercentage = percent;
                                            item.etdiscount = (percent * item.core.etprice / 100).round(2);
                                            avail -= (item.etdiscount * Math.abs(item.core.qty));
                                        }
                                        var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : 0);
                                        item.additionaldiscountpercentage = item.etdiscountpercentage;
                                        item.additionaldiscount = (baserate * item.etdiscountpercentage / 100).round(2);
                                        item.receivable = ((item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount * item.core.qty)).round(2);
                                    }
                                    else {
                                        var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : $scope.system_adonschemediscount ? (item.core.schemediscount / item.core.qty) : 0);
                                        //var _baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : (item.core.schemediscount / item.core.qty));
                                        if (index == (plist.length - 1)) {
                                            item.additionaldiscount = (avail / Math.abs(item.core.qty)).round(2);
                                            item.additionaldiscountpercentage = ((item.additionaldiscount / (baserate)) * 100).round(2);
                                        }
                                        else {
                                            item.additionaldiscountpercentage = percent;
                                            item.additionaldiscount = ((baserate) * percent / 100).round(2);
                                            avail -= (item.additionaldiscount * Math.abs(item.core.qty));
                                        }
                                        item.receivable = (item.core.receivable + (item.core.additionaldiscount * item.core.qty)) - (item.additionaldiscount * item.core.qty);
                                        item.receivable = item.receivable.round(2);
                                    }

                                    if (!$scope.IsSaleReturn && item.receivable < 0) {
                                        item.additionaldiscount = item.core.additionaldiscount;
                                        item.additionaldiscountpercentage = item.core.additionaldiscountpercentage;
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

                        var baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : $scope.system_adonschemediscount ? (item.core.schemediscount / item.core.qty) : 0);
                        //var _baserate = item.core.salerate - ($scope.system_adonfinalprice ? (item.core.discount / item.core.qty) : (item.core.schemediscount / item.core.qty));

                        if (type == '%') {
                            item.additionaldiscount = (baserate * newvalue / 100).round(2);
                            item.additionaldiscountpercentage = newvalue.round(2);
                        }
                        else {
                            item.additionaldiscount = newvalue.round(2);
                            item.additionaldiscountpercentage = ((item.additionaldiscount / baserate) * 100).round(2);
                        }
                        item.receivable = ((item.core.receivable + (item.core.additionaldiscount * item.core.qty)) - (item.additionaldiscount * item.core.qty)).round(2);
                        if (!$scope.IsSaleReturn && item.receivable < 0) {
                            item.additionaldiscount = item.core.additionaldiscount;
                            item.additionaldiscountpercentage = item.core.additionaldiscountpercentage;
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
                        $scope.discountpercentage = (totaldiscount / $scope.TotalValue * 100).round(2);
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
                            maxDiscount.additionaldiscount -= (data.value.rounding / maxDiscount.core.qty);
                            maxDiscount.additionaldiscount.round(2);
                        }

                        /*$scope.CurrentBill.tax = 0;
                        $scope.CurrentBill.discount = 0;
                        $scope.CurrentBill.receivable = 0;
                        $scope.CurrentBill.rounding = 0;
                        $scope.CurrentBill.total = 0;
                        $scope.CurrentBill.additionaldiscount = 0;*/

                        $.each(data.value.items, function (k, v) {

                            $scope.CurrentBill.total -= v.core.receivable;
                            $scope.CurrentBill.receivable -= v.core.receivable;
                            $scope.CurrentBill.discount -= v.core.discount;
                            $scope.CurrentBill.schemediscount -= v.core.schemediscount;
                            $scope.CurrentBill.additionaldiscount -= (v.core.additionaldiscount * v.core.qty);
                            $scope.CurrentBill.tax -= v.core.tax;

                            v.core.additionaldiscount = $scope.GetNumber(v.additionaldiscount).round(2);
                            v.core.additionaldiscountpercentage = $scope.GetNumber(v.additionaldiscountpercentage).round(2);
                            v.core.etdiscount = $scope.GetNumber(v.etdiscount).round(2);
                            v.core.etdiscountpercentage = $scope.GetNumber(v.etdiscountpercentage).round(2);

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
                        $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
                        $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
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

                    var receivable = data.value.receivable;
                    if ($scope.GetNumber(data.value.paymentinfo.card_paid) > ($scope.GetNumber(receivable) + $scope.GetNumber(data.value.rounding))
                        && receivable > 0) {
                        if ($scope.GetNumber(data.value.paymentinfo.card_paid) - ($scope.GetNumber(receivable) + $scope.GetNumber(data.value.rounding)) >= 1) {
                            $rootScope.Toaster.Error("Card payment can't exceed the bill value");
                            return;
                        }
                    }

                    if ($scope.GetNumber(data.value.paymentinfo.upi_paid) > (receivable + $scope.GetNumber(data.value.rounding))
                        && receivable > 0) {
                        if ($scope.GetNumber(data.value.paymentinfo.upi_paid) - (receivable + $scope.GetNumber(data.value.rounding)) >= 1) {
                            $rootScope.Toaster.Error("UPI payment can't exceed the bill value");
                            return;
                        }
                    }

                    if ($scope.GetNumber(data.value.paymentinfo.upi_paid) > 0 &&
                        (data.value.paymentinfo.upiproviderid == 'undefined' ||
                            data.value.paymentinfo.upiproviderid == null ||
                            data.value.paymentinfo.upiproviderid <= 0)) {
                        if (data.value.paymentinfo.deviceresult == undefined ||
                            data.value.paymentinfo.deviceresult == null) {
                            $rootScope.Toaster.Error("Please select the UPI Provider.");
                            return;
                        }
                    }

                    if (($scope.GetNumber(data.value.paymentinfo.card_paid) > 0 || $scope.GetNumber(data.value.paymentinfo.upi_paid) > 0)
                        && ($scope.GetNumber(data.value.paymentinfo.card_paid) + $scope.GetNumber(data.value.paymentinfo.upi_paid)) > receivable) {
                        $rootScope.Toaster.Error("Card/UPI payment can't exceed the bill value");
                        return;
                    }

                    $scope.CurrentBill.billattributes.settlement = data.value;

                    if ($scope.ValidateBill()) {
                        $scope.SaveBill();
                    }
                }
            });
        }

        $scope.SaveBill = function () {
            var request = {
                "Item": $scope.CurrentBill
            }
            request.Item.printtemplateid = $scope.PrinterData.TemplateID;
            request.Item.papersize = $scope.PrinterData.PaperSize;
            request.Item.saledirectdos = $scope.PrinterData.IsDirectDos;
            request.Item.saledirectpdf = $scope.PrinterData.PaperSize == 9 || $scope.system_saledirectpdf;
            request.Item.directpdf = $scope.PrinterData.PaperSize == 9 || $scope.system_saledirectpdf;
            $scope.SaveRequest = true;


            if ($scope.system_supportdeliveryrack && $scope.system_deliveryrackcard) {
                if ($scope.IsSaleReturn) {
                    $scope.CurrentBill.billattributes.deliveryrackcard = null;
                    $scope.CurrentBill.billattributes.deliveryrack = null;
                }
                else if (($scope.CurrentBill.billattributes == undefined || $scope.CurrentBill.billattributes == null) ||
                    ($scope.CurrentBill.billattributes.deliveryrack == undefined || $scope.CurrentBill.billattributes.deliveryrack == null) ||
                    ($scope.CurrentBill.billattributes.deliveryrackcard == undefined || $scope.CurrentBill.billattributes.deliveryrackcard == null)) {

                    var haspossitiveqty = $scope.CurrentBill.Billitems.find(function (item) {
                        return item.qty > 0;
                    });

                    if (haspossitiveqty && haspossitiveqty != null) {
                        $rootScope.Toaster.Error('Delivery rack number missing');
                        return;
                    }
                }
            }

            $http({
                method: 'POST',
                url: "BillService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {

                $scope.SaveRequest = false;
                if (result.id > 0) {
                    $scope.DeliveryRackInformation = '';
                    var allsaved = true;

                    if (result.Bills &&
                        result.Bills != null) {
                        $.each(result.Bills, function (k, v) {
                            if (v.id <= 0)
                                allsaved = false;
                        });
                    }

                    if (allsaved) {
                        $scope.ErrorMessage = "Successfully saved";
                        if (print) {


                            if (result.Bills && result.Bills != null && (result.printdocument == null || result.printdocument == '')) {
                                $.each(result.Bills, function (k, v) {
                                    $scope.Print(v);
                                    $scope.PrintCopy.Send(v.billcopy);
                                });
                            }
                            else {
                                $scope.Print(result);
                                $scope.PrintCopy.Send(result.billcopy);
                            }

                        }

                        $scope.ErrorMessage = "Successfully saved";
                        if ($scope.DiscountQueue && !$scope.discounttimerenabled && $scope.CurrentBill.additionaldiscount > 0) {
                            $scope.discounttimerenabled = true;
                            $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                        }

                        $scope.AddBillHistory(result, false, true)
                        $scope.CreateNew(true);
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

        $scope.LastSettlement = function () {
            var bill = (JSON.parse(JSON.stringify($scope.lastBills[0])));;
            for (var i = 1; i < $scope.lastBills.length; i++) {
                bill.tax += $scope.lastBills[i].tax;
                bill.discount += $scope.lastBills[i].discount;
                bill.billadditionaldiscount += $scope.lastBills[i].billadditionaldiscount;
                bill.total += $scope.lastBills[i].total;
            }

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

                    var bill = (JSON.parse(JSON.stringify($scope.lastBills[0])));;
                    bill.billattributes = { 'settlement': data.value };
                    var request = {
                        "Item": bill
                    }
                    $http({
                        method: 'POST',
                        url: "BillService/SaveSettlement",
                        data: JSON.stringify(request)
                    }).success(function (result) {
                        if (result)
                            $rootScope.Toaster.Notice("Settlement Updated..");
                    });
                }
            });

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

        $scope.EditPriceEnter = function () {
            $scope.autoCoding.lastbit = '';
            $scope.SalesDetail.price = $scope.GetNumber($scope.autoCoding.price);
            if (!$scope.SalesDetail.attributes)
                $scope.SalesDetail.attributes = {};
            $scope.SalesDetail.attributes.manualprice = true;
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

        $scope.GridConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = null;
            if ($scope.IsSaleReturn)
                igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 30);
            else
                igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 30);

            igrid.options.showHeaderRow = true;
            igrid.options.rowHeight = 30;
            igrid.AddSerialNo(40);

            if (!$scope.system_supportestimate) {
                if ($scope.UserInfo.p_BILLITEM && $scope.UserInfo.p_BILLITEM.delete)
                    igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                if ($scope.UserInfo.p_BILLITEM && $scope.UserInfo.p_BILLITEM.modify)
                    igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }
            else if ($scope.IsSaleReturn || $scope.IsEstimate || $scope.IsBulkSale || (!$scope.IsSaleReturn)) {  //&& $scope.UserInfo.p_SALES.delete
                //igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                //igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                if ($scope.UserInfo.p_BILLITEM && $scope.UserInfo.p_BILLITEM.delete)
                    igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                if ($scope.UserInfo.p_BILLITEM && $scope.UserInfo.p_BILLITEM.modify)
                    igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }


            if ($scope.system_multiplesalemen) {
                igrid.AddButtonColumn('SMMChange', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='SMMChange' class='btn btn-info btn-xs'><i class='fa fa-users' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }
            else {
                igrid.AddButtonColumn('SMChange', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='SMChange' class='btn btn-info btn-xs'><i class='fa fa-user' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }

            igrid.AddSimpleColumn('displaycode', 'Barcode', 120);
            var column = igrid.AddSimpleColumn('printingname', 'Product', 180);
            column.toolTip = function (item) {
                if (typeof (item.schemeid) == 'undefined' || item.schemeid == null || item.schemeid == 0)
                    return '';
                return item.scheme;
            };
            if ($scope.ApplicationSettings.appsettings.system_posscrtemplate == 9) {
                var column = igrid.AddCustomColumn('salesmancodeandname', 'Salesman', 110, function (item, col) {
                    if (item.salesmanname &&
                        item.salesmanname != null &&
                        item.salesmanname != '' &&
                        item.salesmancode &&
                        item.salesmancode != null &&
                        item.salesmancode != '') {
                        return item.salesmancode + ' - ' + item.salesmanname;
                    }
                    return item.salesmanname;
                });
                if ($scope.ShowLastBitQty) {
                    var column = igrid.AddCustomColumn('qty', 'Qty', 100, function (item, col) {
                        return item.journalqty != 0 ? $filter('IndianNumber')(item.qty, 2) + "(" +
                            $filter('IndianNumber')(item.journalqty, 2) + ")" : $filter('IndianNumber')(item.qty, 2);
                    });
                }
                else {
                    igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2);
                }
                if ($scope.system_posexclusivetax) {
                    igrid.AddFormatedColumn('etprice', 'Price', 70, 'N2', 2);
                }
                else {
                    igrid.AddFormatedColumn('salerate', 'Price', 70, 'N2', 2);
                }
                igrid.AddFormatedColumn('receivable', 'Amount', 90, 'N2', 2);
                igrid.AddFormatedColumn('discount', 'Discount', 50, 'N2', 0);
                var column = igrid.AddCustomColumn('adddiscount', 'Addln.Dis', 50, function (item, col) {
                    var val = (item.qty * item.additionaldiscount) + item.schemediscount;
                    if (val != 0) return $filter('IndianNumber')(val, 2);
                    return '';
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('discountpercentage', '%', 50, function (item, col) {
                    var val = item.discountpercentage + item.schemediscountpercentage + item.additionaldiscountpercentage;
                    if (val != 0) return $filter('IndianNumber')(val, 2);
                    return '';
                });
                column.cssClass = 'rightalign';
            }
            else {
                if ($scope.system_posexclusivetax) {
                    igrid.AddFormatedColumn('etprice', 'Price', 70, 'N2', 2);
                }
                else {
                    igrid.AddFormatedColumn('salerate', 'Price', 70, 'N2', 2);
                }

                if ($scope.system_salepricemrpdifference) {
                    var column = igrid.AddCustomColumn('mrp', 'MRP', 70, function (item, col) {
                        if (item.attributes && item.attributes.mrp)
                            return $filter('IndianNumber')(item.attributes.mrp, 2);
                        return '';
                    });
                    column.cssClass = 'rightalign';
                }

                igrid.AddSimpleColumn('taxpercentage', 'Tax %', 40).cssClass = 'rightalign';

                if ($scope.ShowLastBitQty) {
                    var column = igrid.AddCustomColumn('qty', 'Qty', 100, function (item, col) {
                        return item.journalqty != 0 ? $filter('IndianNumber')(item.qty, 2) + "(" +
                            $filter('IndianNumber')(item.journalqty, 2) + ")" : $filter('IndianNumber')(item.qty, 2);
                    });
                }
                else {
                    igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2);
                }

                igrid.AddFormatedColumn('discount', 'Discount', 50, 'N2', 0);

                var column = igrid.AddCustomColumn('adddiscount', 'Addln.Dis', 50, function (item, col) {
                    var val = (item.qty * item.additionaldiscount) + item.schemediscount;
                    if (val != 0) return $filter('IndianNumber')(val, 2);
                    return '';
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('discountpercentage', '%', 50, function (item, col) {
                    var val = item.discountpercentage + item.schemediscountpercentage + item.additionaldiscountpercentage;
                    if (val != 0) return $filter('IndianNumber')(val, 2);
                    return '';
                });
                column.cssClass = 'rightalign';
                igrid.AddFormatedColumn('receivable', 'Amount', 90, 'N2', 2);
                var column = igrid.AddCustomColumn('salesmancodeandname', 'Salesman', 110, function (item, col) {
                    if (item.salesmanname &&
                        item.salesmanname != null &&
                        item.salesmanname != '' &&
                        item.salesmancode &&
                        item.salesmancode != null &&
                        item.salesmancode != '') {
                        return item.salesmancode + ' - ' + item.salesmanname;
                    }
                    return item.salesmanname;
                });
            }

            if ($scope.system_multiplesalemen) {
                var column = igrid.AddCustomColumn('salesmenlist', 'S.M', 110, function (item, col) {
                    if (item.attributes && item.attributes != null && 
                        item.attributes.smcodes && item.attributes.smcodes != null) {
                        var lst = [];
                        $.each(item.attributes.smcodes, function (k, v) {
                            var emp = $scope.EmployeeList.find(function (itm) { return itm.id == v });
                            if (emp && emp != null) {
                                lst.push(emp.code.split(' - ')[0]);
                            }
                        })
                        return lst.join(',');
                    }
                });
            }

            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.setCellCSS('igridbigrows');
            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData().getItem(row);
                rowdata.style = rowdata.schemeid > 0 ? 'igridbigrowswithmark' : '';
                return { 'cssClasses': '' };
            };

            $scope.igrid = igrid;


            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete' || command == 'Edit' || command == 'SMChange' || command == 'SMMChange')
                    $scope.ManageBillItem(command, item);
            });

            igrid.grid.onDblClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'taxpercentage') {
                    $scope.ChangeTaxChange(item);
                }
            });

            $scope.igrid.UpdateData([]);
        }

        $scope.ShowMultiplePrice = function () {
            if ($scope.CurrentBill.Billitems == undefined ||
                $scope.CurrentBill.Billitems == null ||
                $scope.CurrentBill.Billitems.length == undefined ||
                $scope.CurrentBill.Billitems.length == 0) return;

            if ($scope.system_posmultiprice == 1) {
                $rootScope.Prompt('Product Price', 'Price', function (code) {
                    if (code == undefined || code == null || code == '') {
                        $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[0]);
                        return;
                    }
                    var price = code.toString();

                    var url = 'SalablegoodsService/BatchPrice/' +
                        $rootScope.LocationInfo.location.id + '/' + price + '/' +
                        $scope.qEncodeURIComponent($scope.CurrentBill.Billitems[0].barcode);

                    if ($scope.CurrentBill.Billitems[0].attributes &&
                        $scope.CurrentBill.Billitems[0].attributes.serialno &&
                        $scope.CurrentBill.Billitems[0].attributes.serialno != null &&
                        $scope.CurrentBill.Billitems[0].attributes.serialno != '') {
                        url = 'SalablegoodsService/BatchPrice/' +
                            $rootScope.LocationInfo.location.id + '/' + price + '/' +
                            $scope.qEncodeURIComponent($scope.CurrentBill.Billitems[0].attributes.serialno);
                    }

                    $http.get(url).then(function (response) {
                        var results = response.data;
                        if (results.length > 0) {
                            if ($scope.CurrentBill.Billitems[0].attributes == undefined)
                                $scope.CurrentBill.Billitems[0].attributes = {};

                            $scope.CurrentBill.Billitems[0].attributes.multipriceid = results[0].id;
                            $scope.CurrentBill.Billitems[0].attributes.mrp = results[0].mrp;
                            $scope.CurrentBill.Billitems[0].price = results[0].price;
                            $scope.CurrentBill.Billitems[0].salerate = results[0].price;
                            $scope.UpdateItemInfo($scope.CurrentBill.Billitems[0], true);
                            $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                            $scope.UpdateOffer();
                        }
                        else {
                            $rootScope.Toaster.Error(price + " not available");
                            $scope.ShowMultiplePrice();
                        }
                    }, function (response) {
                        $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[0]);
                    });
                }, undefined, undefined, function () {
                    $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[0]);
                });
            }
        }


        $scope.ShowNonuanDialog = function (barcode) {
            var dialog = ngDialog.open({
                template: 'NonuanDialog',
                data: {
                    'scope': $scope,
                    'barcode': barcode.substring(1)
                },
                width: '700px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.Parent = $scope.ngDialogData.scope;
                    $scope.Selected = false;
                    $scope.results = [];
                    $scope.Items = [];
                    $scope.qty = 1;
                    $scope.Search = { 'printingname': $scope.ngDialogData.barcode };
                    if ($scope.Parent.NonUANItems == null) {
                        restService.Get('ReferencelistService/NONUAN', true).then(function (results) {
                            $scope.Parent.NonUANItems = results;
                            $scope.Items = results;
                        });
                    }
                    else {
                        $scope.Items = $scope.Parent.NonUANItems;
                    }
                    $scope.searchchanged = function () {
                        $.each($scope.results, function (k, v) {
                            v.selected = false;
                        })
                        if ($scope.results && $scope.results.length == 1) {
                            $scope.Selected = true;
                            $scope.SelectedItem = $scope.results[0];
                            $scope.SelectedItem.selected = true;
                        }
                        else {
                            $scope.Selected = false;
                            $scope.SelectedItem = null;
                        }
                    }

                    $scope.SelectedItem = null;
                    $scope.ItemSelected = function (item) {
                        if (item.selected) {
                            $scope.Selected = true;
                            $scope.SelectedItem = item;
                            $scope.setFocusuanqty();
                        }
                        else {
                            $scope.Selected = false;
                            $scope.SelectedItem = null;
                        }
                    }

                    $scope.Add = function (item) {
                        if (item && item != null) {
                            item.selected = false;
                            if ($scope.qty == undefined || $scope.qty == null || $scope.qty == '')
                                $scope.qty = 1;

                            $scope.closeThisDialog({
                                'item': item,
                                'qty': parseFloat($scope.qty)
                            });
                        }
                        if ($scope.Selected) {
                            $scope.SelectedItem.selected = false;
                            $scope.closeThisDialog({
                                'item': $scope.SelectedItem,
                                'qty': parseFloat($scope.qty)
                            });
                        }
                    };

                    $timeout(function () {
                        $scope.searchchanged();
                    })

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.SalesDetail.printingname = '';
                        $scope.SalesDetail.productid = 0;
                        $scope.SalesDetail.sellingmode = 0;
                        $scope.SalesDetail.barcodeDetail = null;
                        $scope.SalesDetail.qtyeditable = false;
                        $scope.SalesDetail.priceeditable = false;
                        $scope.autoCoding.code = data.value.item.gln;
                        $scope.autoCoding.qty = data.value.qty;
                        $scope.autoCoding.ecode = '';
                        $scope.autoCoding.autoenter = true;
                        $scope.SearchBarcodeDirect(false);
                    }
                }
            });
        }

        $scope.ExchangeSelection = function () {

            var PromotionItems = [];
            var ExchangeItems = [];
            $.each($scope.CurrentBill.Billitems, function (k, v) {

                if (v.qty < 0 && v.schemeid > 0) {
                    var newv = JSON.parse(JSON.stringify(v));
                    newv.qty = newv.qty * -1;
                    newv.discount = newv.discount * -1;
                    newv.schemediscount = newv.discount / newv.qty;
                    if (newv.sellingmode != 3 && newv.qty > 1) {
                        var tqty = newv.qty;
                        for (var i = 0; i < tqty; i++) {
                            newv = JSON.parse(JSON.stringify(newv));
                            newv.qty = 1;
                            newv.promotionid = PromotionItems.length + 1;
                            PromotionItems.push(newv);
                        }
                    }
                    else {
                        newv.promotionid = PromotionItems.length + 1;
                        PromotionItems.push(newv);
                    }
                }
                else if (v.qty > 0 && (v.schemeid == undefined || v.schemeid == 0)) {
                    var newv = JSON.parse(JSON.stringify(v));
                    newv.exchangeid = ExchangeItems.length + 1;
                    newv.display = newv.barcode + '@' + newv.salerate;
                    ExchangeItems.push(newv);
                }
            });

            var dialog = ngDialog.open({
                template: 'PromotionExchange',
                data: {
                    'scope': $scope,
                    'PromotionItems': PromotionItems,
                    'ExchangeItems': ExchangeItems,
                },
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData.scope;
                    $scope.ExchangeItems = $scope.ngDialogData.ExchangeItems;
                    $scope.PromotionItems = $scope.ngDialogData.PromotionItems;
                    $scope.ExchangeSelected = function (item) {
                        var exchangeitem = $scope.ExchangeItems.find(function (sitem) { return item.exchangeid == sitem.exchangeid });
                        item.exchangesalerate = 0;
                        item.exchangebarcode = '';
                        if (exchangeitem && exchangeitem != null) {
                            item.exchangesalerate = exchangeitem.salerate;
                            item.exchangebarcode = exchangeitem.barcode;
                            item.exchangeserialno = exchangeitem.attributes && exchangeitem.attributes != null ? exchangeitem.attributes.serialno : '';
                            item.exchangeqty = exchangeitem.qty;
                        }
                    }

                    $scope.Selected = function () {
                        var errormessage = [];
                        var exchangeoffers = [];

                        $.each($scope.PromotionItems, function (k, v) {
                            if (!(v.exchangeid == undefined ||
                                v.exchangeid == null ||
                                v.exchangeid == 0)) {
                                exchangeoffers.push({
                                    'sellingmode': v.sellingmode,
                                    'promotionid': v.promotionid,
                                    'exchangeid': v.exchangeid,
                                    'rtnsrcitemid': v.rtnsrcitemid,
                                    'returnbillitemid': v.returnbillitemid,
                                    'promotionbarcode': v.barcode,
                                    'exchangebarcode': v.exchangebarcode,
                                    'qty': v.qty,
                                    'eqty': v.exchangeqty
                                })
                            }
                        });

                        if (errormessage.length > 0) {
                            $rootScope.Toaster.Error(errormessage.join('<br>'));
                            return;
                        }
                        var request = {
                            "Item": {
                                'locationid': $scope.Parent.CurrentBill.locationid,
                                'items': exchangeoffers
                            }
                        }
                        $http({
                            method: 'POST',
                            url: "BillService/PromotionExchange",
                            data: JSON.stringify(request)
                        }).success(function (result) {
                            $scope.closeThisDialog(
                                result.exchangeitems
                            );
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var exchangeitems = data.value;

                        $.each($scope.CurrentBill.Billitems, function (k, v) {
                            if (v.qty > 0
                                && (v.schemeid == undefined || v.schemeid == 0)
                                && (v.additionaldiscount == undefined || v.additionaldiscount == 0)) {
                                var exchange = exchangeitems.find(function (sitem) {
                                    return sitem.exchangebarcode == v.barcode;
                                });
                                if (exchange && exchange != null) {
                                    var additionaldiscount = (v.price * exchange.discountpercentage / 100).round(2);
                                    if (additionaldiscount > exchange.discountmax)
                                        additionaldiscount = exchange.discountmax;
                                    v.additionaldiscount = additionaldiscount;
                                    v.additionaldiscountpercentage = (v.additionaldiscount / v.price * 100).round(2);
                                    v.baseprice = (v.price - v.additionaldiscount - (v.schemediscount / v.qty).round(2));
                                    v.receivable = (v.baseprice * v.qty).round(2);
                                    $scope.UpdateTax(v);
                                }
                            }
                        });

                        $scope.CurrentBill.tax = 0;
                        $scope.CurrentBill.discount = 0;
                        $scope.CurrentBill.receivable = 0;
                        $scope.CurrentBill.rounding = 0;
                        $scope.CurrentBill.total = 0;
                        $scope.CurrentBill.additionaldiscount = 0;
                        $scope.CurrentBill.schemediscount = 0;
                        $.each($scope.CurrentBill.Billitems, function (k, v) {
                            $scope.CurrentBill.tax += v.tax;
                            $scope.CurrentBill.discount += v.discount;
                            $scope.CurrentBill.additionaldiscount += (v.qty * v.additionaldiscount);
                            $scope.CurrentBill.receivable += v.receivable;
                            $scope.CurrentBill.schemediscount += v.schemediscount;
                        });
                        $scope.CurrentBill.rounding = $scope.GetRoundingValue();
                        $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
                        $scope.igrid.UpdateData($scope.CurrentBill.Billitems, '_id');
                    }
                }
            });
        }

        $scope.SelectMultipleSalesmen = function (item,allitems) {
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
                                return itm.code.startsWith(code+' - ');
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


        $scope.$on("F2Pressed", function () {
            $scope.setFocusautocoding();
        });

        $scope.LastBillGridConfig = function () {

            if ($scope.HoldList && $scope.HoldList != null && $scope.HoldList.enabled)
                $scope.HoldList.destroy();

            if ($scope.agrid != null)
                $scope.agrid.destroy();

            var igrid = new InfyGrid('lastBillTable', '450px', '100%', true, false, true,
                $scope.system_salecollectrefemp ? 420 : 390);
            igrid.options.rowHeight = 30;
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddFormatedColumn('total', 'Amount', 90, 'N2', 2);

            if ($scope.UserInfo.p_SALES.modify) {
                igrid.AddButtonColumn('Cancel', '', 40, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Cancel' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }

            if ($scope.UserInfo.p_SALES.print) {
                igrid.AddButtonColumn('Print', '', 40, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' class='btn btn-primary btn-xs'><i class='fa fa-print' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }
            igrid.Initialize();
            $scope.agrid = igrid;
            $scope.agrid.UpdateData([]);

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Cancel')
                    $scope.Cancel(item);
                else if (command == 'Print')
                    $scope.Print(item);

            });

            $scope.lastBills = restService.Get('BillmasterService/LastBills', true).then(function (results) {
                $scope.lastBills = results;
                if (typeof ($scope.agrid) != 'undefined' && $scope.agrid != null)
                    $scope.agrid.UpdateData($scope.lastBills);
            });
        }

        $scope.focusconditiononenter = function (issuccess, sfocus, ffocus) {
            $scope[issuccess ? 'setFocus' + sfocus : 'setFocus' + ffocus]();
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

            $rootScope.slideToggeler.Toggle(true);
            $scope.$on(
                "$destroy",
                function (event) {
                    var removecontext = true;
                    if ($scope.system_posvoid &&
                        $scope.CurrentBill && $scope.CurrentBill != null) {
                        if (($scope.CurrentBill.Billitems &&
                            $scope.CurrentBill.Billitems != null &&
                            $scope.CurrentBill.Billitems.length > 0) ||
                            ($scope.CurrentBill.voiditems &&
                                $scope.CurrentBill.voiditems != null &&
                                $scope.CurrentBill.voiditems.length > 0)) {
                            $scope.LocalContext.Post('VoidBills', $scope.CurrentBill, false);
                            $scope.ConfirmWidow.SetEnable(true);
                            removecontext = false;
                        }
                    }

                    if (removecontext) {
                        $scope.LocalContext.Remove('VoidBills');
                        $scope.ConfirmWidow.SetEnable(false);
                    }

                    if ($scope.UpdateOffertimeout != null)
                        $timeout.cancel($scope.UpdateOffertimeout);
                }
            );
            $scope.setFocusautocoding();
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

        $scope.CheckManualDiscount = function (next) {
            if ($scope.AD > 0) {
                $scope['setFocus' + next]();
            }
            else {
                $scope.AddManualBillItem();
            }
        }

        $scope.HyperProductSelected = function (product) {
            $scope.autoCoding.code = product.code;
        }

        $scope.ManualProductSelected = function (product) {
            $scope.autoCoding.code = product.code;
            $scope.SearchBarcodeDirect(false);
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

        $scope.PrintCopy = {
            Send: function (billcopy) {
                if (billcopy == null) return;

                var printerData = localStorageService.get('PrinterData');
                if (printerData == null) {
                    return;
                }

                var request = {
                    'id': printerData.printerid,
                    'content': billcopy.content,
                    'mode': billcopy.mode,
                    'encoded': '',
                    'destination': billcopy.destination
                };
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: 'http://' + printerData.server.name + ':' + printerData.server.port + '/TextilePOS/TransferData',
                    data: srequest
                }).success(function (result) {

                });
            }
        }

        $scope.autocomplete_options = {
            suggest: suggest_state,
            on_select: function (selected) {
                if ($scope.system_supportmanualbill && selected.obj && selected.obj != null && selected.obj.type == 'PRODUCT') {
                    $scope.ManualProductSelected(selected.obj);
                    return;
                }
                if ($scope.system_ishypermarketapplication && selected.obj && selected.obj != null && selected.obj.type == 'PRODUCT') {
                    $scope.HyperProductSelected(selected.obj);
                    return;
                }
                $scope.suggest_selected = selected.obj;
            }
        };
        $scope.openNav = function () {
            document.getElementById("mySidenav").style.width = "50px";
        }
        $scope.closeNav = function () {
            document.getElementById("mySidenav").style.width = "0";
        }
    }]);