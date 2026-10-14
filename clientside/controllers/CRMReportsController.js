
var app = angular.module('InfinityLab');

app.controller('CRMReportsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout) {
        $rootScope.AccordionName = $location.path();

        $scope.SearchRef = restService.SearchReference;
      
        $scope.Entity = {
            "id": 0,
            "name": ""
         
        };

        $scope.CustomerCompaign = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.SearchResult = [];
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

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });

        $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
            $scope.CustomerCategory = results;
        });

        $scope.Countries = restService.Get('ReferencelistService/COUNTRY', true).then(function (results) {
            $scope.Countries = results;
        });

        $scope.TaskList = restService.Get('ScheduleService/TaskList', true).then(function (results) {
            $scope.TaskList = results;
        });

        $scope.CustomerType = [{ 'id': 1, 'name': 'Employee' },
            { 'id': 2, 'name': 'Retail' },
            { 'id': 3, 'name': 'Provider' },
            { 'id': 4, 'name': 'Customer' },
            { 'id': 5, 'name': 'Tailoring' }];

        $scope.Gender = [{ 'id': 1, 'name': 'Male' }, { 'id': 2, 'name': 'Female' }];
        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });

        $scope.Religions = restService.Get('ReferencelistService/RELIGION', true).then(function (results) {
            $scope.Religions = results;
        });
        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            name: "",
            fromdate: "",
            todate: "",
            married: false,
            productid: "",
            brandid: "",
            sizeid: "",
            cityid: "",
            toprice: "",
            fromprice: "",
            religionid:""

        }

        $scope.Clear = function () {
            $scope.CustomerCompaign = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }
        $scope.CustomerList = [];
        $scope.TopCustomerList = [];
        $scope.FrequentCustomerList = [];
        $scope.FeedbackList = [];
        $scope.SMSLogList = [];

        $scope.Send = function () {

            request = {
                "Item": $scope.CustomerCompaign,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("SMS SENT Successfully.!");
                    $scope.CustomerCompaign = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }

        $scope.SearchBirthday = function () {
            $scope.SearchResult = [];
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/SearchBirthday",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.BirthDayConfig();
            }).error(function (result) {
            });

        }

        $scope.SearchAnniversary = function () {
            $scope.SearchCondition.ismarriageday = true;
            $scope.SearchResult = [];
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/SearchBirthday",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.BirthDayConfig();
            }).error(function (result) {
            });

        }

        $scope.BirthDayConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true,0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            if ($scope.SearchCondition.forkids)
                igrid.AddSimpleColumn('name', 'Kids Name', 200);
            else
                igrid.AddSimpleColumn('name', 'Customer Name', 200);

            igrid.AddSimpleColumn('mobileno', 'Mobile No', 100);
            igrid.AddReferenceColumn('gender', 'Gender', 60, $scope.Gender);
            igrid.AddFormatedColumn('dateofbirth', 'Date of Birth', 100, 'dd-MM-yyyy');
            if (!$scope.SearchCondition.forkids)
                igrid.AddFormatedColumn('marriagedate', 'Marriage Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('gstno', 'GST NO', 100);
            igrid.AddSimpleColumn('pincode', 'Pin Code', 80);
            igrid.AddSimpleColumn('area', 'Area', 100);
            igrid.AddFormatedColumn('loyelty', 'Point', 60, 'N2', 2);
            igrid.AddSimpleColumn('address', 'Address', 200);

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.PrintMyCustomers = function () {

            $scope.CustomerList = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/PrepareList",
                data: srequest
            }).success(function (result) {

                $rootScope.Download('CustomerService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + result, 'customerlabel.pdf', 'application/pdf');
                return;

                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "customerlabel.pdf";
                    link.href = 'CustomerService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + result;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);

            }).error(function (result) {

            });
        }

        $scope.SearchMyCustomers = function () {
          
            $scope.CustomerList = [];
           
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

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

        $scope.SearchSMSLog = function () {
            $scope.SMSLogList = [];
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/SearchSMSLog",
                data: srequest
            }).success(function (result) {

                $scope.SMSLogList = result;
                $scope.SMSLogConfig();
            }).error(function (result) {
            });

        }

        $scope.SMSLogConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SMSLogList);

            igrid.AddSimpleColumn('name', 'Message Type', 100);
            igrid.AddSimpleColumn('description', 'Message Name', 150);
            igrid.AddSimpleColumn('message', 'Message', 350);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 120);
            igrid.AddFormatedColumn('actionon', 'Action On', 100, 'dd-MM-yyyy');

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SMSLogList);
        }

        $scope.SearchTopCustomers = function () {

            $scope.TopCustomerList = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/SearchTopCustomers",
                data: srequest
            }).success(function (result) {
                $scope.TopCustomerList = result;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.TopCustomerConfig();

            }).error(function (result) {
            });
        }
        $scope.SearchFrequentInactive = function () {

            $scope.FrequentCustomerList = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerService/SearchInactiveCustomers",
                data: srequest
            }).success(function (result) {
                $scope.FrequentCustomerList = result;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.FrequentCustomerConfig();

            }).error(function (result) {
            });
        }
        $scope.SearchFeedback = function () {

            $scope.FeedbackList = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CustomerwalkinsService/Search",
                data: srequest
            }).success(function (result) {
                $scope.FeedbackList = result;

                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.FeedbackConfig();

            }).error(function (result) {
            });
        }


        $scope.MyCustomerConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true,0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.CustomerList);

            igrid.AddSimpleColumn('name', 'Customer Name', 200);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 100);
            igrid.AddReferenceColumn('gender', 'Gender', 60, $scope.Gender);
            igrid.AddFormatedColumn('dateofbirth', 'Date of Birth', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('marriagedate', 'Marriage Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('religionid', 'Religion', 80, $scope.Religions);
            igrid.AddSimpleColumn('gstno', 'GST NO', 100);
            igrid.AddSimpleColumn('pincode', 'Pin Code', 80);
            igrid.AddSimpleColumn('area', 'Area', 100);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.Cities);
            igrid.AddSimpleColumn('address', 'Address', 200);
            igrid.AddFormatedColumn('loyelty', 'Point', 60, 'N2', 2);
            igrid.AddFormatedColumn('billvalue', 'Total Pur.Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalbills', 'No of Bills', 60, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.CustomerList);

        }
        $scope.TopCustomerConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true,0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('name', 'CUSTOMER NAME', 250);
            igrid.AddSimpleColumn('mobileno', 'MOBILE NO', 150);
            igrid.AddSimpleColumn('area', 'AREA', 100);

            igrid.AddSimpleColumn('billvalue', 'BILLVALUE', 150).total = true;
            igrid.AddSimpleColumn('totalbills', 'TOTAL BILLS', 100).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.TopCustomerList);

        }
        $scope.FrequentCustomerConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.FrequentCustomerList);

            igrid.AddSimpleColumn('name', 'Customer Name', 200);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 100);
            //igrid.AddReferenceColumn('gender', 'Gender', 60, $scope.Gender);
            igrid.AddFormatedColumn('dateofbirth', 'Date of Birth', 100, 'dd-MM-yyyy');
            //igrid.AddFormatedColumn('marriagedate', 'Marriage Date', 100, 'dd-MM-yyyy');
            //igrid.AddReferenceColumn('religionid', 'Religion', 80, $scope.Religions);
            igrid.AddSimpleColumn('gstno', 'GST NO', 100);
            //igrid.AddSimpleColumn('pincode', 'Pin Code', 80);
            //igrid.AddSimpleColumn('area', 'Area', 100);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.Cities);
            //igrid.AddSimpleColumn('address', 'Address', 200);
            //igrid.AddFormatedColumn('loyelty', 'Point', 60, 'N2', 2);
            igrid.AddFormatedColumn('billvalue', 'First Pur.Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalbills', 'First Pur.Bills', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('billvalue2', 'Second Date Value', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalbills2', 'Second Date Bills', 60, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.FrequentCustomerList);

        }

        $scope.FeedbackConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.FeedbackList);

            igrid.AddSimpleColumn('name', 'Customer Name', 100);
            igrid.AddSimpleColumn('mobileno', 'Mobile No', 100);
            igrid.AddSimpleColumn('billno', 'Bill No', 90);
            igrid.AddFormatedColumn('createdon', 'Date', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('feedback', 'Feedback Given By Customer', 300);
            //igrid.AddSimpleColumn('area', 'Area', 100);
            //igrid.AddReferenceColumn('cityid', 'City', 100, $scope.Cities);
            //igrid.AddSimpleColumn('address', 'Address', 200);
        

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.FeedbackList);

        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == "customerlist")
                subtitle = 'CUSTOMER LIST';

            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }


        $scope.SendSMS = function (list) {
            var items = [];

            var dialog = ngDialog.open({
                template: 'SendSms',
                data: { 'scope': $scope, 'list': items },
                width: '650px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                  
                    $scope.Send = function (item) {

                        var poRequest = {
                            'SendSms': $scope.PopupList, isactive: true,
                            version: 1,
                            supplierid: $scope.supplierid,
                            podate: $scope.podate,
                            expectedate: $scope.expectedate
                        };
                        var request = {
                            "item": poRequest,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "PurchaseorderService/Save",
                            data: srequest
                        }).success(function (result) {

                            if (result != null && result.id > 0) {
                                $scope.ngDialogData.result = result;
                                $scope.closeThisDialog($scope.ngDialogData);
                                $rootScope.Toaster.Info("PO generated successfully with PO No : " + result.pono);
                            }
                            else {
                                $scope.ErrorMessage = result.ErrorMessage;
                                $scope.ErrorStyle = "danger";
                            }

                        }).error(function (result) {
                        });

                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                {

                }
            });
        }

        $timeout(function () {
            $scope.setFocusfirst();
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "AgentService/Search",
                    data: srequest
                }).success(function (result) {

                    if (angular.isArray(result)) {
                        $scope.ShowSearch = false;
                        $scope.Agent = result[0];
                    }

                }).error(function (result) {
                });
            }
        }, 100);

    }]);