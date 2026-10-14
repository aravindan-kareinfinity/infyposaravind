
var app = angular.module('InfinityLab');

app.controller('OpeningcashController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SettlementPrint = null;
        $scope.SearchRef = restService.SearchReference;

        $scope.Entity = {
            "id": 0,
            "cashierid":0,
            "paidbyemployeeid": "",
            "receivedbyemployeid": "",
            "counterid": "",
            "paidon":new Date(),
            "returnedon": "",
            "counterid": "",
            "locationid":"",
            "openingamount":"",
            "amount":"",
            "openingdinomination": [{ dinomination: 2000, qty: 0, amount: 0 }, { dinomination: 500, qty: 0, amount: 0 }, { dinomination: 200, qty: 0, amount: 0 }, { dinomination: 100, qty: 0, amount: 0 }, { dinomination: 50, qty: 0, amount: 0 },
            { dinomination: 20, qty: 0, amount: 0 }, { dinomination: 10, qty: 0, amount: 0 }, { dinomination: 5, qty: 0, amount: 0 }, { dinomination: 2, qty: 0, amount: 0 }, { dinomination: 1, qty: 0, amount: 0 }],
            "counterid": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "status":1
        };

        $scope.OpeningBalance = (JSON.parse(JSON.stringify($scope.Entity)));
        
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
        });
        $scope.FilterdCounterList = [];
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.OpeningBalance = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.FilterCounter = function () {
            var FilterdCounterList = [];
            var location = $scope.Locations.find(function (item) {
                return item.id == $scope.OpeningBalance.locationid;
            });
            if (typeof (location) != 'undefined') {
                $.each($scope.Counters, function (k, v) {
                    if (v.locationid == 0 || v.locationid == location.id)
                        FilterdCounterList.push(v);
                });
            }
            $scope.FilterdCounterList = FilterdCounterList;
        }


        $scope.SearchCondition = {
            receivedbyemployeid:"",
            paidbyemployeeid: "",
            paidon: new Date()
        }

        $scope.AmountChanged = function (item) {
            var oldValue = item.amount;
            item.amount = isNaN(item.dinomination * item.qty) ? 0 : item.dinomination * item.qty;
            $scope.OpeningBalance.openingamount = $scope.OpeningBalance.openingamount - oldValue + item.amount;
        };

        //$scope.Save = function () {

        //    request = {
        //        "Item": $scope.OpeningBalance,
        //        "securitytoken": "String content"
        //    }

        //    var srequest = JSON.stringify(request);

        //    $http({
        //        method: 'POST',
        //        url: "CashService/Save",
        //        data: srequest
        //    }).success(function (result) {

        //        if (result != null && result.id > 0) {
        //            $rootScope.Toaster.Info("Saved Successfully");
        //            $scope.OpeningBalance = (JSON.parse(JSON.stringify($scope.Entity)));
        //        }
        //        else {
        //            $scope.ErrorMessage = result.ErrorMessage;
        //            $scope.ErrorStyle = "danger";
        //        }

        //    }).error(function (result) {
        //    });

        //}
        $scope.Save = function () {
            if ($scope.OpeningBalance.locationid == null || $scope.OpeningBalance.locationid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please enter Location");
                return;
            }
            if ($scope.OpeningBalance.counterid == null || $scope.OpeningBalance.counterid == 0) {
                $scope['setFocussecond']();
                $rootScope.Toaster.Error("Please enter Counter");
                return;
            }            
            if ($scope.OpeningBalance.paidbyemployeeid == null || $scope.OpeningBalance.paidbyemployeeid == 0) {
                $scope['setFocusthird']();
                $rootScope.Toaster.Error("Please select Paidbyemployee");
                return;
            }
            if ($scope.OpeningBalance.cashierid == null || $scope.OpeningBalance.cashierid == 0) {
                $scope['setFocusfourth']();
                $rootScope.Toaster.Error("Please select Cashier");
                return;
            }

            request = {
                "Item": $scope.OpeningBalance,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CashService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Saved Successfully");
                    $scope.OpeningBalance = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }

        $scope.ShowStatus = function () {
            $scope.SettlementResult = [];
            request = {
                "item": {
                    locationid: $scope.OpeningBalance.locationid,
                    counterid: $scope.OpeningBalance.counterid,
                    employeeid: $scope.OpeningBalance.cashierid,
                    fromdate: $scope.OpeningBalance.paidon,
                    todate: $scope.OpeningBalance.paidon
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchSettlement",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {

                    $scope.ClosingBalance = $scope.OpeningBalance;
                    $scope.SettlementResult = result;
                    var closingamt = ($scope.ClosingBalance.openingamount + $scope.SettlementResult[0].paid_cash + $scope.SettlementResult[0].paid_card);
                    var cashinhand = ($scope.ClosingBalance.openingamount + $scope.SettlementResult[0].paid_cash) - $scope.SettlementResult[0].return_cash;
                    var list = [];

                    list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
                    list.push($rootScope.PrintFormater.Bold("SALES RECONCILATION REPORT"));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  LOCATION : " + $scope.SearchRef($scope.ClosingBalance.locationid, $scope.Locations)));

                    list.push($rootScope.PrintFormater.Normal("  DATE     : " + $filter('date')($scope.ClosingBalance.returnedon, 'dd-MM-yyyy')));

                    list.push($rootScope.PrintFormater.Normal("  COUNTER  : " + $scope.SearchRef($scope.ClosingBalance.counterid, $scope.Counters)));
                    list.push($rootScope.PrintFormater.Normal("  CASHIER  : " + $scope.SettlementResult[0].employeename));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));

                    var tline = [];
                    var mline = [];
                    var bline = [];
                    $.each($scope.ClosingBalance.openingdinomination, function (k, v) {
                        tline.push($rootScope.PrintFormater.Align(v.dinomination, 6, false))
                        bline.push($rootScope.PrintFormater.Align(v.qty, 6, false))
                        mline.push('_____');
                    })
                    list.push($rootScope.PrintFormater.Condensed(tline.join('')));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Condensed(bline.join('')));

                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));

                    list.push($rootScope.PrintFormater.Normal("  Collection Amout        (Z) : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.closingamount, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Opening Balance         (A) : " + $rootScope.PrintFormater.Align($scope.ClosingBalance.openingamount, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Cash Payment            (B) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].paid_cash, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Card Payment            (C) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].paid_card, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Refund Amount           (D) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].return_cash, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Discount Amount         (E) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].discount, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Adjustment Amount       (F) : " + $rootScope.PrintFormater.Align($scope.SettlementResult[0].adjustment_paid, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Closing Balance (G=(A+B+C)) : " + $rootScope.PrintFormater.Align(closingamt, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("  Cash In Hand    (H=(A+B-D)) : " + $rootScope.PrintFormater.Align(cashinhand, 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal("  Diffrence             (Z-H) : " + $rootScope.PrintFormater.Align((parseFloat($scope.ClosingBalance.closingamount) - cashinhand).toString(), 10, true)));
                    list.push($rootScope.PrintFormater.Normal("__________________________________________"));
                    list.push($rootScope.PrintFormater.Normal(""));
                    list.push("\r\n");
                    list.push("\r\n");

                    $scope.SettlementPrint = list.join('\r\n');
                }
            }).error(function (result) {
            });
        }
        $scope.New = function () {
            $scope.OpeningBalance = (JSON.parse(JSON.stringify($scope.Entity)));
        };
        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
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
                                    url: "CashService/Delete",
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
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.OpeningBalance = item;
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CashService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });

        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);