
var app = angular.module('InfinityLab');

app.controller('HRDashboardController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService) {

        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.totalwalkin = 0;
        $scope.SearchResult = [];
        $scope.TodayEvents = [];
        $scope.NewJoin = [];
        $scope.LeftEmp = [];



        $scope.Locations = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            results.push({ 'id': -1, 'name': 'All Location' });
            $scope.Locations = results;
        });

        $scope.SearchMonth = {
            locationid: 0,
            fromdate: new Date(),
            todate: new Date()
        };


        $scope.TodayBirthday = [];
        $scope.TodayMarriageDay = [];
        $scope.TodayJoined = [];
        $scope.MonthlyJoined = [];
        $scope.Monthlylefted = [];

        $scope.GetWalkin = function () {

            $rootScope.Prompt('Walkin', 'Count', function (count) {
                if (count && count != '') {

                    var request = {
                        "item": {
                            'walkincount': count,
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
        $scope.locationid = -1;
        $scope.sectionabsent = 0;
        $scope.sectiontotal = 0;
        $scope.AllSectionwise = [];
        $scope.ShowSectionDetails = function (id) {
            $scope.sectionabsent = 0;
            $scope.sectiontotal = 0;
            var location = $scope.Locations.find(function (item) {
                return item.id == id
            });
            var result = [];
            $.each($scope.AllSectionwise, function (k, v) {

                if (id == -1 || v.locationname == location.name) {
                    var exist = result.find(function (item) {
                        return item.sectionname == v.sectionname;
                    });
                    if (exist == undefined || exist == null) {
                        exist = { 'sectionname': v.sectionname, 'absent': 0, 'total': 0, 'absentemployees': [] };
                        result.push(exist);
                    }
                    exist.absent += v.absent;
                    exist.total += v.total;

                    $scope.sectionabsent += v.absent;
                    $scope.sectiontotal += v.total;

                    $.each(v.absentemployees, function (kk, vv) {
                        exist.absentemployees.push(vv);
                    })
                }
            });
            $scope.Employees = result;
        }




        $scope.SearchCondition = {
            locationid: 0,
            fromdate: new Date(),
            todate: new Date()
        };

        $scope.HRDashboard = function () {
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

                $scope.totalwalkin = result.totalwalkin;
                $scope.Dashboard = result;
                $scope.TodayBirthday = result.list[0].detail;
                $scope.TodayMarriageDay = result.list[1].detail;
                $scope.TodayJoined = result.list[2].detail;
                $scope.MonthlyJoined = result.list[3].detail;
                $scope.Monthlylefted = result.list[4].detail;

                $scope.TodayEvents = { 'list': [] };
                $scope.TodayEvents.list.push(result.list[2]);
                $scope.TodayEvents.list.push(result.list[0]);
                $scope.TodayEvents.list.push(result.list[1]);
                $scope.TodayEvents.list.push(result.list[3]);
                $scope.TodayEvents.list.push(result.list[4]);
                $scope.AllSectionwise = result.list[5].detail;
                $scope.ShowSectionDetails(-1);
                $scope.availableemployee = result.totalemployee - result.absentemployee;
                $scope.totalemployee = result.totalemployee;
            }).error(function (result) {
            });

        }
       
    }]);