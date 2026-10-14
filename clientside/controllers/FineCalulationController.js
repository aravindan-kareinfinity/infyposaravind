
var app = angular.module('InfinityLab');

app.controller('FineCalculationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Mode = 'Fine';
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.Entity = {
            fromdate: "",
            todate: "",
            actiondate: "",
            toactiondate: "",
            fromactiondate: "",
            type: "StoreOpening",
            StoreOpeningAttribute: {
                'opening': '', 'closing': ''
            },
            companyid: "",
            locationid: "",
            isactive: true,
            version:1
        };

        $scope.FineEntry = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.New = function () {
            $scope.FineEntry = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.Clear = function () {
            $scope.FineEntry = (JSON.parse(JSON.stringify($scope.Entity)));
            if ($scope.igrid != null)
                $scope.igrid.destroy();
        }

     
        $scope.GenerateFine = function () {
            request = {
                "Item": $scope.FineEntry,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentivesService/GenerateFine",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.FineConfig();
            });
        }

        $scope.FineConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 130);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('EmployeeCode', 'Code', 100);
            igrid.AddSimpleColumn('Name', 'Name', 150);
            igrid.AddSimpleColumn('GrossPay', 'Gross Pay', 75);
            igrid.AddSimpleColumn('workingdays', 'Days', 75);
            igrid.AddSimpleColumn('actualworking', 'Working', 75);
            igrid.AddSimpleColumn('nonworking', 'Non Working', 75);
            igrid.AddSimpleColumn('latecheckin', 'Late', 75);
            igrid.AddSimpleColumn('earlycheckout', 'Early', 75);
            igrid.AddSimpleColumn('earlyandlate', 'Minutes', 75);
            igrid.AddSimpleColumn('Fine', 'Fine', 75);
            //igrid.AddButtonColumn('action', 'Action', 100, 'Change', function (e, args) {
            //    $scope.$apply(function () {
            //        $scope.ManageLRInboxList(args.item);
            //    });
            //    return true;
            //});
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

    }]);