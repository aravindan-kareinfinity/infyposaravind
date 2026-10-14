var app = angular.module('InfinityLab');

app.controller('LoyaltyManagementController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.customerid = 0;
        $scope.totalbillvalue = 0;
        $scope.totalreward = 0;
        $scope.customername = "";
        $scope.ShowSearch = true;
        $scope.SearchResult = [];
        $scope.RewardBillResults = [];

        $scope.RetailLocation = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.RetailLocation = results;
        });

        $scope.Entity = {
            "id": 0,
            "createdby": 0,
            "billno": "",
            "billdate": "",
            "billvalue": 0,
            "loyelty": null,
            "adjusted": 0,
            "expiry": null,
            "oldloyelty": null,
            "oldadjusted": 0,
            "oldexpiry": null,
            "remark": ""
        };

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }
        $scope.SearchCondition = {
            emailid: "",
            name: "",
            mobileno: "", 
            issummary: true
        }

        $scope.Search = function () {
            $scope.customername = "";
            $scope.customerid = 0;
            if ($scope.SearchCondition.issummary == undefined || $scope.SearchCondition.issummary == false) {
                if (($scope.SearchCondition.cardno == undefined ||
                    $scope.SearchCondition.cardno == '') &&
                    ($scope.SearchCondition.mobileno == undefined ||
                        $scope.SearchCondition.mobileno == '') &&
                    ($scope.SearchCondition.customername == undefined ||
                        $scope.SearchCondition.customername == '') && 
                    ($scope.SearchCondition.fromdate == undefined ||
                    $scope.SearchCondition.fromdate == '')) {
                    $rootScope.Toaster.Info("Please enter one minimum condition");
                    return;
                }
            }

            $scope.SearchCondition.loyeltyno = $scope.SearchCondition.cardno;
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            $scope.totalbillvalue = 0;
            $scope.totalreward = 0;
            $scope.SearchResult = [];
            $scope.ViewResult = [];
            var srequest = JSON.stringify(request);
            

            $http({
                method: 'POST',
                url: "CustomerService/SearchCustomerReward",
                data: srequest
            }).success(function (result) {
                if (result != 'undefined' && result.length > 0) {
                    $scope.SearchResult = result;
                    $scope.customername = '';
                    $scope.customerid = 0;
                    $.each($scope.SearchResult, function (k, v) {
                        $scope.totalbillvalue += v.billvalue;
                        $scope.totalreward += (v.loyelty - v.adjusted);
                    });
                    $scope.CustomerConfig();

                    if ($scope.SearchCondition.issummary == false && 
                        result[0].customerid == result[result.length - 1].customerid) {
                        $scope.customername = result[0].customername;
                        $scope.customerid = result[0].customerid;
                        request = {
                            "item": { 'customerid': $scope.customerid },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/SearchRewardBill",
                            data: srequest
                        }).success(function (result) {
                            if (result != 'undefined' && result.length > 0) {
                                $scope.RewardBillResults = result;
                                $scope.ConfigRewardGrid();
                            }
                            else {
                                $scope.RewardBillResults = [];
                                $scope.ConfigRewardGrid();
                            }
                        });
                    }
                }
                else {

                    request = {
                        "item": $scope.SearchCondition,
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "CustomerService/Search",
                        data: srequest
                    }).success(function (result) {
                        if (result != 'undefined' && result.length > 0) {
                            $scope.customername = result[0].name;
                            $scope.customerid = result[0].id;
                        }
                        else {
                            $rootScope.Toaster.Info("No Records found.");
                            $scope.customername = '-';
                            $scope.customerid = 0;
                        }
                    });
                }
            }).error(function (result) {
            });
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.Customer = item;
            }
            else if (action == "Add")
            {
                if (!$rootScope.UserInfo.p_CUSTOMERLOYALTYMGMT.create) {
                    $rootScope.Toaster.Info("No Permission to Add points");
                    return;
                }
                if ($scope.customerid == 0)
                {
                    $rootScope.Toaster.Info("Please Search and Select the Customer");
                    return;
                }
                $scope.ShowSearch = false;
                $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
                $scope.Customer.customerid = $scope.customerid;
                $scope.Customer.remark = "";
                $scope.Customer.billdate = new Date();
            }
            else if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete the selected Item ' + item.billno + "?",
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
                                    url: "CustomerrewardsService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        if (item.selected) $scope.selectedcount--;
                                        $scope.igrid.UpdateData($scope.SearchResult);
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

        }
        $scope.Clear = function () {
            $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchResult = [];
        }

        $scope.Save = function () {

            var request = {
                "Item": $scope.Customer,
                "securitytoken": "String content"
            }

            $http({
                method: 'POST',
                url: "CustomerrewardsService/UpdateRewards",
                data: request
            }).success(function (result) {

                if (result.id > 0 && (result.message == null || result.message != '' || result.message.length == 0)) {
                    $rootScope.Toaster.Info("Customer rewards updated successfully.!");
                    $scope.Customer = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.ShowSearch = true;
                }
                else {
                    $rootScope.Toaster.Error(result.message);
                }
            }).error(function (result) {
            });

        }

        $scope.CustomerConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.SearchResult);

            if ($scope.SearchCondition.issummary) {
                igrid.AddSimpleColumn('customername', 'Customer Name', 140);
                igrid.AddSimpleColumn('mobileno', 'Mobile No', 80);
                igrid.AddSimpleColumn('cityname', 'City', 100);
                igrid.AddSimpleColumn('loyeltyno', 'Loyelty Cardno', 100);
                igrid.AddFormatedColumn('loyelty', 'Loyalty Point', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('adjusted', 'Redeem Point', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('balancepoint', 'Balance Point', 80, 'N', 2).total = true;

            }
            else {
                igrid.AddSimpleColumn('customername', 'Customer Name', 140);
                igrid.AddSimpleColumn('mobileno', 'Mobile No', 100);
                igrid.AddSimpleColumn('billno', 'Bill NO', 120);
                igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd/MM/yyyy');
                igrid.AddFormatedColumn('billvalue', 'Bill Value', 100, 'N', 2).total = true;
                igrid.AddFormatedColumn('loyelty', 'Loyalty Point', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('adjusted', 'Redeem Point', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('expired', 'Expired Point', 80, 'N', 2).total = true;
                igrid.AddFormatedColumn('expiry', 'Expiry On', 80, 'dd/MM/yyyy');
                igrid.AddCustomColumn('hasaddlndiscount', 'Addln Discount', 60, function (item, col) {
                    return item.hasaddlndiscount == true ? "YES" : "NO";
                });
                igrid.AddCustomColumn('hasschemediscount', 'Scheme Discount', 60, function (item, col) {
                    return item.hasschemediscount == true ? "YES" : "NO";
                });
                igrid.AddSimpleColumn('remark', 'Remarks', 100);

                var commands = "";
                if ($rootScope.UserInfo.p_CUSTOMERLOYALTYMGMT.modify)
                    commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
                if ($scope.UserInfo.p_CUSTOMERLOYALTYMGMT.delete)
                    commands += "<input type='button' class='btn btn-info btn-xs' value='Delete'/> &nbsp;";

                if ($rootScope.UserInfo.p_CUSTOMERLOYALTYMGMT.modify) {
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
            }
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'CUSTOMER LOYALTY POINT DETAILS'

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

        $scope.ConfigRewardGrid = function () {
            if ($scope.rgrid != null)
                $scope.rgrid.destroy();

            var igrid = new InfyGrid('rewardgrid', '150px', '100%', false, false, false, 300);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.RewardBillResults);

            igrid.AddSimpleColumn('billno', 'Bill NO', 100);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 80, 'dd/MM/yyyy');
            igrid.AddFormatedColumn('loyelty', 'Adjusted Point', 60, 'N', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.rgrid = igrid;
            $scope.rgrid.UpdateData($scope.RewardBillResults);
        }

        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);