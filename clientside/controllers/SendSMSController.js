
var app = angular.module('InfinityLab');

app.controller('SendSMSController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', '$stateParams', 'Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, $stateParams, Excel) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;


        $scope.Types = [{ 'id': 'CUSTOMERS', 'name': 'CUSTOMERS' },
        { 'id': 'CUSTOMERCAMPAIGN', 'name': 'CAMPAIGN' },
        { 'id': 'SUPPLIERS', 'name': 'SUPPLIERS' }, { 'id': 'AGENTS', 'name': 'AGENTS' }, { 'id': 'EMPLOYEES', 'name': 'EMPLOYEES' }];

        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });

        $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
            $scope.CustomerCategory = results;
        });

        restService.Get('ReferencelistService/CUSTOMERCAMPAIGN', true).then(function (results) {
            $scope.CampaignList = results;
        });

        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.Area = restService.Get('ReferencelistService/AREA', true).then(function (results) {
            $scope.Area = results;
        });

        $scope.AreaGroup1 = restService.Get('ReferencelistService/AREAGROUP1', true).then(function (results) {
            $scope.AreaGroup1 = results;
        });

        $scope.AreaGroup2 = restService.Get('ReferencelistService/AREAGROUP2', true).then(function (results) {
            $scope.AreaGroup2 = results;
        });


        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });
        $scope.Religions = restService.Get('ReferencelistService/RELIGION', true).then(function (results) {
            $scope.Religions = results;
        });
        $scope.Counter = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counter = results;
        });

        $scope.Floor = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floor = results;
        });
        $scope.SearchCondition = {
            typeid: "CUSTOMERS",
            mobileno: "",
            categoryid: "",
            productid: "",
            brandid: "",
            fromdate: "",
            todate: "",
            cityid: ""
        }
        $scope.Customers = true;
        $scope.SearchResult = [];
        $scope.CustomerList = [];

        $scope.Search = function () {
            $scope.CustomerList = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            if ($scope.SearchCondition.typeid == 'SUPPLIERS') {
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SupplierService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.CustomerList = result;

                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                    }
                    $scope.MyCustomerConfig();

                }).error(function (result) {
                });
            }
            else if ($scope.SearchCondition.typeid == 'AGENTS') {
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "AgentService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.CustomerList = result;

                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                    }
                    $scope.MyCustomerConfig();

                }).error(function (result) {
                });
            }
            else if ($scope.SearchCondition.typeid == 'CUSTOMERS' ||
                $scope.SearchCondition.typeid == 'CUSTOMERCAMPAIGN' ||
                $scope.SearchCondition.typeid == 'VIP CUSTOMERS') {
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "CustomerService/SearchMyCustomer",
                    data: srequest
                }).success(function (result) {
                    $scope.CustomerList = result;

                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                    }
                    $scope.MyCustomerConfig();

                }).error(function (result) {
                });
            }
            else if ($scope.SearchCondition.typeid == 'EMPLOYEES') {
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "EmployeeService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.CustomerList = result;

                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                    }
                    $scope.MyCustomerConfig();

                }).error(function (result) {
                });
            }
        }


        $scope.ChangeReport = function () {
            $scope.Customers = $scope.SearchCondition.typeid == 'CUSTOMERS' || $scope.SearchCondition.typeid == 'CUSTOMERCAMPAIGN';
            $scope.Employee = $scope.SearchCondition.typeid == 'EMPLOYEES';
            $scope.Supplier = $scope.SearchCondition.typeid == 'SUPPLIERS' || $scope.SearchCondition.typeid == 'AGENTS';
        }

        $scope.AddMobileNo = function () {
            if ($scope.CustomerList == undefined) {
                $scope.CustomerList = [];
            }
            $scope.CustomerList.push(
                {
                    'mobileno': $scope.SearchCondition.mobileno,
                    'name': $scope.SearchCondition.name,
                });
            $scope.MyCustomerConfig();
            $scope.SearchCondition.mobileno = '';
            $scope['setFocusmobileno']();
        }

        $scope.RemoveMobileNo = function () {

            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows == null || seletedRows.length == 0) {
                $rootScope.Toaster.Info("Please select the row from the grid to remove from the sms list");
                return;
            }
            $.each(seletedRows, function (k, v) {
                $scope.CustomerList.splice($scope.CustomerList.indexOf(v), 1);
            });
            $scope.MyCustomerConfig();
            $scope.SearchCondition.mobileno = '';
            $scope['setFocusmobileno']();
        }

        $scope.MyCustomerConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '80px', '100%', true, true, false, 60);

            igrid.AddSimpleColumn('name', 'NAME', 200).editor = Slick.Editors.Text;

            if ($scope.SearchCondition.typeid == 'SUPPLIERS' || $scope.SearchCondition.typeid == 'EMPLOYEES' || $scope.SearchCondition.typeid == 'AGENTS') {
                igrid.AddSimpleColumn('contactno', 'CONTACT NO', 150).editor = Slick.Editors.Text;
            }
            if ($scope.SearchCondition.typeid == 'CUSTOMERS' || $scope.SearchCondition.typeid == 'VIP CUSTOMERS'
                || $scope.SearchCondition.typeid == '' || $scope.SearchCondition.typeid == 'CUSTOMERCAMPAIGN') {
                igrid.AddSimpleColumn('mobileno', 'MOBILE NO', 150).editor = Slick.Editors.Text;
            }
            if ($scope.SearchCondition.typeid == 'CUSTOMERS' || $scope.SearchCondition.typeid == 'CUSTOMERCAMPAIGN') {
                igrid.AddFormatedColumn('dateofbirth', 'DOB', 60, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('billvalue', 'Total Pur.Value', 100, 'N2', 2).editor = Slick.Editors.Text;
                igrid.AddFormatedColumn('totalbills', 'No of Bills', 100, 'N2', 2).editor = Slick.Editors.Text;
            }
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.CustomerList);

        }

        $scope.fileuploadedcomplete = function (value) {

            $scope.fileuploadedcomplete = null;
            if (value.columns.length == 0 || value.columns.length > 2) {
                $rootScope.Toaster.Error("Uploaded file should have only 2 columns.");
                return;
            }
            if (value.rows.length < 1) {
                $rootScope.Toaster.Error("Uploaded file don't have rows.");
                return;
            }

            $scope.CustomerList = [];
            $.each(value.rows, function (k, v) {
                if (k > 0) {
                    $scope.CustomerList.push({
                        'mobileno': v.values[0],
                        'name': v.values.length > 1 ? v.values[1] : '',
                    });
                }
            });
            $scope.MyCustomerConfig();
        }
        $scope.MyCustomerConfig();
        $scope.SendSMS = function () {

            var request = {
                template: $scope.SearchCondition.template,
                smsurl: $scope.SearchCondition.smsurl,
                ispromotional: true,
                excludecustomerhassale: $scope.SearchCondition.excludecustomerhassale,
                fromdate: $scope.SearchCondition.excludefromdate,
                todate: $scope.SearchCondition.excludetodate,
                items: []
            };

            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {

                if ($scope.SearchCondition.typeid == 'SUPPLIERS' || $scope.SearchCondition.typeid == 'EMPLOYEES' || $scope.SearchCondition.typeid == 'AGENTS') {
                    request.items.push({
                        'no': v.contactno, 'name': v.name
                    });
                }
                if ($scope.SearchCondition.typeid == 'CUSTOMERS' || $scope.SearchCondition.typeid == 'VIP CUSTOMERS' ||
                    $scope.SearchCondition.typeid == '' || $scope.SearchCondition.typeid == 'CUSTOMERCAMPAIGN') {
                    request.items.push({
                        'no': v.mobileno, 'name': v.name
                    });
                }
            });


            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "File/SendSMS",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Message added in Queue.will be send shortly");
            }).error(function (result) {
            });
        }

        $scope.SendMessage = function () {

            var request = {
                template: $scope.SearchCondition.template,
                smsurl: $scope.SearchCondition.smsurl,
                scheduleid: $scope.SearchCondition.campaignid,
                ispromotional: true,
                items: []
            };

            var seletedRows = this.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                request.items.push({
                    'no': v.mobileno, 'name': v.name
                });
            });


            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "File/SendCampaign",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Message added in Queue.will be send shortly");
            }).error(function (result) {
            });
        }
    }]);