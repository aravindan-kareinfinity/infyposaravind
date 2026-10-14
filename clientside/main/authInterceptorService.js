'use strict';
app.factory('authInterceptorService', ['$q', '$injector', '$location', 'localStorageService', '$rootScope',
    function ($q, $injector, $location, localStorageService, $rootScope) {

        var authInterceptorServiceFactory = {};
        var $http;
        var nooffailurs = 0;
        var _request = function (config) {

            if (typeof (config.headers) != 'undefined' && typeof (config.headers.Accept) != 'undefined' && config.headers.Accept == 'text/html') {
                return config;
            }

            config.headers = config.headers || {};
            var authData = localStorageService.get('authorizationData');
            if (authData) {
                config.headers.Authorization = 'Bearer ' + authData.token;
            }

            return config;
        }

        var _responseError = function (rejection) {
            var deferred = $q.defer();

            if (rejection.status === 401) {
                var authService = $injector.get('authService');
                authService.refreshToken().then(function (response) {
                    _retryHttpRequest(rejection.config, deferred);
                }, function () {
                    if ($(location).attr('href').indexOf('redir=') < 0)
                        $(location).attr('href', 'login.html?redir=' + encodeURI($(location).attr('href')));
                });
            } else if (rejection.status === 417) {
                if (typeof (rejection.data) == "string") 
                    $rootScope.Toaster.Error(rejection.data);
                else if (rejection.data.error && typeof (rejection.data.error.message) == 'string')
                    $rootScope.Toaster.Error(rejection.data.error.message);
                else if (typeof (rejection.data.error.message) == 'object')
                    $rootScope.Toaster.Error(JSON.stringify(rejection.data)); 
                deferred.reject(rejection);
            }
            else if ((rejection.status === 600 || rejection.status === 601) && rejection.data && rejection.data.QueueId) {
                // Handle queue-based operations
                if (rejection.status === 601) {
                    // Wait 2 seconds before starting polling for 601 status
                    setTimeout(function () {
                        pollForQueueResult(rejection.data.QueueId, rejection.config, deferred);
                    }, 2000);
                } else {
                    // Start polling immediately for 600 status
                    pollForQueueResult(rejection.data.QueueId, rejection.config, deferred);
                }
            }
            else if (rejection.status === 500) {
                if (rejection.data.ExceptionMessage && rejection.data.ExceptionMessage!=null)
                    $rootScope.Toaster.Error(rejection.data.ExceptionMessage);
                else
                    $rootScope.Toaster.Error(rejection.data.Message);
                deferred.reject(rejection);
            }
            else {
                deferred.reject(rejection);
            }
            return deferred.promise;
        }

        var _retryHttpRequest = function (config, deferred) {
            $http = $http || $injector.get('$http');
            $http(config).then(function (response) {
                deferred.resolve(response);
            }, function (response) {
                deferred.reject(response);
            });
        }

        var pollForQueueResult = function (queueId, originalConfig, deferred) {
            $http = $http || $injector.get('$http');

            var poll = function () {
                $http.get('File/QueueResultStatus/' + queueId)
                    .then(function (pollResponse) {
                        if (pollResponse.data && pollResponse.data.IsCompleted) {
                            // Operation completed successfully - execute same pipeline
                            var successResponse = {
                                data: pollResponse.data.Result,
                                status: 200,
                                statusText: 'OK',
                                config: originalConfig,
                                headers: pollResponse.headers,
                                url: originalConfig.url,
                                method: originalConfig.method
                            };
                            deferred.resolve(successResponse);
                        } else {
                            // Operation still running, continue polling
                            setTimeout(poll, 5000); // Poll every 2 seconds
                        }
                    }, function (pollError) {
                        $rootScope.Toaster.Error(pollError.data.ExceptionMessage);
                        var errorResponse = {
                            status: pollError.status || 500,
                            statusText: pollError.statusText || 'Polling Error',
                            data: pollError.data || 'Failed to get queue result',
                            config: originalConfig,
                            url: originalConfig.url,
                            method: originalConfig.method
                        };
                        deferred.reject(errorResponse);
                    });
            };

            // Start polling
            poll();
        }

        authInterceptorServiceFactory.request = _request;
        authInterceptorServiceFactory.responseError = _responseError;

        return authInterceptorServiceFactory;
    }]);