'use strict';
app.factory('authService', ['$http', '$q', 'localStorageService', 'ngAuthSettings',
    function ($http, $q, localStorageService, ngAuthSettings) {

    var serviceBase = '';
    var authServiceFactory = {};

    var _authentication = {
        isAuth: false,
        username: "",
        useRefreshTokens: true
    };

    var _externalAuthData = {
        provider: "",
        username: "",
        externalAccessToken: ""
    };

    var _saveRegistration = function (registration) {

        _logOut();

        return $http.post(serviceBase + 'AccountService/register', registration).then(function (response) {
            return response;
        });

    };

    var _login = function (loginData) {

        var data = "grant_type=password&username=" + loginData.username + "&password=" +
            loginData.password + "&org=" + loginData.orgName + "&client_id=" + ngAuthSettings.clientId
            + "&client_secret=" + ngAuthSettings.secret + '&platform=WEB&deviceid=' + loginData.deviceid + '&TZ=' + new Date().getTimezoneOffset();

        var deferred = $q.defer();

        $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

            if (loginData.useRefreshTokens) {
                localStorageService.set('authorizationData', {
                    token: response.access_token, username: response.username, refreshToken: response.refresh_token,
                    refreshTokenHash: response.refresh_token_hash, useRefreshTokens: true,FullData : response
                });
            }
            else {
                localStorageService.set('authorizationData', {
                    token: response.access_token, username: response.username, refreshToken: "",
                    refreshTokenHash: "", useRefreshTokens: false, FullData: response
                });
            }
            _authentication.isAuth = true;
            _authentication.username = loginData.username;
            _authentication.useRefreshTokens = loginData.useRefreshTokens;

            deferred.resolve(response);

        }).error(function (err, status) {
            //if(status != 400)
            //    _logOut();
            deferred.reject(err);
        });

        return deferred.promise;

    };

    var _logOut = function (callback) {

        var authData = localStorageService.get('authorizationData');
        $http({
            method: 'POST',
            url: "UsersessionService/Logout",
            data: {
                'Item': {
                    'refreshtoken': authData.refreshToken,
                    'refereshtokenhash': authData.refreshTokenHash
                }
            }
        }).success(function (result) {
            localStorageService.remove('authorizationData');
            _authentication.isAuth = false;
            _authentication.username = "";
            _authentication.useRefreshTokens = true;
            callback();
        });
    };

    var _fillAuthData = function () {

        var authData = localStorageService.get('authorizationData');
        if (authData) {
            _authentication.isAuth = true;
            _authentication.username = authData.username;
            _authentication.useRefreshTokens = authData.useRefreshTokens;
            return true;
        }
        return false;
    };

    var _refreshToken = function () {
        //alert('refresh');
        var deferred = $q.defer();
        if (this.refreshTokenList.length != 0) {
            this.refreshTokenList.push(deferred);
            return deferred.promise;
        }
        this.refreshTokenList.push(deferred);
        var refreshtokenlist = this.refreshTokenList;
        var authData = localStorageService.get('authorizationData');
        if (authData) {
            authData.useRefreshTokens = true;
            if (authData.useRefreshTokens) {

                var data = "grant_type=refresh_token&refresh_token=" + authData.refreshToken + "&client_id=" + ngAuthSettings.clientId + "&org=" + authData.FullData.OrganizationCode + "&client_secret=" + ngAuthSettings.secret;

                //localStorageService.remove('authorizationData');

                $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                    try {
                        var authData = {
                            token: response.access_token, username: response.username, refreshToken: response.refresh_token,
                            refreshTokenHash: response.refresh_token_hash, useRefreshTokens: true, FullData: response
                        };

                        localStorageService.set('authorizationData', authData);

                        var newauthdata = localStorageService.get('authorizationData');
                    } catch (exp) {
                        alert(exp);
                    }

                    while (refreshtokenlist.length > 0) {
                        var item = [];
                        $.each(refreshtokenlist, function (k, v) {
                            v.resolve(response);
                            item.push(v);
                        });
                        $.each(item, function (k, v) {
                            refreshtokenlist.splice(refreshtokenlist.indexOf(v), 1);
                        })
                    }
                }).error(function (err, status) {
                    //if(status != 400)
                    //    _logOut();
                    while (refreshtokenlist.length > 0) {
                        var item = [];
                        $.each(refreshtokenlist, function (k, v) {
                            v.reject(err);
                            item.push(v);
                        });
                        $.each(item, function (k, v) {
                            refreshtokenlist.splice(refreshtokenlist.indexOf(v), 1);
                        })
                    }
                });
            }
        }
        else
        {
            deferred.reject();
        }

        return deferred.promise;
    };

    var _obtainAccessToken = function (externalData) {

        var deferred = $q.defer();

        $http.get(serviceBase + 'AccountService/ObtainLocalAccessToken', { params: { provider: externalData.provider, externalAccessToken: externalData.externalAccessToken } }).success(function (response) {

            localStorageService.set('authorizationData', { token: response.access_token, username: response.username, refreshToken: "", useRefreshTokens: false });

            _authentication.isAuth = true;
            _authentication.username = response.username;
            _authentication.useRefreshTokens = false;

            deferred.resolve(response);

        }).error(function (err, status) {
            //_logOut();
            deferred.reject(err);
        });

        return deferred.promise;

    };

    var _registerExternal = function (registerExternalData) {

        var deferred = $q.defer();

        $http.post(serviceBase + 'AccountService/registerexternal', registerExternalData).success(function (response) {

            localStorageService.set('authorizationData', { token: response.access_token, username: response.username, refreshToken: "", useRefreshTokens: false });

            _authentication.isAuth = true;
            _authentication.username = response.username;
            _authentication.useRefreshTokens = false;

            deferred.resolve(response);

        }).error(function (err, status) {
            //_logOut();
            deferred.reject(err);
        });

        return deferred.promise;

    };

    authServiceFactory.saveRegistration = _saveRegistration;
    authServiceFactory.login = _login;
    authServiceFactory.logOut = _logOut;
    authServiceFactory.fillAuthData = _fillAuthData;
    authServiceFactory.authentication = _authentication;
    authServiceFactory.refreshToken = _refreshToken;
    authServiceFactory.refreshTokenList = [];
    authServiceFactory.obtainAccessToken = _obtainAccessToken;
    authServiceFactory.externalAuthData = _externalAuthData;
    authServiceFactory.registerExternal = _registerExternal;

    return authServiceFactory;
}]);