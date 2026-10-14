var app = angular.module('InfinityLab');

app.controller('LoyaltyConfigurationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.IncentiveModes = [{ 'id': 0, 'name': 'Department Level' }, { 'id': 1, 'name': 'Target Based' },
        { 'id': 2, 'name': 'Section Slab Based' }, { 'id': 3, 'name': 'Product Slab Based' }];
        $scope.RewardModes = [{ 'id': 0, 'name': 'Conversion' }, { 'id': 1, 'name': 'Manual' },
        { 'id': 2, 'name': 'Slab Based' }];

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'id': 0, 'name': 'Global' });
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
            "locationid": "",
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
            $scope.ApplicationSettings = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ApplicationSettings = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ApplicationSettings.key = 'APPLICATIONSETTINGS';


        $scope.SearchCondition = {
            key: "APPLICATIONSETTINGS",
            locationid: ""
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
                "Item": { 'locationid': locationid, 'key': $scope.SearchCondition.key },
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

        $scope.Save = function () {

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
                    $scope.SearchHistory(result);
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
                $rootScope.Toaster.Error(result.ErrorMessage);
            });
        }

        $scope.ManageSearchItem = function (action, item) {
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
            }
        }

    }]);