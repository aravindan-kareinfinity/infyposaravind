'use strict';

app.service('cntxService', ['localStorageService', '$q',
    function (localStorageService, $q) {

        this.data = [];

        function internalContext(pageName) {
            this.PageName = pageName;
            this.data = [];
            this.Remove = function (name) {
                if (typeof (localStorageService.get(pageName + "." + name)) != 'undefined') {
                    localStorageService.remove(pageName + "." + name);
                }
            };
            this.Get = function (name, memory) {
                if (typeof (memory) == 'undefined')
                    memory = true;
                if (memory) {
                    if (typeof (this.data[name]) == 'undefined')
                        return null;
                    return this.data[name];
                }
                else {
                    if (typeof (localStorageService.get(pageName+"."+name)) == 'undefined')
                        return null;
                    return localStorageService.get(pageName + "." + name);
                }
                //var deferred = $q.defer();
                //deferred.resolve(cache);
                //return deferred.promise;
            };

            this.Post = function (name, value, memory) {
                if (typeof (memory) == 'undefined')
                    memory = true;

                if (memory) {
                    this.data[name] = value;
                }
                else {
                    localStorageService.set(pageName + "." + name, value);
                }
                //var authData = localStorageService.get('authorizationData');
                //if (authData) {
                //    config.headers.Authorization = 'Bearer ' + authData.token;
                //}

            };
        }

        this.CreatePageContext = function (name) {
            if (typeof (this.data[name]) == 'undefined')
                this.data[name] = new internalContext(name);

            return this.data[name];
        };

        this.GetGlobalContext = function()
        {
            var name = "_GLOBAL_";
            if (typeof (this.data[name]) == 'undefined')
                this.data[name] = new internalContext(name);

            return this.data[name];
        }

    }]);








