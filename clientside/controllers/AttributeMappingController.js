
var app = angular.module('InfinityLab');

app.controller('AttributeMappingController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.favouriteid = 0;
        $scope.SearchCondtion = { 'productid': 0, 'autorefresh': true };
        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
        });

        $scope.Favourits = restService.Get('ReferencelistService/FAV_ATTRIB', true).then(function (results) {
            $scope.Favourits = results;
            results.push({ 'id': 0, 'name': 'New Favourite' });
        });

        $scope.SleeveList = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.SleeveList = results;
        });

        $scope.FitList = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.FitList = results;
        });

        $scope.BrandList = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandList = results;
        });

        $scope.Patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Patterns = results;
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });

        $scope.materials = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.materials = results;
        });

        $scope.Styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Styles = results;
        });

        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.OtherSources = [{ id: 'Fit', name: 'Fit' },
        { id: 'Sleeve', name: 'Sleeve' },
            { id: 'Material', name: 'Material' },
            { id: 'Pattern', name: 'Pattern' },
        { id: 'Type', name: 'Type' }];

        $scope.Entity = {
            "id": 0,
            "pattern": [],
            "size": [],
            "style": [],
            "material": [],
            "colour": [],
            "type": [],
            "brand": [],
            "fit": [],
            "sleeve": []
        };

        $scope.attributelist = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ManageList = function (action, value, name, list) {
            if (action == 'add') {
                var available = list.find(function (item) {
                    return item == value;
                })
                if (typeof (available) == 'undefined' || available == null)
                    list.push(value);
            }
            else if (action == 'delete') {
                list.splice(list.indexOf(value), 1);
            }
        }

        $scope.SaveFavourite = function () {

            $rootScope.Prompt('Favorite', 'Name', function (count) {
                if (count && count != '') {

                    var request = {
                        "item": {
                            'id': $scope.favouriteid, 'type': 'FAV_ATTRIB',
                            'name': count,
                            'data': JSON.stringify($scope.attributelist),
                            'isactive': true
                        },
                        "securitytoken": "String content"
                    };

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "ReferencelistService/Save",
                        data: srequest
                    }).success(function (result) {
                        $scope.Favourits.push(result);
                    });
                }
            }, '');

        }

        $scope.Save = function () {

            var request = {
                "Item": {
                    'id': $scope.SearchCondtion.productid,
                    'attributelist': $scope.attributelist
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ProductsService/Attributelist",
                data: srequest
            }).success(function (result) {
                if (result) {
                    $rootScope.Toaster.Info("Saved Successfully.!");
                }
            }).error(function (result) {
            });

        }

        $scope.Show = function (frombutton) {
            if (!frombutton && !$scope.SearchCondtion.autorefresh) return;

            if (typeof ($scope.SearchCondtion.productid) == 'undefined' || $scope.SearchCondtion.productid == 0
                || $scope.SearchCondtion.productid == null) {
                $scope.attributelist = (JSON.parse(JSON.stringify($scope.Entity)));
            }
            else {
                restService.Get('ProductsService/Attributelist/' + $scope.SearchCondtion.productid, true).then(function (results) {
                    if (results == null)
                        $scope.attributelist = (JSON.parse(JSON.stringify($scope.Entity)));
                    else
                        $scope.attributelist = results;

                    if ($scope.attributelist.brand == null)
                        $scope.attributelist.brand = [];
                    if ($scope.attributelist.fit == null)
                        $scope.attributelist.fit = [];
                    if ($scope.attributelist.sleeve == null)
                        $scope.attributelist.sleeve = [];
                });
            }
        }

    }]);



