'use strict';

app.factory('httpCache', function ($cacheFactory) {
    return $cacheFactory('httpCache');
});

app.service('restService', ['$http', 'ngAuthSettings', 'localStorageService', 'httpCache', '$q',
    function ($http, ngAuthSettings, localStorageService, httpCache,$q) {

    var serviceBase = ngAuthSettings.apiServiceBaseUri;
    var cacheingenabled = false;
    //var coreRestService = {};

    this.Get = function (url,supportCache) {

        if (cacheingenabled && supportCache)
        {
            var cache = httpCache.get(url);
            if (cache) {
                var deferred = $q.defer();
                deferred.resolve(cache);
                return deferred.promise;
            }
        }
        //var authData = localStorageService.get('authorizationData');
        //if (authData) {
        //    config.headers.Authorization = 'Bearer ' + authData.token;
        //}
        return $http.get(serviceBase + url).then(function (results) {
            if (cacheingenabled && supportCache)
                httpCache.put(url, results.data);
            return results.data;
        });
    };

    this.Post = function (url,request) {
       
        //var authData = localStorageService.get('authorizationData');
        //if (authData) {
        //    config.headers.Authorization = 'Bearer ' + authData.token;
        //}

        return $http.post(serviceBase + url, request).then(function (response) {
            return response.data;
        });
    };

    this.SearchReference = function (searchValue, listValues, defaultValue) {

        if (typeof (listValues) == 'undefined' || listValues == null || !angular.isArray(listValues))
            return searchValue;

        if (typeof (searchValue) != 'undefined') {
            var found = listValues.find(function (arritem) {
                return arritem.id == searchValue;
            });
            if (typeof (found) != 'undefined' && found != null)
                return found.name;
        }
        return typeof (defaultValue) == 'undefined' ? "None" : defaultValue;
    };

    this.DefaultReference = function (name) {
        return [{ 'id': -1, 'name': 'Loading...' }];
    };
    //return coreRestService;
}]);








