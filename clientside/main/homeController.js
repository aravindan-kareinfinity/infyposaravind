'use strict';
app.controller('homeController', ['$scope', '$location', 'authService', 'ngAuthSettings', 'localStorageService', 'ngDialog', '$http', '$rootScope', 'UserDetails', '$parse', 'restService', '$timeout', '$filter',
    function ($scope, $location, authService, ngAuthSettings, localStorageService, ngDialog, $http, $rootScope, UserDetails, $parse, restService, $timeout, $filter) {

        $scope.HasProductGroups = false;
        $scope.ProductGroups = [];
        $scope.IntrayMsgCount = 0;
        $scope.CompanyList = [];
        $scope.LocationList = [];
        $scope.CounterList = [];
        restService.Get('ReferencelistService/PRODUCTGROUP', true).then(function (results) {
            $scope.ProductGroups = results;
            $scope.HasProductGroups = results.length > 0;
        });

        restService.Get('ReferencelistService/FULLCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        });

        restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });

        restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.CounterList = results
        });

        $scope.ConnectURL = null;
        $scope.ShowConnectURL = false;
        var authData = localStorageService.get('authorizationData');
        $rootScope.License = $scope.License = localStorageService.get('licenseData');

        $rootScope.IsStandardVersion = false;
        $rootScope.IsPOSVersion = false;
        if ($rootScope.License && $rootScope.License != null) {
            $rootScope.IsPOSVersion = $rootScope.License.mode == 0;
            $rootScope.IsStandardVersion = $rootScope.License.mode == 1;
        }

        $scope.sizechanged = function () {
            //alert('sizechanged');
            //alert('' + $rootScope.windowSize.height + ' X ' + $rootScope.windowSize.width);
            //$rootScope.$emit('windowSizeChanged', $rootScope.windowSize);
        }

        $scope.AppInfo = {
            CompanyTitle: "Textile POS",
            CompanyShortName: "T"
        };

        $scope.UserInfo = {
            id: authData.FullData.userId,
            userName: authData.FullData.userName,
            Title: authData.FullData.Title,
            Designation: authData.FullData.Designation,
            CompanyName: authData.FullData.CompanyName,
            Landing: authData.FullData.LandingScreen,
            Passcode: authData.FullData.Passcode,
            EmployeeId: authData.FullData.employeeId,
            LoggedOn: authData.FullData['.issued'],
            RootUser: authData.FullData.RootUser == 'True',
            useRefreshTokens: true,
            OrganizationID: authData.FullData.OrganizationID,
            AD: authData.FullData.AD
        };

        $scope.MyPasscode = function () {
            restService.Get('UsersService/MyPasscode', true).then(function (results) {
                $scope.UserInfo.Passcode = results;
            });
        }


        if ($location.path() == '' || $location.path() == '/home') {
            $timeout(function () {

                if (authData.FullData.KU) {
                    var killedUserCount = parseInt(authData.FullData.KU);
                    if (killedUserCount > 0) {
                        $rootScope.Toaster.Error(killedUserCount + ' Users killed.');
                        authData.FullData.KU = '0';
                        localStorageService.set('authorizationData', authData);
                    }
                }

                switch ($scope.UserInfo.Landing) {
                    case 'LR Inbox':
                        $location.path('/LRInbox');
                        break;
                    case 'Dashboard':
                        $location.path('/home');
                        break;
                    case 'Sales':
                        $location.path('/SalesOffer');
                        break;
                    case 'Settlement':
                        $location.path('/Settlements');
                        break;
                    case 'Employee In-Out':
                        $location.path('/empinout');
                        break;
                    case 'HR Dashboard':
                        $location.path('/HRDashboard');
                        break;
                    case 'Intray':
                        $location.path('/Intray');
                        break;
                    case 'Notification':
                        $location.path('/Notifications');
                        break;
                    case 'Feedback':
                        const baseUrl = window.location.origin + window.location.pathname.replace(/index\.html.*/i, '');
                        const newUrl = baseUrl + 'FB/Q/0?REPEAT=TRUE';
                        window.location.href = newUrl; // Or use window.open(newUrl, '_blank') for new tab
                        break;
                    case 'Catalog':
                        const basecatUrl = window.location.origin + window.location.pathname.replace(/index\.html.*/i, '');
                        const newcatUrl = basecatUrl + 'Catalog.html?TZ=' + new Date().getTime();
                        window.location.href = newcatUrl; // Or use window.open(newUrl, '_blank') for new tab
                        break;
                }

            }, 100);
        }

        UserDetails.Info = $scope.UserInfo;
        $rootScope.UserInfo = $scope.UserInfo;
        $rootScope.UserInfo.appsettings = null;
        $rootScope.ShowDeliveryChallan = false;
        $rootScope.ShowEstimateBill = false;
        $scope.AppMode = 'APPAREL';

        $scope.LoadApplicationConfig = function () {
            var srequest = JSON.stringify({
                "Item": { 'locationid': -9, 'key': 'APPLICATIONSETTINGS' },
                "securitytoken": "String content"
            });

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $rootScope.UserInfo.appsettings = result[0].appsettings;
                    switch (result[0].appsettings.appmode) {
                        case 1:
                            $scope.AppMode = 'MFG';
                            break;
                        case 2:
                            $scope.AppMode = 'B2B';
                            break;
                        case 3:
                            $scope.AppMode = 'MOB';
                            break;
                        case 4:
                            $scope.AppMode = 'FRN';
                            break;
                        case 5:
                            $scope.AppMode = 'HYP';
                            break;
                    }
                    $rootScope.ShowDeliveryChallan = $rootScope.UserInfo.appsettings.system_supportdc;
                    $rootScope.ShowEstimateBill = $rootScope.UserInfo.appsettings.system_supportestimate;
                    $rootScope.ShowProductDesign = $rootScope.UserInfo.appsettings.system_disabledesigneditable;
                    $rootScope.ShowDeliveryRack = $rootScope.UserInfo.appsettings.system_supportdeliveryrack;
                    $rootScope.ShowEcommerce = $rootScope.UserInfo.appsettings.unicommerce;
                    
                    if ($rootScope.UserInfo.appsettings.system_webprint) {
                        $rootScope.WebPrint.Initialize('localhost', 8099, 8103);
                    }
                }
                else {
                    $rootScope.UserInfo.appsettings = {};
                }

                $rootScope.$on('cfpLoadingBar:loading', function (event, data) {
                    data.s = 'Loading';
                    //console.log(data); // 'Data to send'
                });

                $rootScope.$on('cfpLoadingBar:loaded', function (event, data) {
                    data.s = 'Loaded';
                    //console.log(data); // 'Data to send'
                });

            });
        }();

        $scope.RunningRequest = [];
        $scope.RunningRequestCount = 0;
        $scope.RequestStatus = function (data, status) {
            if (status == 'S') {
                $scope.RunningRequest.push({ 'url': data.url, 'started': new Date() });
                $scope.RunningRequestCount++;
            }
            else {
                var item = $scope.RunningRequest.find(function (item) {
                    return item.url == data.url;
                });
                if (typeof (item) != 'undefined' && item != null) {
                    $scope.RunningRequest.splice($scope.RunningRequest.indexOf(item), 1);
                }
                $scope.RunningRequestCount--;
            }
        }

        $scope.ShowRunningRequest = function () {
            if ($scope.RunningRequest.length > 0) {
                var list = [];
                var current = new Date();
                $.each($scope.RunningRequest, function (k, v) {
                    var seconds = (current.getTime() - v.started.getTime()) / 1000;
                    list.push(v.url + ' ' + seconds + 's');
                });
                $rootScope.Toaster.Error(list.join('<br>'));
            }
            else {
                $rootScope.Toaster.Info('No Request!');
            }
        }

        $scope.OpenConnector = function () {

        }

        $scope.ShowConnector = function () {
            var authData = localStorageService.get('authorizationData');
            if (authData) {
                $scope.ConnectURL = 'File/MobileLogin/' + $rootScope.UserInfo.OrganizationID + '/' + authData.refreshToken;
                $scope.ShowConnectURL = true;

                var request = {
                    'item': {
                        'refreshToken': authData.refreshToken, 'accessToken': authData.token
                    }
                };
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: 'UsersessionService/SetMobileToken',
                    data: srequest
                }).success(function (result) {
                });

            }
        }

        var slideToggeler = function (element, subtitles) {
            this.element = element;
            this.subtitles = subtitles;
            this.Toggle = function (val) {
                if (val) {
                    this.element.addClass('sidebar-collapse');
                    this.subtitles.removeClass('subtitle_hide');
                }
                else {
                    this.element.removeClass('sidebar-collapse');
                    this.subtitles.addClass('subtitle_hide');
                }
            }
        };

        $('.sidebar-toggle').on('click', function (evt) {
            if ($('body').hasClass('sidebar-collapse')) {
                $('.subtitle').removeClass('subtitle_hide');
                $('.main-container').addClass('side_open_width');
                $('.main-container').removeClass('side_close_width');
                $('.main-footer').addClass('side_open_width');
                $('.main-footer').removeClass('side_close_width');
            }
            else {

                $('.main-footer').removeClass('side_open_width');
                $('.main-footer').addClass('side_close_width');
                $('.main-container').removeClass('side_open_width');
                $('.main-container').addClass('side_close_width');
                $('.subtitle').addClass('subtitle_hide');
            }
        });

        //side_close_width
        $rootScope.slideToggeler = new slideToggeler($('body'), $('.subtitle'));
        $rootScope.slideToggeler.Toggle(true);

        $scope.ProfileClick = function () {
            //$location.path('/profile');
        };



        $scope.SelectLocation = function (key, postexecution) {
            if (typeof (key) == 'undefined')
                key = '';

            if ($scope.CompanyList == null ||
                $scope.CompanyList.length == 0 ||
                $scope.LocationList == null ||
                $scope.LocationList.length == 0 ||
                $scope.CounterList == null ||
                $scope.CounterList.length == 0) {

                $timeout(function () {
                    $scope.SelectLocation(key, postexecution);
                }, 50);

                return;
            }

            var dialog = ngDialog.open({
                template: 'LocationList',
                data: { 'scope': $scope, 'reportkey': key },
                width: '600px',
                controller: ['$scope', '$timeout', 'restService', 'localStorageService', '$http', '$rootScope',
                    function ($scope, $timeout, restService, localStorageService, $http, $rootScope) {

                        $scope.CompanyList = $scope.ngDialogData.scope.CompanyList;
                        $scope.LocationList = $scope.ngDialogData.scope.LocationList;
                        $scope.CounterList = $scope.ngDialogData.scope.CounterList;
                        $scope.FilterdCounterList = [];
                        $scope.AutonumberList = [];
                        $scope.locationinfo = "-None-";
                        $scope.selectedformat = null;
                        $scope.selectedcompany = null;
                        $scope.selectedlocation = null;
                        $scope.selectedcounter = null;
                        var printerData = localStorageService.get('LocationData');

                        $scope.FilterCounter = function () {
                            var FilterdCounterList = [];
                            var location = $scope.LocationList.find(function (item) {
                                return item.id == $scope.selectedlocation;
                            });
                            if (typeof (location) != 'undefined') {
                                $.each($scope.CounterList, function (k, v) {
                                    if (v.locationid == 0 || v.locationid == location.id)
                                        FilterdCounterList.push(v);
                                });
                            }
                            $scope.FilterdCounterList = FilterdCounterList;
                        }

                        if (printerData != null) {
                            $scope.locationinfo = printerData.locationinfo;
                            if (printerData.counter && printerData.company != null)
                                $scope.selectedcompany = printerData.company.id;
                            if (printerData.counter && printerData.location != null)
                                $scope.selectedlocation = printerData.location.id;
                            if (printerData.counter && printerData.counter != null)
                                $scope.selectedcounter = printerData.counter.id
                            $scope.FilterCounter();
                        }


                        $scope.ChangePaymentPort = function () {
                            $rootScope.Prompt('Payment Integration ', 'Port No',
                                function (value) {
                                    if (value && value != null && value != '')
                                        localStorageService.set('LocalData.int_payport', value);
                                }, localStorageService.get('LocalData.int_payport'));
                        }

                        $scope.ChangeDate = function () {
                            $.confirm({
                                title: 'SAVE',
                                content: 'Do you want to change the current billing date? The change will be applied immediately and cannot be revert it back!!',
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
                                            $scope.ChangeDateOk();
                                        }
                                    },
                                    cancel: function () {
                                        //close
                                    },
                                },

                                onContentReady: function () {
                                    // bind to events
                                    var jc = this;
                                    this.$content.find('form').on('submit', function (e) {
                                        // if the user submits the form by pressing enter in the field.
                                        e.preventDefault();
                                        jc.$$formSubmit.trigger('click'); // reference the button and click it
                                    });
                                }
                            });
                        }
                        $scope.ChangeDateOk = function () {
                            var locationid = $scope.selectedlocation;
                            if (locationid == 'undefined' || locationid == null || locationid == 0) {
                                locationid = $rootScope.LocationInfo.location.id;
                            }
                            var request = {
                                'item': {
                                    'locationid': locationid, 'nextdaycount': 1
                                }
                            };
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: 'ConfigurationService/ChangeDate',
                                data: srequest
                            }).success(function (result) {
                                $scope.closeThisDialog();
                                if (!result.Success)
                                    $rootScope.ShowAlert('Change Date', result.ErrorMessage);
                                else {
                                    $rootScope.Toaster.Info('Date Changed Successfully. ' + $filter('date')(result.nextday, 'dd-MM-yyyy'));
                                }
                            });

                            /*$rootScope.Prompt('Change Next Day', '', function (value) {

                                var request = {
                                    'item': {
                                        'locationid': $scope.selectedlocation, 'nextdaycount': value
                                    }
                                };
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: 'ConfigurationService/ChangeDate',
                                    data: srequest
                                }).success(function (result) {

                                });
                            },'1');*/

                        }

                        $scope.Current = function () {
                            printerData = localStorageService.get('LocationData');
                            if (typeof (printerData) == 'undefined' || printerData == null)
                                return;
                            $scope.closeThisDialog(printerData);
                        };



                        restService.Get('ReferencelistService/AUTONUMBER', true).then(function (results) {
                            $scope.AutonumberList = results;
                        });

                        $scope.Selected = function () {
                            var company = $scope.CompanyList.find(function (item) {
                                return item.id == $scope.selectedcompany;
                            });
                            var location = $scope.LocationList.find(function (item) {
                                return item.id == $scope.selectedlocation;
                            });
                            var counter = $scope.CounterList.find(function (item) {
                                return item.id == $scope.selectedcounter;
                            });

                            var billconfig = $scope.AutonumberList.find(function (item) {
                                return item.locationid == location.id && item.type == 'bill.billno';
                            });

                            if (typeof (billconfig) != 'undefined' && billconfig.prefix != null && billconfig.prefix.indexOf('{') > 0)
                                billconfig.prefix = billconfig.prefix.substr(0, billconfig.prefix.indexOf('{'));

                            var billreturnconfig = $scope.AutonumberList.find(function (item) {
                                return item.locationid == location.id && item.type == 'billreturn.billno';
                            });

                            if (typeof (billreturnconfig) == 'undefined' || billreturnconfig == null) {
                                billreturnconfig = $scope.AutonumberList.find(function (item) {
                                    return item.type == 'billreturn.billno' && item.companyid == company.id;
                                });
                            }

                            if (typeof (billreturnconfig) != 'undefined' && billreturnconfig.prefix != null && billreturnconfig.prefix.indexOf('{') > 0)
                                billreturnconfig.prefix = billreturnconfig.prefix.substr(0, billreturnconfig.prefix.indexOf('{'));

                            var bulkbillconfig = $scope.AutonumberList.find(function (item) {
                                return item.locationid == location.id && (item.type == 'bulkbill.billno' || item.type == 'bulkbill.ebillno');
                            });

                            if (typeof (bulkbillconfig) != 'undefined' && bulkbillconfig.prefix != null && bulkbillconfig.prefix.indexOf('{') > 0)
                                bulkbillconfig.prefix = bulkbillconfig.prefix.substr(0, bulkbillconfig.prefix.indexOf('{'));


                            var billmasterconfig = $scope.AutonumberList.find(function (item) {
                                return item.locationid == location.id && item.type == 'billmaster.code';
                            });

                            if (typeof (billmasterconfig) != 'undefined' && billmasterconfig.prefix != null && billmasterconfig.prefix.indexOf('{') > 0)
                                billmasterconfig.prefix = billmasterconfig.prefix.substr(0, billmasterconfig.prefix.indexOf('{'));

                            if (typeof (bulkbillconfig) == 'undefined' || bulkbillconfig == null)
                                bulkbillconfig = {};

                            if (typeof (billconfig) == 'undefined' || billconfig == null)
                                billconfig = {};

                            if (typeof (billreturnconfig) == 'undefined' || billreturnconfig == null)
                                billreturnconfig = {};

                            if (typeof (billmasterconfig) == 'undefined' || billmasterconfig == null)
                                billmasterconfig = {};

                            bulkbillconfig.prefixlist = [];
                            billconfig.prefixlist = [];
                            billmasterconfig.prefixlist = [];
                            billreturnconfig.prefixlist = [];

                            $.each($scope.AutonumberList, function (k, v) {
                                if (v.locationid == location.id && (v.type == 'bill.billno') && v.prefix != null) {
                                    if (v.prefix.indexOf('{') > 0)
                                        billconfig.prefixlist.push(v.prefix.substr(0, v.prefix.indexOf('{')))
                                    else if (v.prefix != null && v.prefix != '')
                                        billconfig.prefixlist.push(v.prefix);
                                }
                                else if ((v.locationid == location.id || (v.locationid == 0 && v.companyid == company.id)) && (v.type == 'billreturn.billno') && v.prefix != null) {
                                    if (v.prefix.indexOf('{') > 0)
                                        billreturnconfig.prefixlist.push(v.prefix.substr(0, v.prefix.indexOf('{')))
                                    else if (v.prefix != null && v.prefix != '')
                                        billreturnconfig.prefixlist.push(v.prefix);
                                }
                                else if (v.locationid == location.id && (v.type == 'bulkbill.billno' || v.type == 'bulkbill.ebillno') && v.prefix != null) {
                                    if (v.prefix.indexOf('{') > 0)
                                        bulkbillconfig.prefixlist.push(v.prefix.substr(0, v.prefix.indexOf('{')))
                                    else if (v.prefix != null && v.prefix != '')
                                        bulkbillconfig.prefixlist.push(v.prefix);
                                } else if (v.locationid == location.id && v.type == 'billmaster.code' && v.prefix != null) {
                                    if (v.prefix.indexOf('{') > 0)
                                        billmasterconfig.prefixlist.push(v.prefix.substr(0, v.prefix.indexOf('{')))
                                    else if (v.prefix != null && v.prefix != '')
                                        billmasterconfig.prefixlist.push(v.prefix);
                                }
                            });

                            if (company != null &&
                                location != null) {
                                var locationinfo = company.name + '/' + location.name;
                                if (counter != null) {
                                    var locationinfo = company.name + '/' + location.name + '-' + counter.name;
                                }

                                var printerData = {
                                    'location': location,
                                    'company': company,
                                    'locationinfo': locationinfo,
                                    'counter': counter,
                                    'floorid': counter != null ? counter.floorid : 0,
                                    'billconfig': billconfig,
                                    'billreturnconfig': billreturnconfig,
                                    'bulkbillconfig': bulkbillconfig,
                                    'billmasterconfig': billmasterconfig
                                };

                                localStorageService.set('LocationData', printerData);
                                $scope.closeThisDialog(printerData);
                            }
                        }

                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (data.value != null) {
                        $rootScope.LocationInfo = data.value;
                    }
                    if (typeof (postexecution) != 'undefined') {
                        postexecution(data.value);
                    }
                }
            });

        }

        $scope.SelectPrinter = function (key, postexecution) {

            var locationdata = localStorageService.get('LocationData');
            if (locationdata == undefined || locationdata == null) {
                $rootScope.Toaster.Error('Please select the location before selecting printer');
                $scope.SelectLocation('', function () {
                    locationdata = localStorageService.get('LocationData');
                    if (locationdata && locationdata != null) {
                        $scope.SelectPrinter(key);
                    }
                })
                return;
            }


            if (typeof (key) == 'undefined')
                key = '';
            var dialog = ngDialog.open({
                template: 'PrinterList',
                data: {
                    'scope': $scope, 'reportkey': key,
                    'saveprinter': typeof (postexecution) == 'undefined' || postexecution == null
                },
                width: '600px',
                controller: ['$scope', '$timeout', 'restService', 'localStorageService', '$rootScope',
                    function ($scope, $timeout, restService, localStorageService, $rootScope) {
                        var request = {
                            "item": {},
                            "securitytoken": "String content"
                        }
                        $scope.ShowPDF = false;
                        $scope.ListOfPrinters = [];
                        $scope.currentlocation = null;
                        var printerData = localStorageService.get('LocationData');
                        if (printerData != null) {
                            $scope.currentlocation = printerData.location;
                        }
                        var printerData = localStorageService.get('PrinterData');
                        var srequest = JSON.stringify(request);
                        $scope.selectedserver = null;
                        $scope.printerinfo = "-None-";
                        $scope.selectedformat = null;
                        $scope.saveprinter = $scope.ngDialogData.saveprinter;
                        $scope.PaperSizeList = [
                            { 'id': 0, 'name': 'Default' },
                            { 'id': 4, 'name': '4 Inches' },
                            { 'id': 3, 'name': '3 Inches' },
                            { 'id': 9, 'name': 'Laser Print' }];

                        $scope.selectedpapersize = 0;


                        $scope.ReportSelected = function () {
                            var report = $scope.ReportList.find(function (item) { return item.id == $scope.selectedformat; });
                            $scope.ShowPDF = typeof (report) != 'undefined' && report.ispdf;
                        }

                        $scope.Current = function () {
                            printerData = localStorageService.get('PrinterData');
                            if (typeof (printerData) == 'undefined' || printerData == null)
                                return;

                            if ($scope.ShowReportList) {
                                var report = null;
                                report = $scope.ReportList.find(function (item) {
                                    return item.id == $scope.selectedformat;
                                });

                                printerData.report = report;
                                if ($scope.saveprinter) {
                                    localStorageService.set('PrinterData', printerData);
                                }
                                $scope.closeThisDialog(printerData);
                            }
                            else {
                                $scope.closeThisDialog(printerData);
                            }
                        };


                        $scope.ShowReportList = false;
                        $scope.ShowPaperSizeList = typeof ($rootScope.UserInfo.appsettings.system_supportpapersize) != 'undefined' && $rootScope.UserInfo.appsettings.system_supportpapersize
                        if ($scope.ngDialogData.reportkey != '') {
                            if ($scope.ngDialogData.reportkey == 'THERMAL') {
                                $scope.ShowPaperSizeList = true;
                            }
                            else {
                                $scope.ShowPaperSizeList = false;
                                $scope.ReportList = restService.Get('ConfigurationService/ReportList?id=' + $scope.ngDialogData.reportkey, true).then(function (results) {
                                    $scope.ReportList = results;
                                    $scope.ShowReportList = true;
                                });
                            }
                        }

                        $scope.ChangeCompany = function (callback) {
                            var company = $scope.CompanyList.find(function (item) {
                                return item.id == $scope.selectedcompany;
                            })

                            if (company != 'undefined' && company != null) {
                                if ($scope.currentlocation != null) {
                                    var servers = [];
                                    $.each(company.servers, function (k, v) {
                                        if (v.location == null || v.location == '' || v.location == $scope.currentlocation.name || v.location == $scope.currentlocation.code) {
                                            servers.push(v);
                                        }
                                    });
                                    $scope.ServerList = servers;
                                }
                                else {
                                    $scope.ServerList = company.servers;
                                }
                            }

                            if (typeof (QuantoNativeInterface) != 'undefined') {
                                if (typeof ($scope.ServerList) == 'undefined' || $scope.ServerList == null)
                                    $scope.ServerList = [];

                                var mobilePrinter = $scope.ServerList.find(function (item) {
                                    {
                                        return item.type == 'Mobile'
                                    }
                                });

                                if (typeof (mobilePrinter) == 'undefined' || mobilePrinter == null) {
                                    $scope.ServerList.push({ 'id': -9, 'type': 'Mobile', 'name': 'Mobile' });
                                }
                            }

                            if (printerData && printerData != null) {
                                $scope.selectedserver = printerData.server.id;
                                $scope.GetPrinters(callback);
                            }
                            else {
                                if (callback) callback();
                            }
                        };

                        $scope.GetPrinters = function (callback) {

                            if (typeof ($scope.ServerList) == 'undefined' || $scope.ServerList == null)
                                return;
                            var server = $scope.ServerList.find(function (item) {
                                return item.id == $scope.selectedserver;
                            });

                            if (typeof (server) == 'undefined' || server == null) return;

                            if (server.id == -9 && server.type == 'Mobile') {
                                var printerList = JSON.parse(QuantoNativeInterface.GetListOfPrinters());
                                var results = [];
                                $.each(printerList, function (k, v) {
                                    results.push({ 'isandroid': true, 'id': v.id, 'name': v.display, 'type': v.type });
                                });
                                $scope.ListOfPrinters = results;
                                if (printerData != null) {
                                    $scope.selectedprinter = printerData.printer.id;
                                    if (callback) callback();
                                }
                            }
                            else if (server.id == -10 && server.type == 'Web Printer') {
                                $scope.ListOfPrinters = $rootScope.WebPrint.ListOfPrinters;
                                if (callback) callback();
                            }
                            else if (server.type == 'WebPrinter') {
                                $rootScope.ExternalWebPrint = new WebPrintService('External', function (response) {
                                    $timeout(function () {
                                        $scope.ListOfPrinters = response.data;
                                        if (callback) callback();
                                    }, 10);
                                });
                                $rootScope.ExternalWebPrint.Initialize(server.name, parseInt(server.port),
                                    parseInt(server.port));
                                setTimeout($rootScope.ExternalWebPrint.ListOfPrinters(callback), 10);
                            }
                            else {
                                var service = (server.type == 'SSLPrinter' ? 'https://' : 'http://') + server.name + ':' + server.port + '/TextilePOS/ListOfPrinters?d=' + new Date().getTime();
                                $.get(service).then(function (results) {
                                    $scope.ListOfPrinters = results;
                                    if (callback) callback();

                                    var selectedprinter = results.find(function (item) {
                                        item.id == $scope.selectedprinter
                                    });
                                    if (selectedprinter && selectedprinter != null)
                                        $scope.selectedprinter = selectedprinter.id;
                                    else
                                        $scope.selectedprinter = null;

                                    $timeout(function () {
                                        $scope.setFocusselectedprinter();
                                    }, 50);
                                }, function (error) {
                                    //alert(error);
                                });

                                //$scope.ListOfPrinters = restService.Get(service, true).then(function (results) {
                                //    $scope.ListOfPrinters = results;

                                //    if (printerData != null) {
                                //        $scope.selectedprinter = printerData.printer.id;
                                //    }
                                //});
                            }
                        };

                        $scope.Test = function () {
                            $rootScope.Prompt('Test Print', 'Type here', function (value) {

                                try {
                                    var printdocument = window.btoa(value);

                                    var server = $scope.ServerList.find(function (item) {
                                        return item.id == $scope.selectedserver;
                                    });
                                    var printer = $scope.ListOfPrinters.find(function (item) {
                                        return item.id == $scope.selectedprinter;
                                    });

                                    if (typeof (server) == 'undefined' || typeof (printer) == 'undefined' || server == null || printer == null) {
                                        return;
                                    }

                                    var service = 'http://' + server.name + ':' + server.port + '/TextilePOS/Print';
                                    var request = {
                                        'id': printer.id,
                                        'content': printdocument,
                                        'encoded': 'BASE64'
                                    };
                                    if ($scope.selectedserver == -10) {
                                        $rootScope.WebPrint.Print(request, function () { });
                                    }
                                    else {
                                        var srequest = JSON.stringify(request);
                                        $http({
                                            method: 'POST',
                                            url: service,
                                            data: srequest
                                        }).success(function (result) {
                                            $scope.closeThisDialog();
                                        });
                                    }
                                } catch (e) {

                                }

                            }, 'Content goes here..', 15);
                        }

                        $scope.PrinterSelected = function () {

                        }

                        $scope.CompanyList = restService.Get('CompanyService/ListPrinter', true).then(function (results) {
                            $scope.CompanyList = results;

                            if (printerData != null) {
                                $scope.currentlocation = printerData.location;
                                $scope.printerinfo = printerData.printerinfo;
                                $scope.selectedpapersize = printerData.PaperSize;
                                $timeout(function () {
                                    $scope.selectedcompany = printerData.company.id;
                                    $scope.ChangeCompany(function () {
                                        $timeout(function () {
                                            $scope.selectedserver = printerData.server.id;
                                            $scope.selectedprinter = printerData.printer.id;
                                        }, 100);
                                    })
                                }, 100);
                                //$scope.ServerList = [printerData.server];
                                //$scope.ListOfPrinters = [printerData.printer];
                            }
                            //$scope.ChangeCompany();
                        });


                        $scope.Selected = function (pdf) {
                            var company = $scope.CompanyList.find(function (item) {
                                return item.id == $scope.selectedcompany;
                            });
                            var server = $scope.ServerList.find(function (item) {
                                return item.id == $scope.selectedserver;
                            });
                            var printer = $scope.ListOfPrinters.find(function (item) {
                                return item.id == $scope.selectedprinter;
                            });
                            var report = null;
                            if ($scope.ShowReportList) {
                                report = $scope.ReportList.find(function (item) {
                                    return item.id == $scope.selectedformat;
                                });
                            }

                            var defaulttemplateid = 0;
                            var isdirectdos = false;
                            var locationdata = localStorageService.get('LocationData');
                            if (typeof (locationdata) != 'undefined' &&
                                typeof (locationdata.counter) != 'undefined' &&
                                locationdata.counter != null &&
                                locationdata.counter.dotmatrixprint)
                                isdirectdos = true;

                            if (typeof (locationdata) != 'undefined' &&
                                typeof (locationdata.counter) != 'undefined' &&
                                locationdata.counter != null &&
                                locationdata.counter.productgroup &&
                                locationdata.counter.productgroup > 0)
                                defaulttemplateid = locationdata.counter.productgroup;

                            if (company != null &&
                                server != null &&
                                printer != null) {
                                var service = 'http://' + server.name + ':' + server.port + '/TextilePOS/Print';
                                var printerinfo = server.name + ':' + server.port + '@' + printer.name + ' from ' + company.name;
                                if ($scope.ShowReportList) {
                                    if (report != null && report != 'null' && typeof (report) != 'undefined')
                                        printerinfo += '(' + report.name + ')';
                                    else
                                        return;
                                }

                                var printerData = {
                                    'server': server,
                                    'printer': printer,
                                    'company': company,
                                    'printerid': printer.id,
                                    'service': service,
                                    'report': report,
                                    'IsPDF': $scope.ShowPDF,
                                    'IsDirectDos': isdirectdos,
                                    'PaperSize': $scope.selectedpapersize,
                                    'IsDesign': pdf,
                                    'TemplateID': defaulttemplateid,
                                    'IsWebPrint': $scope.selectedserver == -10,
                                    'isandroid': typeof (printer.isandroid) != 'undefined' || printer.isandroid,
                                    'printertype': typeof (printer.type) != 'undefined' ? printer.type : '',
                                    'printerinfo': printerinfo
                                };

                                if ($scope.saveprinter)
                                    localStorageService.set('PrinterData', printerData);

                                $scope.closeThisDialog(printerData);
                            }
                        }

                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (data.value != null) {
                        $rootScope.PrinterInfo = data.value;
                        if (typeof (postexecution) != 'undefined') {
                            postexecution(data.value);
                        }
                    }

                }
            });
        };

        $rootScope.Handlers.SelectPrinter = $scope.SelectPrinter;
        $rootScope.Handlers.SelectLocation = $scope.SelectLocation;

        $scope.SignOut = function () {
            authService.logOut(function () {
                $(location).attr('href', 'login.html');
            });

            //if ($(location).attr('href').indexOf('redir=') < 0)
            //    $(location).attr('href', 'login.html?redir=' + encodeURI($(location).attr('href')));
        };


        $scope.LoadPermission = function () {

            var permissions = localStorageService.get('Permissions-' + $scope.UserInfo.id);
            if (typeof (permissions) != 'undefined' && permissions != null && typeof (permissions.length) != 'undefined') {
                $scope.UserInfo.permisisons = permissions;
                $.each(permissions, function (k, v) {
                    $parse('UserInfo.p_' + v.code).assign($scope, v);
                });
                UserDetails.Info = $scope.UserInfo;
            }
            else {

                var request = {
                    "item": { 'id': $scope.UserInfo.id },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "UsersService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length == 1) {
                        $scope.UserInfo.permisisons = result[0].permissions;
                        $.each(result[0].permissions, function (k, v) {
                            $parse('UserInfo.p_' + v.code).assign($scope, v);
                        });
                        UserDetails.Info = $scope.UserInfo;
                    }
                }).error(function (result) {
                });
            }
        }

        $scope.F2Listener = function (event) {
            if (event.key == "F2")
                $rootScope.$broadcast("F2Pressed");
        }

        $scope.LoadPermission();

        $scope.IntraySearch = function () {
            $scope.IntrayMsgCount = 0;
            var request = {
                "Item": { 'statusid': 1 },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IntrayService/Search",
                data: srequest
            }).success(function (result) {
                $scope.IntrayMsgCount = (result == null) ? 0 : result.length;
            }).error(function (result) {
            });
        }

        $scope.Initialize = function () {

            var printerData = localStorageService.get('LocationData');
            if (printerData != null) {
                $rootScope.LocationInfo = printerData;
            }

            var printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                $rootScope.PrinterInfo = printerData;
            }

            $rootScope.Triggers = {
                'SelectLocation': $scope.SelectLocation, 'SelectPrinter': $scope.SelectPrinter, 'SignOut': $scope.SignOut
            };

            var parameters = decodeURI(window.location.search)
                .replace('?', '')
                .split('&')
                .map(param => param.split('='))
                .reduce((values, [key, value]) => {
                    values[key] = value
                    return values
                }, {});

            if (parameters.kiosk &&
                parameters.kiosk != null &&
                parameters.kiosk == 'true') {
                $scope.IsKiosk = true;
                $rootScope.IsKiosk = true;
            }
        };

        $scope.Initialize();
        $scope.IntraySearch();

        $scope.submenu = {};
        $scope.ShowSubmenu = function () {
            $timeout(function () {
                $('#submenu')[0].click();
            }, 50);
        }


        $rootScope.LicenseMessage = function () {
            var dialog = ngDialog.open({
                template: 'LicenseExpiry',
                data: [$scope,
                    $rootScope.LicenseMessageDays,
                    $rootScope.LicenseExpired],
                width: '400px',
                closeByEscape: false,
                closeByDocument: false,
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    if ($scope.days > 10)
                        $scope.days = 10;
                    $scope.days = $scope.ngDialogData[1];
                    $scope.expired = $scope.ngDialogData[2];
                    $scope.timepending = Math.round($scope.days * 20);
                    $scope.Execute = function () {
                        $scope.timepending--;
                        if ($scope.timepending > 0) {
                            $timeout(function () {
                                $scope.Execute();
                            }, 100)
                        }
                        else {
                            $scope.closeThisDialog();
                        }
                    }
                    $timeout(function () {
                        $scope.Execute();
                    }, 100)
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                }
            });
        }

        $rootScope.ShowLicenseMessage = false;
        //$rootScope.License.valiate_upto = true;
        if ($rootScope.License &&
            $rootScope.License.valiate_upto) {
            var validupto = new Date($rootScope.License.validupto);
            if (validupto.getTime() > new Date().getTime()) {
                var Difference_In_Time = validupto.getTime() - new Date().getTime();
                var Difference_In_Days = Difference_In_Time / (1000 * 3600 * 24);
                $rootScope.LicenseMessageDays = Math.round(Difference_In_Days);
                if ($rootScope.LicenseMessageDays >= 5) {
                    $rootScope.ShowLicenseMessage = true;
                    $rootScope.LicenseExpired = false;
                    $rootScope.LicenseMessage();
                }
            }
            else {
                $rootScope.LicenseMessageDays = 5;
                $rootScope.LicenseExpired = true;
                $rootScope.ShowLicenseMessage = true;
                $rootScope.LicenseMessage();
            }
        }

        $timeout(function () {
            var locationInfo = localStorageService.get('LocationData');
            if (locationInfo == undefined || locationInfo == null)
                $rootScope.LocationInfo = null;
            else
                $rootScope.LocationInfo = locationInfo;
            var printerData = localStorageService.get('PrinterData');
            if (printerData == undefined || printerData == null)
                $rootScope.PrinterInfo = null;
            else
                $rootScope.PrinterInfo = printerData;
        }, 50)


        $('.sidebar-toggle').click(function () {
            if ($('.main-sidebar').hasClass('sidebar-expand')) {
                $('.main-sidebar').removeClass('sidebar-expand');
            }
            else {
                $('.main-sidebar').addClass('sidebar-expand');
            }
        });
    }]);
