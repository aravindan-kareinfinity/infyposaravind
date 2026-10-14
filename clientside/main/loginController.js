'use strict';
app.controller('loginController', ['$scope', '$location', 'authService', 'ngAuthSettings',
    '$http', 'localStorageService', '$timeout',
    function ($scope, $location, authService, ngAuthSettings, $http, localStorageService, $timeout) {

        $scope.loginData = {
            username: "",
            password: "",
            useRefreshTokens: true
        };


        //$scope.loginData = { orgName: 'ELAYA', username: 'elayaraja@gmail.com', password: 'abc123', 'useRefreshTokens': true };
        $scope.license = "";
        $scope.message = "";
        $scope.location = null;
        $scope.locationexasted = false;
        $scope.loginpage = false;
        $scope.ShowReset = function () {
            $scope.loginpage = !$scope.loginpage;
            //$scope.loginData = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $http.get('http://localhost:8099/TextilePOS/License').then(function (result) {
            $scope.location = result.data
        }, function (error) {
            $http.get('http://localhost:8100/TextilePOS/License').then(function (result) {
                $scope.location = result.data
            }, function (error) {
                $scope.locationexasted = true;
                var localscope = $scope;
                var WebPrint = new WebPrintService('License', function (data) {
                    localscope.location = data;
                });
                WebPrint.Initialize('localhost', 8099, 8100);
            });
        });


        $scope.getUserIP = function (onNewIP) { //  onNewIp - your listener function for new IPs
            //compatibility for firefox and chrome
            var myPeerConnection = window.RTCPeerConnection || window.mozRTCPeerConnection || window.webkitRTCPeerConnection;
            var pc = new myPeerConnection({
                iceServers: []
            }),
                noop = function () { },
                localIPs = {},
                ipRegex = /([0-9]{1,3}(\.[0-9]{1,3}){3}|[a-f0-9]{1,4}(:[a-f0-9]{1,4}){7})/g,
                key;

            function iterateIP(ip) {
                if (!localIPs[ip]) onNewIP(ip);
                localIPs[ip] = true;
            }

            //create a bogus data channel
            pc.createDataChannel("");

            // create offer and set local description
            pc.createOffer().then(function (sdp) {
                sdp.sdp.split('\n').forEach(function (line) {
                    if (line.indexOf('candidate') < 0) return;
                    line.match(ipRegex).forEach(iterateIP);
                });

                pc.setLocalDescription(sdp, noop, noop);
            }).catch(function (reason) {
                // An error occurred, so handle the failure to connect
            });

            //listen for candidate events
            pc.onicecandidate = function (ice) {
                if (!ice || !ice.candidate || !ice.candidate.candidate || !ice.candidate.candidate.match(ipRegex)) return;
                ice.candidate.candidate.match(ipRegex).forEach(iterateIP);
            };
        }

        $scope.getUserIP(function (ip) {
            $scope.localip = ip;
        });

        $scope.getOrganization = false;
        $http.get('CompanyService/License').then(function (result) {
            $scope.license = result.data.key;
            $scope.getOrganization = result.data.IsSAAS;
            localStorageService.set('licenseData', result.data);
        }, function (error) {
            $scope.license = error;
        });


        $.get('https://jsonip.com?d=' + new Date().getTime()).then(function (result) {
            $scope.ipaddress = result.ip;
        }, function (error) {
            $.get('https://api.ipify.org?format=json&?d=' + new Date().getTime()).then(function (result) {
                $scope.ipaddress = result.ip;
            }, function (error) {
                $.get('http://icanhazip.com?d=' + new Date().getTime()).then(function (result) {
                    $scope.ipaddress = result;
                });
            });
        });

        $scope.login = function () {
            if (typeof ($scope.loginData.username) == 'undefined' || $scope.loginData.username == '') {
                $scope.message = 'Please enter the user name.';
                return;
            }

            if (typeof ($scope.loginData.password) == 'undefined' || $scope.loginData.password == '') {
                $scope.message = 'Please enter the password.';
                return;
            }

            $scope.loginData.deviceid = '';
            if ($scope.location && $scope.location.ipaddress) {
                $scope.loginData.deviceid = $scope.location.ipaddress.replace('.','-');
            }

            $scope.message = "Connecting...";
            authService.login($scope.loginData).then(function (response) {
                if ($scope.getOrganization) {
                    $http.get('CompanyService/License/' + $scope.loginData.orgName).then(function (result) {
                        localStorageService.set('licenseData', result.data);
                        $scope.LoadPermission();
                    }, function (error) {
                        $scope.license = error;
                    });
                }
                else {
                    $scope.LoadPermission();
                }
            },
                function (err) {
                    $scope.message = err.error_description;
                });
        };

        $scope.refresh = function () {
            var authData = localStorageService.get('authorizationData');
            if (authData) {
                var data = "grant_type=refresh_token&refresh_token=" + authData.refreshToken+"&client_id=InfyPOS&org=undefined&client_secret=KareInfinity";
                $http.post(serviceBase + 'token', data,
                    { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
                        alert('success ' + response);
                    }).error(function (err, status) {
                        alert('failed '+err);
                    });
            }
            else {
                alert('Last Authentication information not available in the local storegage');
                if ($scope.loginData.username && $scope.loginData.username != null && $scope.loginData.username != '') {
                    var data = "grant_type=refresh_token&refresh_token=" + $scope.loginData.username + "&client_id=InfyPOS&org=undefined&client_secret=KareInfinity";
                    $http.post(serviceBase + 'token', data,
                        { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
                            alert('success ' + response);
                        }).error(function (err, status) {
                            alert('failed ' + err);
                        });
                }
            }
        }

        $scope.LoadPermission = function () {

            var authData = localStorageService.get('authorizationData');
            var request = {
                "item": {
                    'id': authData.FullData.userId,
                    'includesetting': true,
                    'locationinfo': $scope.location,
                    'ipaddress': $scope.ipaddress,
                    'ValidateSecurity': true
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "UsersService/Search",
                data: srequest
            }).success(function (result) {

                if (typeof (result.Success) != 'undefined' && !result.Success) {
                    $scope.message = result.ErrorMessage;
                    return;
                }
                if (result.length > 0) {
                    if (result.length == 1) {
                        var authData = localStorageService.get('authorizationData');
                        localStorageService.set('Permissions-' + authData.FullData.userId, result[0].permissions);
                    }

                    if ($scope.location &&
                        $scope.location != null &&
                        result[0].systemrole &&
                        result[0].systemrole != null &&
                        result[0].systemrole != '') {
                        var authData = localStorageService.get('authorizationData');
                        authData.systemrole = result[0].systemrole;
                        localStorageService.set('authorizationData', authData);
                    }

                    var fullhref = $(location).attr('href');
                    var rloc = fullhref.indexOf('redir=');
                    if (rloc > 0)
                        $(location).attr('href', decodeURI(fullhref.substr(rloc + 6)));
                    else
                        $(location).attr('href', 'index.html?v=7');
                }

            }).error(function (result) {
                $scope.message = result;
            });
        }

        $scope.changepassword = function () {
            var request = {
                "item": $scope.loginData,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "UsersService/ChangePassword",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.ShowReset();
                    $scope.loginData.password = $scope.loginData.newpassword;
                    $scope.loginData.newpassword = '';
                    $scope.loginData.confirmpassword = '';
                }
                else {
                    $scope.message = 'usernae or password missmatch';
                }
            }).error(function (result) {
            });

        };

        $scope.authExternalProvider = function (provider) {

            var redirectUri = location.protocol + '//' + location.host + '/authcomplete.html';

            var externalProviderUrl = ngAuthSettings.apiServiceBaseUri + "AgentService/ExternalLogin?provider=" + provider
                + "&response_type=token&client_id=" + ngAuthSettings.clientId
                + "&redirect_uri=" + redirectUri;
            window.$windowScope = $scope;

            var oauthWindow = window.open(externalProviderUrl, "Authenticate Account", "location=0,status=0,width=600,height=750");
        };

        $scope.authCompletedCB = function (fragment) {

            $scope.$apply(function () {

                if (fragment.haslocalaccount == 'False') {

                    authService.logOut();

                    authService.externalAuthData = {
                        provider: fragment.provider,
                        username: fragment.external_user_name,
                        externalAccessToken: fragment.external_access_token
                    };

                    $location.path('/associate');

                }
                else {
                    //Obtain access token and redirect to orders
                    var externalData = { provider: fragment.provider, externalAccessToken: fragment.external_access_token };
                    authService.obtainAccessToken(externalData).then(function (response) {

                        $location.path('/orders');

                    },
                        function (err) {
                            $scope.message = err.error_description;
                        });
                }

            });
        }


        $timeout(function () {
            if ($scope.getOrganization) {
                $scope.setFocusorganization();
            }
            else {
                $scope.setFocususername();
            }
        }, 50)

        $scope.OnEnterKey = function (source) {
            if (source == 'organization')
                $scope.setFocususername();
            else if (source == 'username')
                $scope.setFocuspassword();
            else
                $scope.login();
        }
    }]);
