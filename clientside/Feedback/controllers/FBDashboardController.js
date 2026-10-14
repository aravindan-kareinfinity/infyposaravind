
var app = angular.module('InfinityLab');

app.controller('FBDashboardController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {

        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        var legendPosition = 'right';

        $scope.yoptions = {
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: 'Yearly Ratings',
                position: 'bottom'
            },
            legend: {
                display: true,
                position: 'right'
            },
            plugins: {
                labels: {
                    render: 'value',
                    fontColor: function (data) {
                        var rgb = hexToRgb(data.dataset.backgroundColor[data.index]);
                        var threshold = 140;
                        var luminance = 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
                        return luminance > threshold ? 'black' : 'white';
                    }
                }
            }
        };

        $scope.moptions = {
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: 'Monthly Ratings',
                position: 'bottom'
            },
            legend: {
                display: true,
                position: 'right'
            },
            plugins: {
                labels: {
                    render: 'value',
                    fontColor: function (data) {
                        var rgb = hexToRgb(data.dataset.backgroundColor[data.index]);
                        var threshold = 140;
                        var luminance = 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
                        return luminance > threshold ? 'black' : 'white';
                    }
                }
            }
        };

        $scope.woptions = {
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: 'Weekly Ratings',
                position:'bottom'
            },
            legend: {
                display: true,
                position: 'right'
            },
            plugins: {
                labels: {
                    render: 'value',
                    fontColor: function (data) {
                        var rgb = hexToRgb(data.dataset.backgroundColor[data.index]);
                        var threshold = 140;
                        var luminance = 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
                        return luminance > threshold ? 'black' : 'white';
                    }
                }
            }
        };


        $http({
            method: 'GET',
            url: "FeedbackService/Dashboard"
        }).success(function (result) {

            $scope.ydata = result.ydata;
            $scope.ylabels = result.ylabels;

            $scope.mdata = result.mdata;
            $scope.mlabels = result.mlabels;

            $scope.wdata = result.wdata;
            $scope.wlabels = result.wlabels;

        }).error(function (result) {
        });


        $timeout(function () {
            $scope.SearchCondition = {};
            $scope.SearchCondition.fromdate = $filter('date')(new Date().setMonth(new Date().getMonth() - 1), datePicker.shortDisplay);
            $scope.SearchCondition.todate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.Search();
        }, 50);

        var COLORS = [
            '#FF3784',
            '#36A2EB',
            '#4BC0C0',
            '#F77825',
            '#9966FF',
            '#00A8C6',
            '#379F7A',
            '#CC2738',
            '#8B628A',
            '#8FBE00',
            '#606060'
        ];

        var result = {
            "labels": ["this for question 5", "this question 2", "this question 3", "this for question 4"],
            "datasets": [
                {
                    "labels": ["this for question 4", "mens", "test2", "b"],
                    "data": [5,5,5,5],
                    backgroundColor: [
                        '#FF6384',
                        '#FF6384',
                        '#FF6384',
                        '#FF6384'
                    ]
                }]
        }
        

        $scope.Search = function () {
            var request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            };


            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "FeedbackService/SearchChartData",
                data: srequest
            }).success(function (result) {


                $scope.LeftChart = new Chart(document.getElementById('leftchart'), {
                    type: 'bar',
                    data: {
                        labels: result.labels,
                        datasets: result.datasets
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            labels: {
                                render: 'datalabel',
                                position: 'border',
                                fontSize: 14,
                                fontStyle: 'bold',
                                fontColor: '#000',
                                fontFamily: '"Lucida Console", Monaco, monospace'
                            }
                        },
                        scales: {
                            xAxes: [{
                                stacked: true,
                            }],
                            yAxes: [{
                                stacked: true
                            }]
                        }
                    }
                });

            }).error(function (result) {
            });
        }
    }]);