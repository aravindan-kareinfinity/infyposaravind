
var app = angular.module('InfinityLab');

app.controller('JobworkSampleController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'Excel', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, Excel, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;

        $scope.StatusList = [{ 'id': 0, 'name': 'Received' },
        { 'id': 1, 'name': 'WIP' },
        { 'id': 2, 'name': 'Partialy Delivered' },
        { 'id': 3, 'name': 'Fully Delivered' },
        { 'id': 4, 'name': 'Partialy Cancelled' },
        { 'id': 5, 'name': 'Fully Cancelled' },
        { 'id': 6, 'name': 'Returneded' }];

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.CourierList = restService.Get('ReferencelistService/COURIERONLY', true).then(function (results) {
            $scope.CourierList = results;
        });

        $scope.B2BCustomers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.B2BCustomers = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        // model to be used for Courier
        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 0,
            "createdon": "",
            "modifiedon": "",
            "receivingcompanyid": 0,
            "companyid": 0,
            "receivedon": null,
            "senton": null,
            "type": 0,
            "islr": false,
            "podnumber": null,
            "awbnumber": null,
            "supplierid": 0,
            "status": 100,
            "receivedby": 0,
            "deliveredto": 0,
            "version": "1",
            "attachment": [],
            "agentid": 0,
            isbillcopy: "",
            lrnos: "",
            invnos: "",
            entryno: "",
            copybills: ""
        };
        $scope.JobworkSample = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.ErrorMessage = "";

        $scope.AddSlide = function () {
            if ($scope.JobworkSample.attachment == 'undefined' || $scope.JobworkSample.attachment == null)
                $scope.JobworkSample.attachment = [];
            $scope.JobworkSample.attachment.push($scope.Slide);
            $scope.Slide = { 'title': '', 'image': '' };
        };
        $scope.ManagesearchSlideItem = function (action, item) {
            if (action == "DeleteSlide") {
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
                                $scope.JobworkSample.attachment.splice($scope.JobworkSample.attachment.indexOf(item), 1);
                                $scope.$apply();
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
            else if (action == "EditSlide") {
                $scope.Slide = item;
            }
        }

        $scope.SearchCondition = {
            companyid: "",
            type: -9,
            supplierid: "",
            status: "",
            awbnumber: "",
            podnumber: "",
            fromdate: null,
            todate: null
        }


        $scope.JobworkSampleList = [];

        $scope.Save = function () {


            if ($scope.JobworkSample.receivedcouriercompanyid == null || $scope.JobworkSample.receivedcouriercompanyid == 0) {
                $rootScope.Toaster.Error("Please select Courier");
                return;
            }


            if ($scope.JobworkSample.customerid == null || $scope.JobworkSample.customerid == 0) {
                $rootScope.Toaster.Error("Please select Customer");
                return;
            }
            //if ($scope.JobworkSample.receivedby == null || $scope.JobworkSample.receivedby == 0) {
            //    $scope['setFocusreceivedby']();
            //    $rootScope.Toaster.Error("Please select receivedby");
            //    return;
            //}

            var request = {
                "Item": $scope.JobworkSample,
                "securitytoken": "String content"
            }

            var isedit = typeof ($scope.JobworkSample.id) != 'undefined' && $scope.JobworkSample.id > 0;

            $http({
                method: 'POST',
                url: "JobworksamplesService/Save",
                data: request
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Courier Saved Successfully.!");
                    if (!isedit) {
                        $scope.JobworkSampleList.push(result);
                        if ($scope.JobworkSampleList.length == 1) {
                            $scope.JobworkSampleEditConfig();
                        }
                    }
                    else {
                        $scope.JobworkSampleList.splice($scope.JobworkSampleList.indexOf($scope.JobworkSample), 1);
                        $scope.JobworkSampleList.push(result);
                        $scope.JobworkSample = result;
                    }
                    $scope.egrid.UpdateData($scope.JobworkSampleList);
                    $scope.JobworkSample = (JSON.parse(JSON.stringify($scope.Entity)));
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
                    content: 'Do you want to delete ' + item.awbnumber + "?",
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
                                    url: "JobworksamplesService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        if ($scope.JobworkSample &&
                                            $scope.JobworkSample.id &&
                                            $scope.JobworkSample.id == item.id) {
                                            $scope.JobworkSample.id = 0;
                                            $scope.JobworkSample.version = 1;
                                        }
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.JobworkSampleList.splice($scope.JobworkSampleList.indexOf(item), 1);
                                        $scope.egrid.UpdateData($scope.JobworkSampleList);
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
                $scope.JobworkSample = item;
            } else if (action == 'Status') {
                $scope.ShowCustomerDialog(item);
            }
        }
        $scope.Search = function (id) {

            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            if (typeof (id) != 'undefined')
                request.item = { 'id': id };

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "JobworksamplesService/Search",
                data: srequest
            }).success(function (result) {
                if (typeof (id) != 'undefined' && result.length == 1) {
                    $scope.ShowSearch = false;
                    $scope.JobworkSample = result[0];
                }
                else {
                    if ($scope.JobworkSampleList.length == 0) {
                        $scope.JobworkSampleEditConfig();
                    }
                    $scope.JobworkSampleList = result;
                }
                $scope.egrid.UpdateData($scope.JobworkSampleList);
            }).error(function (result) {
            });
        }

        $scope.JobworkSampleEditConfig = function () {
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sampleno', 'Sample No', 100);
            igrid.AddReferenceColumn('samplestatus', 'Status', 100, $scope.StatusList);
            igrid.AddReferenceColumn('customerid', 'Customer', 120, $scope.B2BCustomers);
            igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Products);
            igrid.AddFormatedColumn('receivedon', 'Received On', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expecteddelivery', 'Expected', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('sampleqty', 'Samples', 100, 'N0', 0);
            igrid.AddFormatedColumn('orderqty', 'Orders', 100, 'N0', 0);
            igrid.AddFormatedColumn('purchaseprice', 'Cost', 100, 'N2', 2);
            igrid.AddFormatedColumn('sellingprice', 'Price', 100, 'N2', 2);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Status'/> &nbsp;"
            if ($rootScope.UserInfo.p_JOBWORKSAMPLE.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_JOBWORKSAMPLE.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_JOBWORKSAMPLE.modify || $rootScope.UserInfo.p_JOBWORKSAMPLE.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 150, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Status')
                            $scope.ManageSearchItem('Status', args.item);
                    });
                });
            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.egrid = igrid;
            $scope.egrid.UpdateData([]);
        }

        $timeout(function () {
            if (typeof ($location.search().id) != 'undefined') {
                $scope.Search($location.search().id);
            };

            //$scope.setFocusfirst();
        }, 10);

        $scope.ShowCustomerDialog = function (item) {


            var dialog = ngDialog.open({
                template: 'ChangeStatus',
                data: [$scope, item],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.JobworkSample = (JSON.parse(JSON.stringify($scope.ngDialogData[1])));
                    $scope.StatusList = $scope.ngDialogData[0].StatusList;
                    $scope.SearchRef = $scope.ngDialogData[0].SearchRef
                    $scope.samplestatus = $scope.JobworkSample.samplestatus;
                    $scope.customername = $scope.ngDialogData[0].B2BCustomers.find(function (item) {
                        return item.id == $scope.JobworkSample.customerid;
                    }).name;

                    $scope.ShowChangeStatus = true;
                    $scope.AddRemark = function () {
                        $scope.ShowChangeStatus = false;
                        if ($scope.JobworkSample.communication == null)
                            $scope.JobworkSample.communication = { 'remark': '', list: [] };
                        else if ($scope.JobworkSample.communication.list == null)
                            $scope.JobworkSample.communication.list = [];

                        $scope.JobworkSample.communication.list.push({
                            'addedon': new Date(),
                            'remark': $scope.remarks,
                            'status': $scope.samplestatus
                        });
                    }
                    $scope.ChangeStatus = function () {
                        $scope.JobworkSample.samplestatus = $scope.samplestatus;
                        $scope.closeThisDialog($scope.JobworkSample);
                    }
                    $scope.Select = function () {
                        $scope.closeThisDialog($scope.JobworkSample);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {


                        var request = {
                            "Item": data.value,
                            "securitytoken": "String content"
                        }

                        $http({
                            method: 'POST',
                            url: "JobworksamplesService/Save",
                            data: request
                        }).success(function (result) {
                            if ($scope.JobworkSample &&
                                $scope.JobworkSample.id &&
                                $scope.JobworkSample.id == result.id) {
                                $scope.JobworkSample = result;
                            }
                            $scope.JobworkSampleList.splice($scope.JobworkSampleList.indexOf(request.Item), 1);
                            $scope.JobworkSampleList.push(result);
                            $scope.egrid.UpdateData($scope.JobworkSampleList);
                        });
                    }
                }
            });
        }

        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };

        //$scope.exporttoExcel = function (tableid, title) {
        //    $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'COURIER ENTRY REPORT');
        //    if ($scope.exportHref != null) {
        //        $timeout(function () {
        //            var link = document.createElement('a');
        //            link.download = filename + ".xls";
        //            link.href = $scope.exportHref;
        //            document.body.appendChild(link);
        //            link.click();
        //            link.remove();
        //        }, 100);
        //    }
        //}

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            subtitle = 'COURIER REPORT ';
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

        $scope.DefaultSettings = {
            "customer": false,
            "collectweight": false,
            "couriercompanyid": 0,
            "name": true,
            "weight": '',
            "inward": false,
            "payment": '',
            "billcopy": false,
            "detail": false,
            "agent": false,
            "customerb2b": false,
            "invoiceno": false,
            "rate": '',
            "collectpayment": false,
            "address": false,
            "supplier": false,
            "awbewb": false,
            "collectrate": false,
            "outward": true,
            "pod": false
        };
        $scope.JobworkSampleSetting = $scope.DefaultSettings;
        $scope.ChangeConfiguration = function () {
            if (typeof ($scope.JobworkSample.type) == 'undefined' || $scope.JobworkSample.type == -9) {
                $scope.JobworkSampleSetting = $scope.DefaultSettings;
            }
            else {
                var couriertype = $scope.TypesofJobworkSample.find(function (item) {
                    return item.id == $scope.JobworkSample.type
                });
                if (couriertype && couriertype != null) {
                    $scope.JobworkSampleSetting = couriertype.additionalinfo.courier
                }
                else {
                    $scope.JobworkSampleSetting = $scope.DefaultSettings;
                }
            }
            if (!($scope.JobworkSampleSetting.inward && $scope.JobworkSampleSetting.outward))
                $scope.JobworkSample.mode = $scope.JobworkSampleSetting.inward ? 1 : 2;

            $scope.JobworkSample.companyid = $scope.JobworkSampleSetting.couriercompanyid;
            $scope.JobworkSample.weight = $scope.JobworkSampleSetting.weight;
            $scope.JobworkSample.rate = $scope.JobworkSampleSetting.rate;
            $scope.JobworkSample.amount = $scope.JobworkSampleSetting.payment;
        }

        $scope.CreateNew = function () {
            $scope.JobworkSample.Receivedon = new Date();
            $scope.JobworkSample.SentOn = new Date();
            $scope.JobworkSampleEditConfig();
        }();

    }]);


