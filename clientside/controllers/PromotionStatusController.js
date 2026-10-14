
var app = angular.module('InfinityLab');

app.controller('PromotionStatusController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.PromotionList = [];
        $scope.ConditionModes = [{ 'id': 1, 'name': 'Product Attribute' }, { 'id': 2, 'name': 'Invoice Item' }, { 'id': 3, 'name': 'Barcode' }, { 'id': 4, 'name': 'Marked Item' },
            { 'id': 5, 'name': 'Excluded Barcode' }, { 'id': 6, 'name': 'Excluded Marker' }, { 'id': 7, 'name': 'Excluded Discount' },
            { 'id': 8, 'name': 'Barcode File' }, { 'id': 9, 'name': 'Excluded Product Attribute' }, { 'id': 10, 'name': 'Bill/Bill Item Value' },
            { 'id': 11, 'name': 'Agreement Code' }, { 'id': 12, 'name': 'Barcode Prefix' }];

        //$scope.Products = restService.Get('ReferencelistService/PROMOTIONS', true).then(function (results) {
        //    $scope.Products = results;
        //});

        $scope.SearchPromotions = function () {

            request = {
                "Item": {
                    "barcode": $scope.barcode,
                    "locationid": $rootScope.LocationInfo.location.id
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PromotionsService/SearchPromotion",
                data: srequest
            }).success(function (result) {

                $.each($scope.PromotionList, function (k, v) {
                    v.status = 'No';
                    v.eligiblereason = '';
                    v.rejectedreason = '';
                    v.reason = '';
                    var item = result.find(function (itm) {
                        return itm.id == v.id;
                    });

                    if (typeof (item) != 'undefined' && item != null) {
                        if (item.eligible) {
                            v.status = 'Yes';
                        }
                        else {
                            v.status = 'No';
                        }
                        if (item.mode >= 1)
                            v.reason = $scope.SearchRef(item.mode, $scope.ConditionModes);
                        if (item.exculdemode >= 1)
                            v.reason = $scope.SearchRef(item.exculdemode, $scope.ConditionModes);
                    }
                });
                $scope.PromotionsConfig($scope.PromotionList, '_id');
            }).error(function (result) {
            });
        };

        $scope.ChangePromotion = function (mode, promotion) {
            var request = {
                "Item": {
                    isadd: mode == 'Add', "barcode": $scope.barcode,
                    promotion: promotion, option: 1
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PromotionsService/UpdateRule",
                data: srequest
            }).success(function (result) {

                if (result != 'undefined' && result != null && result == true) {
                    $rootScope.Toaster.Info("Saved Successfully.!");
                    $scope.SearchPromotions();
                }

            }).error(function (result) {
            });
        };

        $scope.ShowChoice = function (mode, promotion) {
            var dialog = ngDialog.open({
                template: 'ChooseOption',
                data: {
                    'mode': mode,
                    'promotion': promotion,
                    'barcode': $scope.barcode,
                    'locationid': $rootScope.LocationInfo.location.id
                },
                width: '600px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.mode = $scope.ngDialogData.mode
                    $scope.barcode = $scope.ngDialogData.barcode;
                    $scope.promotion = $scope.ngDialogData.promotion;
                    $scope.locationid = $scope.ngDialogData.locationid;
                    $scope.BarcodeList = [];
                    $scope.OptionList = [{ 'id': 1, 'name': 'Barcode' }, { 'id': 2, 'name': 'Invoice Items' }];
                    $scope.SelectAllChanged = function (items) {
                        $.each(items, function (k, v) {
                            v.selected = $scope.selectall;
                        });
                    };

                    $scope.LoadItems = function () {
                        request = {
                            "Item": {
                                "barcode": $scope.barcode,
                                "locationid": $rootScope.LocationInfo.location.id,
                                'option': $scope.option,
                                'isadd': $scope.mode == 'Add'
                            },
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "PromotionsService/SearchItems",
                            data: srequest
                        }).success(function (result) {
                            $scope.BarcodeList = result;
                        }).error(function (result) {
                        });
                    }

                    $scope.Selected = function () {
                        var blist = [];
                        $.each($scope.BarcodeList, function (k, v) {
                            if (v.selected) {
                                blist.push(v);
                            }
                        });
                        $scope.mode = $scope.ngDialogData.mode
                        $scope.barcode = $scope.ngDialogData.barcode;
                        $scope.promotion = $scope.ngDialogData.promotion;
                        $scope.locationid = $scope.ngDialogData.locationid;

                        var request = { 'items': blist, 'isadd': $scope.mode, 'barcode': $scope.barcode, 'promotion': $scope.promotion, 'locationid': $scope.locationid, 'option': $scope.option };
                        if (blist.length > 0)
                            $scope.closeThisDialog(request);
                        else
                            $scope.closeThisDialog();
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": data.value,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "PromotionsService/UpdateRule",
                            data: srequest
                        }).success(function (result) {

                            if (result != null &&
                                result.length > 0) {
                                $rootScope.Toaster.Info("Saved Successfully.!");
                            }

                        }).error(function (result) {
                        });

                    }
                }
            });
        }

        $scope.LoadAvailablePromotions = function () {
            request = {
                "Item": {
                    "name": null,
                    "validfrom": null,
                    "validto": null,
                    "mode": 0,
                    "locationid": $rootScope.LocationInfo.location.id
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PromotionsService/Search",
                data: srequest
            }).success(function (result) {
                $scope.PromotionList = result;
                $scope.PromotionsConfig($scope.PromotionList, '_id');
            }).error(function (result) {
            });
        }

        $scope.LoadAvailablePromotions();

        $scope.PromotionsConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('name', 'Name', 500);
            igrid.AddFormatedColumn('validfrom', 'Valid From', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('validto', 'Valid Upto', 100, 'dd-MM-yyyy');
            igrid.AddCustomColumn('enabled', 'Status', 60, function (item, col) {
                return item.enabled == true ? "Active" : "Deactive";
            });
            igrid.AddCustomColumn('status', 'In Scheme', 60, function (item, col) {
                return item.status == 'Yes' ? "Yes" : "No";
            });
            igrid.AddSimpleColumn('reason', 'Reason', 200);

            var commands = ""
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Add'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Remove'/>"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Add')
                        $scope.ManageSearchItem('Add', args.item);
                    else if ($(e.target).val() == 'Remove')
                        $scope.ManageSearchItem('Remove', args.item);
                });
            });
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.PromotionList);
        }
        $scope.ManageSearchItem = function (action, item) {
            if (action == "Add") {
                if (item.status == 'Yes') {
                    $rootScope.Toaster.Info("Already Added.");
                    return;
                }
                else if (item.status == 'No') {
                    $scope.ChangePromotion('Add', item);
                }
            }
            else if (action == "Remove") {
                if (item.status == 'Yes') {
                    $scope.ChangePromotion('Remove', item);
                }
                else (item.status == 'No')
                {
                    $rootScope.Toaster.Info("Already Removed.");
                    return;
                }
            }
        }

        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);
