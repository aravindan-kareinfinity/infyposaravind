
var app = angular.module('InfinityLab');

app.controller('ApplicationSettingsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;

        $scope.OrganizationSelected = false;
        $scope.SearchOrganization = function () {
            $scope.OrganizationSelected = false;
            $scope.OrganizationName = "";
            restService.Get('OrganisationService/Search/' + $scope.organizationcode, true).then(function (results) {
                $scope.CurrentOrganization = results;
                if ($scope.CurrentOrganization != null) {
                    $scope.OrganizationSelected = true;
                    $scope.OrganizationName = $scope.CurrentOrganization.name;
                    $scope.SearchCondition.organizationid = $scope.CurrentOrganization.id;
                    restService.Get('ReferencelistService/SAAS/' + $scope.CurrentOrganization.id + '/LOCATION', true).then(function (results) {
                        $scope.Location = results;
                        results.push({ 'id': 0, 'name': 'Global' });
                    });
                    restService.Get('ReferencelistService/SAAS/' + $scope.CurrentOrganization.id + '/CITY', true).then(function (results) {
                        $scope.AreaList = results;
                    });
                    restService.Get('ReferencelistService/SAAS/' + $scope.CurrentOrganization.id + '/CUSTOMERCATEGORY', true).then(function (results) {
                        $scope.CustomerCategory = results;
                    });
                    restService.Get('ReferencelistService/SAAS/' + $scope.CurrentOrganization.id + '/SEASONS', true).then(function (results) {
                        $scope.SeasonList = results;
                    });
                    restService.Get('ReferencelistService/SAAS/' + $scope.CurrentOrganization.id + '/BULKSALEREASON', true).then(function (results) {
                        $scope.SaleReason = results;
                    });
                    restService.Get('ReferencelistService/SAAS/' + $scope.CurrentOrganization.id + '/DESIGNATION', true).then(function (results) {
                        $scope.Designations = results;
                    });
                }
            });
        }



        $scope.BirthdayRewardModes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Month' },
            { 'id': 2, 'name': 'Range' }
        ];

        $scope.IncentiveModes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 4, 'name': 'Fixed %' },
            { 'id': 1, 'name': 'Target Based' },
            { 'id': 2, 'name': 'Section Price Slab Based' },
            { 'id': 8, 'name': 'Section Age Slab Based' },
            { 'id': 3, 'name': 'Product Price Slab Based' },
            { 'id': 7, 'name': 'Product Age Slab Based' },
            { 'id': 5, 'name': 'Employee And Product Age' },
            { 'id': 6, 'name': 'Employee Incentive Config' },
        ];


        $scope.POModes = [
            { 'id': 0, 'name': 'Textile' },
            { id: -1, name: 'UAN/GLN' },
            { id: -2, name: 'Hypermarket' },
            { id: -3, name: 'Vendor Agreement' },
            { id: -4, name: 'Daily Trade' },
            { id: -5, name: 'UAN PO Upload' },
            { id: -9, name: 'Product Design' }];


        $scope.BusinessModes = [
            { 'id': 0, 'name': 'Default' },
            { 'id': 1, 'name': 'Size' },
            { 'id': 2, 'name': 'Design_SizeGroup' },
            { 'id': 3, 'name': 'Design_SizeGroup_Colour' },
            { 'id': 4, 'name': 'Design_SizeRange_ColourShade' },
            { 'id': 5, 'name': 'UAN' },
            { 'id': 6, 'name': 'Label' },
        ];

        $scope.TimeoutList = [
            { 'id': 0, 'name': 'Default' },
            { 'id': 1, 'name': '1 Minute' },
            { 'id': 1, 'name': '3 Minute' },
            { 'id': 1, 'name': '5 Minute' },
            { 'id': 1, 'name': '10 Minute' },
            { 'id': 1, 'name': '15 Minute' }
        ];

        $scope.TailoringModes = [
            { 'id': 0, 'name': 'Default' },
            { 'id': 1, 'name': 'Wages' }
        ];

        $scope.FinancePostModes = [
            { 'id': 0, 'name': 'Default' },
            { 'id': 1, 'name': 'Oodo' }
        ];

        $scope.QRIntegrationModes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 11, 'name': 'razorpay', 'default': '{"appKey":"3ab65571-9f8c-41a9-9c61-c2f695d6821c","username":"2345234502","password":"123456Q","sendurl":"https://demo.ezetap.com/api/2.0/merchant/qrcode/generate","statusurl":"http://demo.ezetap.com/api/2.0/payment/status"}' },
            { 'id': 12, 'name': 'paytm', 'default': '{"mid":"Sparki03231555459213","accesskey":"j4VEpzLW5#DpePW9", "version": "V1","clientId": "C11","sendurl":"https://securegw.paytm.in/paymentservices/qr/create","statusurl":"https://securegw.paytm.in/v3/order/status"}' }
        ];

        $scope.IntegrationModes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'innoviti', 'default': '8080' },
            { 'id': 2, 'name': 'razorpay', 'default': '{"appKey":"9fd452ef-1812-4483-83c2-16868f9d56d0","username":"0108202300","password":"123456Q","sendurl":"https://demo.ezetap.com/api/3.0/p2p/start","statusurl":"https://demo.ezetap.com/api/3.0/p2p/status","cancelurl":"https://demo.ezetap.com/api/3.0/p2p/cancel"}' },
            { 'id': 3, 'name': 'pinelab', 'default': '{"MerchantStorePosCode":"1221258055","MerchantID":29610,"SecurityToken":"a4c9741b-2889-47b8-be2f-ba42081a246e","AutoCancelDurationInMinutes":5,"sendurl":"https://www.plutuscloudserviceuat.in:8201/API/CloudBasedIntegration/V1/UploadBilledTransaction","statusurl":"https://www.plutuscloudserviceuat.in:8201/API/CloudBasedIntegration/V1/GetCloudBasedTxnStatus","cancelurl":"https://www.plutuscloudserviceuat.in:8201/API/CloudBasedIntegration/V1/CancelTransaction"}' },
            { 'id': 4, 'name': 'paytm', 'default': '{"paytmMid":"Mooyss23950688435066","paytmTid":"70001521","sendurl":"https://securegw-stage.paytm.in/ecr/payment/request","statusurl": "https://securegw-stage.paytm.in/ecr/payment/status", "cancelurl": "https://securegw-stage.paytm.in/ecr/void"}' }
        ];

        $scope.POSMultiplePrice = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Restricted' },
            { 'id': 2, 'name': 'Open' },
        ];

        $scope.ApplicationMode = [
            { 'id': 0, 'name': 'Apparel' },
            { 'id': 1, 'name': 'Manufacturing' },
            { 'id': 2, 'name': 'Apparel B2B' },
            { 'id': 3, 'name': 'Mobile Showrooms' },
            { 'id': 4, 'name': 'Franchise' },
            { 'id': 5, 'name': 'Hypermarket' }
        ];

        $scope.InwardMode = [
            { 'id': 0, 'name': 'Auto' },
            { 'id': 1, 'name': 'Manual' }]

        $scope.SpecialBarcodeModes = [
            { 'id': 1, 'name': '4[PCODE]7[BARCODE]2[SIZE]5[PRICE]' }
        ];

        $scope.IncentiveSaleModes = [
            { 'id': 0, 'name': 'None' }, { 'id': 1, 'name': 'Target Based' },
            { 'id': 2, 'name': 'Distribution' }, { 'id': 3, 'name': 'Section Level' }];

        $scope.POSScreenTemplates = [
            { 'id': 1, 'name': 'Barcode' },
            { 'id': 2, 'name': 'Design/SizeList' },
            { 'id': 3, 'name': 'Barcode/Design & Size' },
            { 'id': 4, 'name': 'UAN/Design & Size' },
            { 'id': 9, 'name': 'Scanning' }];

        $scope.MulticompanyMode = [
            { 'id': 0, 'name': 'None' }, { 'id': 1, 'name': 'Products' },
            { 'id': 2, 'name': 'Location' }, { 'id': 3, 'name': 'Barcode' }];

        $scope.RewardModes = [{ 'id': 0, 'name': 'Conversion' }, { 'id': 1, 'name': 'Manual' },
        { 'id': 2, 'name': 'Slab Based - Total Sales' }, { 'id': 3, 'name': 'Slab Based - Bill Value' }];

        $scope.PrintMode = [{ 'id': 0, 'name': 'None' }, { 'id': 1, 'name': 'Manual' },
        { 'id': 2, 'name': 'Current' }];

        $scope.BillCopyMode = [{ 'id': 0, 'name': 'None' }, { 'id': 1, 'name': 'File' },
        { 'id': 2, 'name': 'TCP/IP' }, { 'id': 3, 'name': 'Web' }];

        $scope.RoundingMode = [{ 'id': 0, 'name': 'None' }, { 'id': 1, 'name': 'Rounding-5' }, { 'id': 2, 'name': 'Down-5' },
        { 'id': 3, 'name': 'Up-5' }, { 'id': 4, 'name': 'Down-10' }, { 'id': 5, 'name': 'Up-10' },
        { 'id': 6, 'name': 'Rounding-10' },
        { 'id': 7, 'name': 'Down-1' }, { 'id': 8, 'name': 'Up-1' }, { 'id': 9, 'name': 'Rounding-1' },
        { 'id': 10, 'name': 'No Rounding' }, { 'id': 12, 'name': 'Rounding-25' },
        { 'id': 99, 'name': '2 Digit' }, { 'id': 11, 'name': 'Custom' }];

        $scope.LeaveEncashMode = [{ 'id': 0, 'name': 'Gross Salary' }, { 'id': 1, 'name': 'Basic Pay' }];
        $scope.LeaveEncashType = [{ 'id': 0, 'name': 'Fixed Days' }, { 'id': 1, 'name': 'Sundays' }, { 'id': 2, 'name': 'Sundays+Holidays' }, { 'id': 3, 'name': 'Auto' }, { 'id': 4, 'name': 'None' }];

        $scope.WorkdaysMode = [{ 'id': 0, 'name': 'Except Sunday' }, { 'id': 1, 'name': 'Include Sunday' }, { 'id': 2, 'name': 'Fixed 30Days' }, { 'id': 3, 'name': 'All Days' }];
        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'id': 0, 'name': 'Global' });
        });
        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.AreaList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.AreaList = results;
        });
        $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
            $scope.CustomerCategory = results;
        });
        $scope.SeasonList = restService.Get('ReferencelistService/SEASONS', true).then(function (results) {
            $scope.SeasonList = results;
        });
        $scope.SaleReason = restService.Get('ReferencelistService/BULKSALEREASON', true).then(function (results) {
            $scope.SaleReason = results;
        });
        $scope.Designations = restService.Get('ReferencelistService/DESIGNATION', true).then(function (results) {
            $scope.Designations = results;
        });
        restService.Get('ReferencelistService/FULL-TDSLEDGERS', true).then(function (results) {
            $scope.TDSLedgers = results;
        });
        restService.Get('ReferencelistService/TCSLEDGERS', true).then(function (results) {
            $scope.TCSLedgers = results;
        });

        $scope.BankList = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.BankList = results;
        });
        $scope.refapplist = [{ 'code': 'APPLICATIONSETTINGS', 'value': 'Application Settings' }];
        $scope.key = "APPLICATIONSETTINGS"

        $scope.Entity = {
            "id": 0,
            "userid": 0,
            "key": "APPLICATIONSETTINGS",
            "reportformat": { format: "" },
            "organizationid": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": new Date(),
            "name": "",
            "nooflines": 0,
            "isdefault": false,
            "locationid": null,
            "appsettings": {
                "offersale": true, "season": null, "area": null, "salereason": null, "lockdate": "", intray_supplier: "",
                intray_product: "", intray_agent: "", intray_inward: "", intray_task: "", salesman_insentive: "",
                "sale_negativestock": false, 'settlement_reward_slabs': null
            }

        }

        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.New = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.ApplicationSettings = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ApplicationSettings = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ApplicationSettings.key = 'APPLICATIONSETTINGS';


        $scope.SearchCondition = {
            key: "APPLICATIONSETTINGS",
            locationid: ""
        }

        $scope.applicationmodechanged = function () {
            $scope.ApplicationSettings.appsettings.system_isb2bapplication = $scope.ApplicationSettings.appsettings.appmode == 2;
            $scope.ApplicationSettings.appsettings.system_ismfgapplication = $scope.ApplicationSettings.appsettings.appmode == 1;
            $scope.ApplicationSettings.appsettings.system_ismobileapplication = $scope.ApplicationSettings.appsettings.appmode == 3;
            $scope.ApplicationSettings.appsettings.system_isfranchiseapplication = $scope.ApplicationSettings.appsettings.appmode == 4;
            $scope.ApplicationSettings.appsettings.system_ishypermarketapplication = $scope.ApplicationSettings.appsettings.appmode == 5;

        }

        $scope.Search = function () {
            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $scope.SearchResult = result;
                }
                else {
                    $scope.ApplicationSettings = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.ApplicationSettings.key = $scope.SearchCondition.key;
                }
            }).error(function (result) {
            });

        }

        $scope.SearchDirect = function (locationid) {

            return;
            request = {
                "Item": {
                    'locationid': locationid,
                    'organizationid': $scope.OrganizationSelected && $scope.UserInfo.RootUser ? $scope.CurrentOrganization.id : 0,
                    'key': $scope.SearchCondition.key
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.ApplicationSettings = result[0];
                }
                else {
                    $rootScope.Toaster.Info("Application Settings Not found!");
                }
            }).error(function (result) {
            });

        }

        $scope.ManageClaimSlabs = function (action, item) {
            if (typeof ($scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs) == 'undefined' ||
                $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs == null) {
                $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs = [];
            }
            if (action == 'add') {
                $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs.push({
                    'from': parseFloat($scope.ApplicationSettings.appsettings.reward_slab_amount),
                    'upto': 0,
                    'percentage': parseFloat($scope.ApplicationSettings.appsettings.reward_slab_percent)
                });
            }
            else {
                $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs.splice($scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs.indexOf(item), 1);
            }

            $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs.sort(function (a, b) {
                return a.from - b.from;
            });
            for (var i = 0; i < $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs.length; i++) {
                if (i + 1 < $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs.length) {
                    $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs[i].upto = $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs[i + 1].from;
                }
                else {
                    $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs[i].upto = 10000000;
                }
                if (i > 0 && $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs[i].from != $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs[i - 1].upto)
                    $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs[i].from = $scope.ApplicationSettings.appsettings.settlement_rewardclaim_slabs[i - 1].upto;
            }
        }

        $scope.ManageSlabs = function (action, item) {
            if (typeof ($scope.ApplicationSettings.appsettings.settlement_reward_slabs) == 'undefined' ||
                $scope.ApplicationSettings.appsettings.settlement_reward_slabs == null) {
                $scope.ApplicationSettings.appsettings.settlement_reward_slabs = [];
            }
            if (action == 'add') {
                $scope.ApplicationSettings.appsettings.settlement_reward_slabs.push({
                    'from': parseFloat($scope.ApplicationSettings.appsettings.reward_slab_amount),
                    'upto': 0,
                    'percentage': parseFloat($scope.ApplicationSettings.appsettings.reward_slab_percent)
                });
            }
            else {
                $scope.ApplicationSettings.appsettings.settlement_reward_slabs.splice($scope.ApplicationSettings.appsettings.settlement_reward_slabs.indexOf(item), 1);
            }

            $scope.ApplicationSettings.appsettings.settlement_reward_slabs.sort(function (a, b) {
                return a.from - b.from;
            });
            for (var i = 0; i < $scope.ApplicationSettings.appsettings.settlement_reward_slabs.length; i++) {
                if (i + 1 < $scope.ApplicationSettings.appsettings.settlement_reward_slabs.length) {
                    $scope.ApplicationSettings.appsettings.settlement_reward_slabs[i].upto = $scope.ApplicationSettings.appsettings.settlement_reward_slabs[i + 1].from;
                }
                else {
                    $scope.ApplicationSettings.appsettings.settlement_reward_slabs[i].upto = 10000000;
                }
                if (i > 0 && $scope.ApplicationSettings.appsettings.settlement_reward_slabs[i].from != $scope.ApplicationSettings.appsettings.settlement_reward_slabs[i - 1].upto)
                    $scope.ApplicationSettings.appsettings.settlement_reward_slabs[i].from = $scope.ApplicationSettings.appsettings.settlement_reward_slabs[i - 1].upto;
            }
        }

        $scope.SaveWithConfirmation = function () {

            $.confirm({
                title: 'Confirmation',
                content: 'Do you want to Freeze the sale date on ' + $scope.ApplicationSettings.appsettings.system_saledatefreezeon + " ?",
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
                                "Item":
                                    $scope.ApplicationSettings,
                                key: $scope.key,
                                "securitytoken": "String content"
                            }

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "ConfigurationService/Save",
                                data: srequest
                            }).success(function (result) {

                                if (result != null && result.id > 0) {
                                    $rootScope.Toaster.Info("Successfully saved");
                                    //$scope.SearchHistory(result);
                                }
                                else {
                                    $rootScope.Toaster.Error(result.ErrorMessage);
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


        $scope.Import = function (data) {
            $scope.ApplicationSettings.appsettings = (JSON.parse(data));
            $scope.$apply();
        }

        $scope.ShowDefaultQrConfg = function (id) {
            var find = $scope.QRIntegrationModes.find(function (item) {
                return item.id == id;
            });
            if (find && find != null) {
                $scope.ApplicationSettings.appsettings.qr_payconfig = find.default;
                $rootScope.Toaster.Error("Payment QR integration configuration changed");
            }
        }

        $scope.ShowDefaultIntegrationConfg = function (id) {
            var find = $scope.IntegrationModes.find(function (item) {
                return item.id == id;
            });
            if (find && find != null) {
                $scope.ApplicationSettings.appsettings.int_payconfig = find.default;
                $rootScope.Toaster.Error("Payment integration configuration changed");
            }
        }

        $scope.UnicommerceConfig = function () {
            if ($scope.ApplicationSettings.appsettings.unicommerce == undefined || $scope.ApplicationSettings.appsettings.unicommerce == null)
                $scope.ApplicationSettings.appsettings.unicommerce = {};

            if ($scope.ApplicationSettings.appsettings.unicommerce.channels == undefined ||
                $scope.ApplicationSettings.appsettings.unicommerce.channels == null)
                $scope.ApplicationSettings.appsettings.unicommerce.channels = [];

            var dialog = ngDialog.open({
                template: 'UnicommerceConfiguration',
                data: {
                    'scope': $scope,
                    'unicommerce': $scope.ApplicationSettings.appsettings.unicommerce
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.RetialLocations = $scope.ngDialogData.scope.Location;
                    $scope.BuyingCompany = $scope.ngDialogData.scope.Company;
                    $scope.unicommerce = $scope.ngDialogData.unicommerce;
                    $scope.SearchRef = $scope.ngDialogData.scope.SearchRef;
                    $scope.SyncStock = function (locationid) {
                        request = {
                            "Item": { 'locationid': locationid },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "EcombillService/SyncStock",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info(result);
                        }).error(function (result) {
                            $rootScope.Toaster.Error(result.ErrorMessage);
                        });
                    }

                    $scope.Addchannel = function () {
                        $scope.unicommerce.channels.push({
                            'name': $scope.unicommerceitem.name,
                            'customercode': $scope.unicommerceitem.customercode,
                            'locationid': $scope.unicommerceitem.locationid,
                            'companyid': $scope.unicommerceitem.companyid
                        });
                    }
                    $scope.Delete = function (item) {
                        $scope.unicommerce.channels.splice($scope.unicommerce.channels.indexOf(item), 1);
                    };
                    $scope.Selected = function () {
                        $scope.closeThisDialog($scope.unicommerce);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.ApplicationSettings.appsettings.unicommerce = data.value;
                    }
                }
            });
        }


        $scope.Export = function () {
            var blob = new Blob([JSON.stringify($scope.ApplicationSettings.appsettings)],
                { type: "text/plain;charset=utf-8" });
            saveAs(blob, "ApplicationSetting.Config");
        }

        $scope.Save = function () {

            $scope.ApplicationSettings.organizationid = $scope.OrganizationSelected && $scope.UserInfo.RootUser ? $scope.CurrentOrganization.id : 0;

            if ($scope.ApplicationSettings.appsettings.system_saledatefreeze != 'undefined' &&
                $scope.ApplicationSettings.appsettings.system_saledatefreeze != null &&
                $scope.ApplicationSettings.appsettings.system_saledatefreeze) {

                if ($scope.ApplicationSettings.appsettings.system_saledatefreezeon == 'undefined' ||
                    $scope.ApplicationSettings.appsettings.system_saledatefreezeon == null) {
                    $rootScope.Toaster.Error("Invalid Freeze date.")
                    return;
                }
                $scope.SaveWithConfirmation();
                return;
            }

            request = {
                "Item":
                    $scope.ApplicationSettings,
                key: $scope.key,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ConfigurationService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.ApplicationSettings.id = result.id;
                    $scope.ApplicationSettings.version = result.version;
                    $scope.ApplicationSettings.createdon = result.createdon;
                    $scope.ApplicationSettings.modifiedon = result.modifiedon;
                    $scope.ApplicationSettings.createdby = result.createdby;
                    $scope.ApplicationSettings.modifiedby = result.modifiedby;
                    //$scope.SearchHistory(result);
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
                $rootScope.Toaster.Error(result.ErrorMessage);
            });
        }


        $scope.ManageSearchItem = function (action, item) {
            item.organizationid = $scope.OrganizationSelected && $scope.UserInfo.RootUser ? $scope.CurrentOrganization.id : 0;
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete Report ' + item.name + " ?",
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
                                    url: "ConfigurationService/Delete",
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
                $scope.ShowSearch = true;
                $scope.ApplicationSettings = item;

                if ($scope.ApplicationSettings.appsettings.system_isb2bapplication)
                    $scope.ApplicationSettings.appsettings.appmode = 2;
                else if ($scope.ApplicationSettings.appsettings.system_ismfgapplication)
                    $scope.ApplicationSettings.appsettings.appmode = 1;
                else if ($scope.ApplicationSettings.appsettings.system_ismobileapplication)
                    $scope.ApplicationSettings.appsettings.appmode = 3;
                else if ($scope.ApplicationSettings.appsettings.system_isfranchiseapplication)
                    $scope.ApplicationSettings.appsettings.appmode == 4;
                else if ($scope.ApplicationSettings.appsettings.system_ishypermarketapplication)
                    $scope.ApplicationSettings.appsettings.appmode == 5;
                else
                    $scope.ApplicationSettings.appsettings.appmode == 0;

                $scope.Navigation.SetMode('EDIT');
            }
        }
        $scope.Navigation.SetMode('SEARCH');
    }]);