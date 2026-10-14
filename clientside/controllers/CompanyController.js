//ReferencelistService/CITY
var app = angular.module('InfinityLab');

app.controller('CompanyController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $stateParams) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.screentype = $stateParams.type;

        $scope.servertypes = [{ 'id': 'Printer', 'name': 'Printer Only' },
        { 'id': 'Offline', 'name': 'Offline Server' },
        { 'id': 'Both', 'name': 'Both' },
        { 'id': 'WebPrinter', 'name': 'Web Printer' },
            { 'id': 'SSLPrinter', 'name': 'SSL Printer' },
            { 'id': 'IPPrinter', 'name': 'IP Printer' },];

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });

        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });

        $scope.Countries = restService.Get('ReferencelistService/COUNTRY', true).then(function (results) {
            $scope.Countries = results;
        });

        $scope.Tallyconfigtypes = [{ 'code': 'PURCHASE', 'name': 'Purchase' },
        { 'code': 'SALES', 'name': 'Sales' }];



        $scope.SearchRef = restService.SearchReference;


        $scope.Entity = {
            name: "",
            contactperson: "",
            contactno: "",
            emailid: "",
            address: "",
            cityid: "",
            stateid: "",
            countryid: "",
            pincode: "",
            isactive: true,
            tinanddate: "",
            cstanddate: "",
            areacode: "",
            tan: "",
            pan: "",
            bankaccountname: "",
            bankaccountno: "",
            bankifci: "",
            pfno: "",
            esino: "",
            version: 0,
            code: "",
            website: ""
        };
        $scope.Company = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Company = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";
        $scope.ShowAllCompany = function () {
            $scope.Company = null;
            var request = {
                "Item": {},
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CompanyService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
            }).error(function (result) {
            });
        }

        $scope.ShowTallyConfig = function (item) {
            $.each($scope.SearchResult, function (k, v) {
                v.style = {};
            });
            item.style = {
                "background-color": "lightgrey", "color":"orangered" };
            $scope.Company = item;
            $scope.tallyconfigtype = 'PURCHASE';
        }

        $scope.SearchCode = function (type) {
            if (type == 'Supplier') {
                var request = {
                    "Item": { code: $scope.Company.company_as_suppliercode },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "SupplierService/Search",
                    data: srequest
                }).success(function (result) {
                    if (angular.isArray(result) && result.length == 1) {
                        $scope.Company.company_as_supplierid = result[0].id;
                        $scope.Company.suppliername = result[0].name;
                    }
                    else {
                        $scope.Company.company_as_supplierid = 0;
                        $scope.Company.suppliername = 'Not Found';
                        $scope.CreateCompanyAsSupplier();
                    }
                });
            }
            else {
                var request = {
                    "Item": { code: $scope.Company.company_as_customercode },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "CustomerService/Search",
                    data: srequest
                }).success(function (result) {
                    if (angular.isArray(result) && result.length == 1) {
                        $scope.Company.company_as_customerid = result[0].id;
                        $scope.Company.customername = result[0].name;
                    }
                    else {
                        $scope.Company.company_as_customerid = 0;
                        $scope.Company.customername = 'Not Found';
                        $scope.CreateCompanyAsCustomer();
                    }
                });
            }
        }

        $scope.CreateCompanyAsSupplier = function () {
            $.confirm({
                title: 'Supplier',
                content: 'Do you want to create this company as supplier?',
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
                                "Item": $scope.Company,
                                "SecurityToken": "String content"
                            }
                            $http({
                                method: 'POST',
                                url: "SupplierService/CompanyAsSupplier",
                                data: request
                            }).success(function (result) {
                                if (result) {
                                    $scope.Company.company_as_supplierid = result.id;
                                    $scope.Company.suppliername = result.name;
                                }
                            }).error(function (result) {

                            });
                        }
                    },
                    cancel: function () {
                        //close
                    },
                },

            });
        }

        $scope.CreateCompanyAsCustomer = function () {
            $.confirm({
                title: 'Customer',
                content: 'Do you want to create this company as customer?',
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
                                "Item": $scope.Company,
                                "SecurityToken": "String content"
                            }
                            $http({
                                method: 'POST',
                                url: "CustomerService/CompanyAsCustomer",
                                data: request
                            }).success(function (result) {
                                if (result) {
                                    $scope.Company.company_as_customerid = result.id;
                                    $scope.Company.customername = result.name;
                                }
                            }).error(function (result) {

                            });

                        }
                    },
                    cancel: function () {
                        //close
                    },
                },

            });
        }

        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }

        $scope.New = function () {
            $scope.Company = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.IsEmpty = function (value) {
            return typeof (value) == 'undefined' || value == '';
        }
        $scope.Save = function () {

            if ($scope.IsEmpty($scope.Company.name)) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please fill out companyname");
                return;
            }
            if ($scope.screentype == 'Company' && $scope.IsEmpty($scope.Company.contactperson)) {
                $scope['setFocuscontactperson']();
                $rootScope.Toaster.Error("Please fill out  contactperson");
                return;
            }
            if ($scope.screentype == 'Company' && $scope.IsEmpty($scope.Company.contactno)) {
                $scope['setFocuscontactno']();
                $rootScope.Toaster.Error("Please fill out  contactno");
                return;
            }

            if ($scope.screentype == 'Company')
                $scope.Company.gstno = $scope.Company.tinanddate;
            request = {
                "Item": $scope.Company,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CompanyService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Company Saved Successfully.!");
                    $scope.Company = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }


            }).error(function (result) {
            });

        }


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
                                    url: "CompanyService/Delete",
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
                $scope.ShowSearch = false;
                $scope.Company = item;
                $scope.Navigation.SetMode('EDIT');

            }
        }

        $scope.RemoveServer = function (server) {
            $scope.Company.servers.splice($scope.Company.servers.indexOf(server), 1);
        };

        $scope.AddServer = function () {
            if (typeof ($scope.Company.servers) == 'undefined' || $scope.Company.servers == null)
                $scope.Company.servers = [];

            $scope.Company.servers.push({
                'name': $scope.Company.serverName,
                'ip': $scope.Company.serverIp,
                'port': $scope.Company.serverPort,
                'location': $scope.Company.serverLocation,
                'type': $scope.Company.serverType
            });
        };

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CompanyService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.CompanyConfig();
            }).error(function (result) {
            });

        }

        $scope.CompanyConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '420px', '100%', true, false, true, 50);
            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddSimpleColumn('contactperson', 'Contact Person', 150);
            igrid.AddSimpleColumn('contactno', 'Contact No', 140);
            igrid.AddSimpleColumn('emailid', 'Email Id', 150);
            igrid.AddSimpleColumn('address', 'Address', 200);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.Cities);
            igrid.AddSimpleColumn('tinanddate', 'GST', 150);

            var commands = "";
            if ($rootScope.UserInfo.p_COMPANYMASTER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_COMPANYMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_COMPANYMASTER.modify || $rootScope.UserInfo.p_COMPANYMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }


        $timeout(function () {
            $scope.setFocusfirst();
            $scope.CompanyConfig();
        }, 10);

    }]);
