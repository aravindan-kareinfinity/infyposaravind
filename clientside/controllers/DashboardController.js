
var app = angular.module('InfinityLab');

app.controller('DashboardController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {

        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowB2BDashboard = false;
        $scope.totalwalkin = 0;
        $scope.BillType = [{ id: 0, name: 'Cash' }, { id: 1, name: 'Card' }, { id: 3, name: 'Redemption' }, { id: 8, name: 'Return' }, { id: 9, name: 'Cancel' }, { id: 10, name: 'Bulk' }];
        $scope.warehouse = { 'total': 0, 'items': [] };
        $scope.salesmanlabels = [];
        $scope.series = ['Series A'];
        $scope.saleseries = ['Sale Value', 'Bill Count'];
        $scope.salesmandata = [];
        $scope.salesectionlabels = [];
        $scope.salesectiondata = [];

        $scope.SearchResult = [];
        $scope.Sales = [];
        $scope.Settlement = [];
        $scope.Salesman = [];
        $scope.Salesection = [];
        $scope.SalesPerHrs = [];
        $scope.TodayEvents = [];
        $scope.StockValueList = [];
        $scope.totalsettlement = 0;
        $scope.absentemployee = 0;
        $scope.totalemployee = 0;
        $scope.unsettlebillcount = 0;
        $scope.Employees = [];
        $scope.hasLoan = false;
        $scope.totalsalevalue = 0.0;
        $scope.totalsettlement = 0.0;
        $scope.totalbillcount = 0;
        $scope.totalbillvalue = 0.0;
        $scope.stockvalue = 0.0;
        $scope.totalsalevalue = 0.0;

        $scope.datasetOverride = [

            {
                yAxisID: 'y-axis-2',
                label: 'Bill Value',
                borderWidth: 3,
                type: 'line',
                fill: false,
                borderColor: '#7c4dff'
            },
            {
                yAxisID: 'y-axis-1',
                label: 'Bill Count',
                borderWidth: 1,
                type: 'bar',
                backgroundColor: '#ff4081'
            }
        ];


        $scope.options = {
            scales: {
                yAxes: [
                    {
                        id: 'y-axis-1',
                        type: 'linear',
                        display: true,
                        position: 'left',
                        ticks: {
                            // Include a dollar sign in the ticks
                            callback: function (value, index, values) {
                                var divider = 10000000;
                                var dividerLabel = 'Cr';
                                var fixed = 2;
                                if (value > 10000000) {
                                    divider = 10000000;
                                    dividerLabel = 'Cr';
                                } else if (value > 100000) {
                                    divider = 100000;
                                    dividerLabel = 'Lac';
                                } else if (value > 1000) {
                                    divider = 1000;
                                    dividerLabel = 'K';
                                }
                                else {
                                    divider = 1;
                                    dividerLabel = '';
                                    fixed = 0;
                                }
                                return (parseFloat(value) / divider).toFixed(fixed) + dividerLabel;
                            }
                        },
                        scaleLabel: {
                            display: true,
                            labelString: 'Bill Count'
                        }
                    },
                    {
                        id: 'y-axis-2',
                        type: 'linear',
                        display: true,
                        position: 'right',
                        ticks: {
                            // Include a dollar sign in the ticks
                            callback: function (value, index, values) {
                                var divider = 10000000;
                                var dividerLabel = 'Cr';
                                var fixed = 2;
                                if (value > 10000000) {
                                    divider = 10000000;
                                    dividerLabel = 'Cr';
                                } else if (value > 100000) {
                                    divider = 100000;
                                    dividerLabel = 'Lac';
                                }
                                else {
                                    divider = 1;
                                    dividerLabel = '';
                                    fixed = 0;
                                }

                                return (parseFloat(value) / divider).toFixed(fixed) + dividerLabel;
                            }
                        },
                        scaleLabel: {
                            display: true,
                            labelString: 'Sale Value'
                        }
                    }
                ]
            }
        };

        $scope.SearchCondition = {
            locationid: 0,
            fromdate: new Date(),
            todate: new Date()
        };

        $scope.GetWalkin = function () {

            if (typeof ($rootScope.LocationInfo) == 'undefined' ||
                typeof ($rootScope.LocationInfo.location) == 'undefined') {
                $rootScope.Toaster.Error("Please select sales location.");
                return;
            }

            $rootScope.Prompt('Walkin - ' + $rootScope.LocationInfo.location.name, 'Count', function (count) {
                if (count && count != '') {
                    var request = {
                        "item": {
                            'walkincount': count.replaceAll('$', ''),
                            'walkintoken': count,
                            'locationid': $rootScope.LocationInfo.location.id,
                            'isactive': true
                        },
                        "securitytoken": "String content"
                    };

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "CustomerwalkinsService/DirectEntry",
                        data: srequest
                    }).success(function (result) {
                        $scope.totalwalkin += result.walkincount;
                    });
                }
            }, '');

        }

        $scope.Search = function (hidebusinesstrend) {

            if ($scope.BusinessTrandTimer != null)
                $timeout.cancel($scope.BusinessTrandTimer);

            if ($scope.ShowB2BDashboard == true) {
                $scope.DisplayB2BDashboard();
            }

            $scope.SearchCondition.hidebusinesstrend = hidebusinesstrend;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchDashboardData",
                data: srequest
            }).success(function (result) {
                $scope.totalsettlement = 0.0;
                $scope.totalbillcount = 0;
                $scope.totalbillvalue = 0.0;
                $scope.stockvalue = 0.0;
                $scope.totalsalevalue = 0.0;

                $scope.taxlossonsales = 0.0;
                $scope.taxlossonstock = 0.0;
                if (result != null) {
                    $scope.stockvalue = 0;
                    $scope.unsettlebillcount = 0;
                    $scope.absentemployee = 0;
                    $scope.totalemployee = 0;
                    $scope.taxlossonsales = result.taxlossonsales;
                    $scope.taxlossonstock = result.taxlossonstock;
                    $scope.Employees = result.lstempsection;
                    $scope.Sales = result.lstsale;
                    $scope.Settlement = result.lstsettlement;
                    $scope.Salesman = result.lstsalesman;
                    $scope.Salesection = result.lstsalesection;
                    $scope.SaleList = result.lstsalesection;
                    $scope.Fastmoving = 1;
                    $scope.Saleproductgroups = result.lstsalepgrp;
                    $scope.SalesPerHrs = result.lstproduct;
                    $scope.StockValueList = result.lststockvalue;
                    $scope.CurrentStock = result.totalstock;
                    $scope.unsettlebillcount = result.unsettlebillcount;
                    $scope.stockvalue = result.totalstock;
                    $scope.totalsalevalue = result.totalsalevalue;
                    $scope.totalwalkin = result.totalwalkin;
                    $.each($scope.Sales, function (k, v) {
                        $scope.totalbillcount += v.billcount;
                        $scope.totalbillvalue += v.billvalue;
                    });

                    $.each($scope.Settlement, function (k, v) {
                        $scope.totalsettlement += v.cash;
                        $scope.totalsettlement += v.card;
                        $scope.totalsettlement += v.upi;
                        $scope.totalsettlement += v.credit;
                        $scope.totalsettlement += v.voucher;
                        $scope.totalsettlement -= v.cashreturn;
                        $scope.totalsettlement -= v.creditnote;
                        $scope.totalsettlement += v.adjustment;
                        $scope.totalsettlement += v.discount;
                        $scope.totalsettlement += v.loan;
                        if (v.loan > 0)
                            $scope.hasLoan = true;
                        //$scope.totalsettlement -= v.discount;
                    });

                    $scope.absentemployee = result.hrdashboard.absentemployee;
                    $scope.totalemployee = result.hrdashboard.totalemployee;

                    //$.each($scope.Employees, function (k, v) {
                    //    $scope.absentemployee += v.absent;
                    //    $scope.totalemployee += v.total;
                    //});

                    $scope.availableemployee = $scope.totalemployee - $scope.absentemployee;

                    $scope.LoadGraphData();
                    $scope.ShowAdditionalDashboards(result);

                    if (!hidebusinesstrend) {
                        if (result.lrstockvssales != null) {
                            $scope.ShowBusinessTrend(result.lrstockvssales);
                        }
                        else {
                            $scope.BusinessTrandTimer = $timeout(function () {
                                $scope.BusinessTrend();
                            }, 15 * 1000);
                        }
                    }
                }

                $('#btntaxlossonsales').attr('title', "You lost " + $filter('IndianNumber')($scope.taxlossonsales, 0) + " on 12% GST");
                $('#btntaxlossonstock').attr('title', "You may loose " + $filter('IndianNumber')($scope.taxlossonstock, 0) + " on Wrong Pricing");
                $('[data-toggle="tooltip"]').tooltip(); 

            }).error(function (result) {
            });
            //$scope.AdditionalDashBoard();
            //$scope.HRDashboard();
        }

        $scope.BusinessTrandTimer = null;
        $scope.$on("$destroy", function () {
            if ($scope.BusinessTrandTimer != null)
                $timeout.cancel($scope.BusinessTrandTimer);
        });

        $scope.AdditionalDashBoard = function () {

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchAdditionalDashboard",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.warehouse.total = 0;
                    $scope.warehouse.items = result.lstlrvalue;
                    $.each($scope.warehouse.items, function (k, v) {
                        $scope.warehouse.total += v.totalcount;
                    });
                }
            }).error(function (result) {
            });
        }

        $scope.ShowAdditionalDashboards = function (result) {
            if (result != null && result.additionaldashboard != null) {
                $scope.warehouse.total = 0;
                $scope.warehouse.items = result.additionaldashboard.lstlrvalue;
                if ($scope.warehouse.items != null) {
                    $.each($scope.warehouse.items, function (k, v) {
                        $scope.warehouse.total += v.totalcount;
                    });
                }
                $scope.TodayEvents = result.hrdashboard;
            }
        }

        $scope.ShowBusinessTrend = function (result) {

                var lables = [];
                var stock = [];
                var cost = [];
                var margin = [];
                var inward = [];

                $.each(result, function (k, v) {

                    lables.push(v.Label);
                    stock.push(v.Stock);
                    cost.push(v.Sales - v.Margin);
                    margin.push(v.Margin);
                    inward.push(v.Purchase);

                });

                var barChartData = {
                    labels: lables,
                    datasets: [
                        {
                            label: 'Stock',
                            stack: 'Stack 1',
                            type: 'line',
                            fill: false,
                            borderColor: '#7c4dff',
                            yAxisID: "y-axis-2",
                            data: stock
                        },
                        {
                            label: 'Cost',
                            backgroundColor: '#ffb300',
                            stack: 'Stack 0',
                            type: 'bar',
                            yAxisID: "y-axis-1",
                            data: cost
                        }, {
                            label: 'Margin',
                            backgroundColor: '#00bfa5',
                            stack: 'Stack 0',
                            type: 'bar',
                            yAxisID: "y-axis-1",
                            data: margin
                        },
                        {
                            label: 'Inward',
                            backgroundColor: '#ff4081',
                            stack: 'Stack 3',
                            type: 'bar',
                            yAxisID: "y-axis-1",
                            data: inward
                        }]
                };

                var ctx = document.getElementById("canvas").getContext("2d");
                $scope.myBar = new Chart(ctx, {
                    type: 'bar',
                    data: barChartData,
                    options: {

                        tooltips: {
                            mode: 'index',
                            intersect: false
                        },
                        responsive: true,

                        scales: {

                            yAxes: [{
                                type: "linear", // only linear but allow scale type registration. This allows extensions to exist solely for log scale for instance
                                display: true,
                                position: "left",
                                id: "y-axis-1",
                                ticks: {
                                    // Include a dollar sign in the ticks
                                    callback: function (value, index, values) {
                                        var divider = 10000000;
                                        var dividerLabel = 'Cr';
                                        var fixed = 2;
                                        if (value > 10000000) {
                                            divider = 10000000;
                                            dividerLabel = 'Cr';
                                        } else if (value > 100000) {
                                            divider = 100000;
                                            dividerLabel = 'Lac';
                                        } else if (value > 1000) {
                                            divider = 1000;
                                            dividerLabel = 'K';
                                        }
                                        else {
                                            divider = 1;
                                            dividerLabel = '';
                                            fixed = 0;
                                        }

                                        return (parseFloat(value) / divider).toFixed(fixed) + dividerLabel;
                                    }
                                },
                                stacked: true,
                                scaleLabel: {
                                    display: true,
                                    labelString: 'Sales'
                                }
                            }, {
                                type: "linear", // only linear but allow scale type registration. This allows extensions to exist solely for log scale for instance
                                display: true,
                                position: "right",
                                id: "y-axis-2",
                                ticks: {
                                    // Include a dollar sign in the ticks
                                    callback: function (value, index, values) {
                                        var divider = 10000000;
                                        var dividerLabel = 'Cr';
                                        var fixed = 2;
                                        if (value > 10000000) {
                                            divider = 10000000;
                                            dividerLabel = 'Cr';
                                        } else if (value > 100000) {
                                            divider = 100000;
                                            dividerLabel = 'Lac';
                                        } else if (value > 1000) {
                                            divider = 1000;
                                            dividerLabel = 'K';
                                        }
                                        else {
                                            divider = 1;
                                            dividerLabel = '';
                                            fixed = 0;
                                        }

                                        return (parseFloat(value) / divider).toFixed(fixed) + dividerLabel;
                                    }
                                },
                                gridLines: {
                                    drawOnChartArea: false
                                },
                                scaleLabel: {
                                    display: true,
                                    labelString: 'Stock'
                                }
                            }]
                        }
                    }
                });
        }

        $scope.HRDashboard = function () {

            $scope.TodayEvents = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/HRDashboard",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.TodayEvents = result;
                }
            }).error(function (result) {
            });
        }

        $scope.LoadTable = function (name) {
            $timeout(function () {
                $('#' + name).dataTable();
            }, 10);
        }

        $scope.BusinessTrend = function () {

            var request = {
                "item": { 'currentstock': $scope.CurrentStock },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/BusinessTrend",
                data: srequest
            }).success(function (result) {

                var lables = [];
                var stock = [];
                var cost = [];
                var margin = [];
                var inward = [];

                $.each(result, function (k, v) {

                    lables.push(v.Label);
                    stock.push(v.Stock);
                    cost.push(v.Sales - v.Margin);
                    margin.push(v.Margin);
                    inward.push(v.Purchase);

                });

                var barChartData = {
                    labels: lables,
                    datasets: [
                        {
                            label: 'Stock',
                            stack: 'Stack 1',
                            type: 'line',
                            fill: false,
                            borderColor: '#7c4dff',
                            yAxisID: "y-axis-2",
                            data: stock
                        },
                        {
                            label: 'Cost',
                            backgroundColor: '#ffb300',
                            stack: 'Stack 0',
                            type: 'bar',
                            yAxisID: "y-axis-1",
                            data: cost
                        }, {
                            label: 'Margin',
                            backgroundColor: '#00bfa5',
                            stack: 'Stack 0',
                            type: 'bar',
                            yAxisID: "y-axis-1",
                            data: margin
                        },
                        {
                            label: 'Inward',
                            backgroundColor: '#ff4081',
                            stack: 'Stack 3',
                            type: 'bar',
                            yAxisID: "y-axis-1",
                            data: inward
                        }]
                };

                var ctx = document.getElementById("canvas").getContext("2d");
                $scope.myBar = new Chart(ctx, {
                    type: 'bar',
                    data: barChartData,
                    options: {

                        tooltips: {
                            mode: 'index',
                            intersect: false
                        },
                        responsive: true,

                        scales: {

                            yAxes: [{
                                type: "linear", // only linear but allow scale type registration. This allows extensions to exist solely for log scale for instance
                                display: true,
                                position: "left",
                                id: "y-axis-1",
                                ticks: {
                                    // Include a dollar sign in the ticks
                                    callback: function (value, index, values) {
                                        var divider = 10000000;
                                        var dividerLabel = 'Cr';
                                        var fixed = 2;
                                        if (value > 10000000) {
                                            divider = 10000000;
                                            dividerLabel = 'Cr';
                                        } else if (value > 100000) {
                                            divider = 100000;
                                            dividerLabel = 'Lac';
                                        } else if (value > 1000) {
                                            divider = 1000;
                                            dividerLabel = 'K';
                                        }
                                        else {
                                            divider = 1;
                                            dividerLabel = '';
                                            fixed = 0;
                                        }

                                        return (parseFloat(value) / divider).toFixed(fixed) + dividerLabel;
                                    }
                                },
                                stacked: true,
                                scaleLabel: {
                                    display: true,
                                    labelString: 'Sales'
                                }
                            }, {
                                type: "linear", // only linear but allow scale type registration. This allows extensions to exist solely for log scale for instance
                                display: true,
                                position: "right",
                                id: "y-axis-2",
                                ticks: {
                                    // Include a dollar sign in the ticks
                                    callback: function (value, index, values) {
                                        var divider = 10000000;
                                        var dividerLabel = 'Cr';
                                        var fixed = 2;
                                        if (value > 10000000) {
                                            divider = 10000000;
                                            dividerLabel = 'Cr';
                                        } else if (value > 100000) {
                                            divider = 100000;
                                            dividerLabel = 'Lac';
                                        } else if (value > 1000) {
                                            divider = 1000;
                                            dividerLabel = 'K';
                                        }
                                        else {
                                            divider = 1;
                                            dividerLabel = '';
                                            fixed = 0;
                                        }

                                        return (parseFloat(value) / divider).toFixed(fixed) + dividerLabel;
                                    }
                                },
                                gridLines: {
                                    drawOnChartArea: false
                                },
                                scaleLabel: {
                                    display: true,
                                    labelString: 'Stock'
                                }
                            }]
                        }
                    }
                });
            }).error(function (result) {
            });
        }



        $scope.LoadGraphData = function () {

            $.each($scope.Salesman, function (k, v) {
                $scope.salesmanlabels.push(v.employeename);
                $scope.salesmandata.push(v.qty);
            });

            $.each($scope.Salesection, function (k, v) {
                $scope.salesectionlabels.push(v.section);
                $scope.salesectiondata.push(v.qty);
            });

            $scope.labels = [];
            $scope.data = [];
            var dataset1 = [];
            var dataset2 = [];
            $.each($scope.SalesPerHrs, function (k, v) {
                $scope.labels.push(v.name);
                dataset1.push(v.value);
            });
            $.each($scope.SalesPerHrs, function (k, v) {
                dataset2.push(v.qty);
            });
            $scope.data.push(dataset1);
            $scope.data.push(dataset2);
        }


        $scope.LoadB2BDashboard = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.LoadB2BDashboard();
                }, 10);
                return;
            }
            if ($rootScope.UserInfo.appsettings.system_isb2bapplication) {
                $scope.ShowB2BDashboard = true;
                $scope.DisplayB2BDashboard();
            }
        }

        $scope.B2BDashBoard = {};

        $scope.DisplayB2BDashboard = function () {
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "RetailReportService/SearchB2BDashboard",
                data: srequest
            }).success(function (result) {
                $scope.B2BDashBoard = result;
                $scope.B2BDashBoard.suppliertransaction = [];
                $.each(result.debitnotes, function (k, v) {
                    if (v.type == 'DebitNote')
                        $scope.B2BDashBoard.suppliertransaction.push({ 'type': 'Debit Note', 'qty': v.qty, 'amount': v.receivable });
                    else if (v.type == 'DiscountDebityNote')
                        $scope.B2BDashBoard.suppliertransaction.push({ 'type': 'Supplier Discount', 'qty': v.qty, 'amount': v.receivable });
                });

                $scope.B2BDashBoard.suppliertransaction.push({ 'type': 'Purchase', 'qty': result.purchase.buyingqty, 'amount': result.purchase.buyingamount });
                $scope.B2BDashBoard.suppliertransaction.push({ 'type': 'Payment', 'qty': '', 'amount': result.payments.amount });

                $.each(result.sales, function (k, v) {
                    if (v.type == 'ReturnBill')
                        v.type = 'Sales Return';
                    else
                        v.type = 'Sales - ' + v.type;
                });
                if (result.bulksales.issuedqty != 0) {
                    result.sales.push({
                        'type': 'Approve Issued', 'qty': result.bulksales.issuedqty,
                        'receivable': result.bulksales.issuedamount
                    });
                }
                if (result.bulksales.returnqty != 0) {
                    result.sales.push({
                        'type': 'Approve Return', 'qty': result.bulksales.returnqty,
                        'receivable': result.bulksales.returnamount
                    });
                }
                if (result.bulksales.billedqty != 0) {
                    result.sales.push({
                        'type': 'Approve Billed', 'qty': result.bulksales.billedqty,
                        'receivable': 0
                    });
                }

                $.each(result.advances, function (k, v) {
                    if (v.type == 'customer')
                        result.sales.push({
                            'type': 'Advance -' + v.name, 'qty': 0,
                            'receivable': v.advance
                        });
                    else {
                        result.purchasetransaction.push({ 'type': 'Advance - ' + v.name, 'value': v.advance });
                    }
                });
            });
        }

        $scope.ShowTaxLoss = function (mode) {
            var newurl = $location.absUrl();
            newurl = newurl.substring(0, newurl.indexOf('?'));
            newurl = newurl.substring(0,newurl.lastIndexOf('/'));
            newurl += '/File/RangeTaxSalesLossList/' + $scope.UserInfo.OrganizationID;

            var fromdate = $filter('date')(new Date($scope.SearchCondition.fromdate), 'dd-MM-yyyy');
            var todate = $filter('date')(new Date($scope.SearchCondition.todate), 'dd-MM-yyyy');
            window.open(newurl + '/' + mode + '/' + fromdate+'/'+todate, '_target');
        }

        $timeout(function () {
            $scope.SearchCondition.fromdate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.SearchCondition.todate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.Search(false);
            $scope.LoadB2BDashboard();
        }, 50);

    }]);