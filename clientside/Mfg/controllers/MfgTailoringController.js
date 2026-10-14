var app = angular.module('InfinityLab');

app.controller('MfgTailoringController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'cntxService', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, cntxService, $filter) {

        $scope.Navigation = $rootScope.GetNavigation();
        $scope.selectedcount = 0;
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = true;
        $scope.ShowPayments = false;
        $scope.SearchResult = [];
        $scope.ErrorMessage = '';
        $scope.mfg_tailoringmode = 0;
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.SearchCondition = {
            companyid: 0,
            designid: 0,
            JobOrderno: "",
            fromdate: null,
            todate: null,
            createdon: new Date()
        }

        $scope.SizeGroupList = [];
        $scope.SizeOrGroupList = [];
        $scope.SizeList = [];


        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
        });

        $scope.SizeGroupList = restService.Get('ReferencelistService/SIZEGROUP', true).then(function (results) {
            $scope.SizeGroupList = results;
        });

        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.AllColourList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.AllColourList = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.JobWorkCharges = restService.Get('ReferencelistService/FULL-JOBCHARGE', true).then(function (results) {
            $scope.JobWorkCharges = results;
        });

        $scope.AllDesignList = [];
        restService.Get('MfgdesignService/SearchAll', true).then(function (results) {
            $scope.AllDesignList = results;
        });

        $scope.StatusList = [
            { id: 'Summary', name: 'Summary' },
            { id: 'Pending', name: 'Pending' },
            { id: 'Paid', name: 'Paid' },
            { id: 'Unpaid', name: 'Unpaid' },
            { id: 'Issue Detail', name: 'Issue Detail' },
            { id: 'Received Detail', name: 'Received Detail' },
            { id: 'Paid Detail', name: 'Paid Detail' },
            { id: 'Report', name: 'Report' }];

        $scope.Entity = {
            "employeeid": 0,
            "rqty": 0,
            "jobchargeid": 0,
            "jobrate": "",
            "jobcharge": "",
            "designid": "",
            colourid: "",
            sleeveid: "",
            sizegroupid: "",
            iqty: "",
            rqty: "",
            paidcharge: "",
            barcode: "",
            workorderid: "",
            jobworkid: "",
            cqty: "",
            sizelist: "",
            "tqty": 0,
            employeeid: "",
            "id": 0,
            "modifiedon": "",
            "createdon": "",
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "isactive": true,
            "issuedon": new Date(),
            productid: "",
            approvedby: ""
        }

        $scope.TailoringOrder = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.SavedList = [];
        $scope.New = function () {
            $scope.TailoringOrder = (JSON.parse(JSON.stringify($scope.Entity)));
        }


        $scope.ProductChanged = function () {
            $scope.DesignList = [];
            if ($scope.TailoringOrder.productid && $scope.TailoringOrder.productid != null) {
                $.each($scope.AllDesignList, function (k, v) {
                    if (v.productid == $scope.TailoringOrder.productid) {
                        $scope.DesignList.push(v);
                    }
                })
                $timeout(function () {
                    $scope.setFocusdesign();
                });
            }
        }

        $scope.ColourChanged = function () {
            var design = $scope.DesignList.find(function (item) {
                return item.id == $scope.TailoringOrder.designid;
            });
            var list = [];
            if ($scope.IsSize) {
                $.each(design.sizegroup.items, function (k, v) {
                    if (v.colourid == $scope.TailoringOrder.colourid) {
                        var size = $scope.SizeList.find(function (item) {
                            return item.id == v.sizeid;
                        });
                        if (size && list.indexOf(size) < 0) {
                            list.push(size);
                        }
                    }
                });

            }
            else {
                var defaultSize = { 'id': 'All Size', 'name': 'All Size', 'list': [] }
                $.each(design.sizegroup.items, function (k, v) {
                    if (v.colourid == $scope.TailoringOrder.colourid) {
                        var size = $scope.SizeGroupList.find(function (item) {
                            return item.name == v.sizegroup;
                        });
                        if (size && list.indexOf(size) < 0) {
                            list.push({ 'id': v.sizegroup, 'name': v.sizegroup, 'list': size.list });
                        }
                        else {
                            var size = defaultSize.list.find(function (item) {
                                return item.id == v.sizeid;
                            });
                            if (size == undefined) {
                                defaultSize.list.push({ 'id': v.sizeid, 'name': $scope.SearchRef(v.sizeid, $scope.SizeList) });
                            }
                        }
                    }
                });

                if (list.length == 0) {
                    list.push(defaultSize);
                    $.each($scope.SizeGroupList, function (k, v) {
                        list.push(v);
                    });
                }
            }

            $scope.SizeOrGroupList = list;
            $timeout(function () {
                $scope.setFocussizegroup();
            });
        }
        $scope.Designchanged = function () {
            var design = $scope.DesignList.find(function (item) {
                return item.id == $scope.TailoringOrder.designid;
            });
            var list = [];
            var clist = [];

            $.each(design.sizegroup.items, function (k, v) {
                var colour = clist.find(function (item) {
                    return item.id == v.colourid;
                });

                if (typeof (colour) == 'undefined') {
                    clist.push({ 'id': v.colourid, 'name': $scope.SearchRef(v.colourid, $scope.AllColourList) });
                }
            });

            $scope.SizeOrGroupList = list;
            $scope.ColourList = clist;
            $timeout(function () {
                $scope.setFocussizecolour();
            });
        }

        $scope.Save = function () {

            if (typeof ($scope.TailoringOrder.employeeid) == 'undefined' || $scope.TailoringOrder.employeeid == null || $scope.TailoringOrder.employeeid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info("Please Select Employee Name");
                return false;
            }

            if ($scope.TailoringOrder.approvedby == null || $scope.TailoringOrder.approvedby == "") {
                $scope['setFocusapprovedby']();
                $rootScope.Toaster.Info("Please Select Given Person Name");
                return false;
            }

            $scope.TailoringOrder.jobrate = 0;
            if ($scope.TailoringOrder.jobchargeid &&
                $scope.TailoringOrder.jobchargeid != null) {

                var jobchargeconfig = $scope.JobWorkCharges.find(function (item) {
                    return item.id == $scope.TailoringOrder.jobchargeid;
                });
                if (jobchargeconfig && jobchargeconfig != null)
                    $scope.TailoringOrder.jobrate = jobchargeconfig.additionalinfo.mfgconfig.charge;
            }

            if ($scope.mfg_tailoringmode == 0) {


                var design = $scope.DesignList.find(function (item) {
                    return item.id == $scope.TailoringOrder.designid;
                });
                if ($scope.IsSize) {
                    var size = $scope.SizeList.find(function (item) {
                        return item.id == $scope.TailoringOrder.sizeorgroupid;
                    });
                    $scope.TailoringOrder.sizeid = size.id;
                    $scope.TailoringOrder.sizename = size.name;

                    jobsize = design.sizegroup.items.find(function (item) {
                        return item.sizeid == size.id
                    });

                    if ($scope.TailoringOrder.jobchargeid &&
                        $scope.TailoringOrder.jobchargeid != null) {

                        var jobcharge = $scope.JobWorkCharges.find(function (item) {
                            return item.id == $scope.TailoringOrder.jobchargeid;
                        });

                        if (jobcharge && jobcharge != null && jobcharge.additionalinfo != null && jobcharge.additionalinfo.mfgconfig) {
                            $scope.TailoringOrder.jobrate = jobcharge.additionalinfo.mfgconfig.charge;
                            $scope.TailoringOrder.sizelist = [{ 'id': size.id, 'name': size.name, 'jobcharge': jobcharge.additionalinfo.mfgconfig.charge }];
                        }
                    }
                    else {
                        $scope.TailoringOrder.sizelist = [{ 'id': size.id, 'name': size.name, 'jobcharge': jobsize.jobcharge }];
                    }
                }
                else {

                    var size = $scope.SizeOrGroupList.find(function (item) {
                        return item.id == $scope.TailoringOrder.sizeorgroupid;
                    });
                    $scope.TailoringOrder.sizegroupid = size.id;
                    $scope.TailoringOrder.sizegroupname = size.name;
                    $scope.TailoringOrder.sizelist = size.list;

                    if ($scope.TailoringOrder.jobchargeid &&
                        $scope.TailoringOrder.jobchargeid != null) {

                        var jobcharge = $scope.JobWorkCharges.find(function (item) {
                            return item.id == $scope.TailoringOrder.jobchargeid;
                        });

                        if (jobcharge && jobcharge != null) {
                            $scope.TailoringOrder.jobrate = jobcharge.additionalinfo.mfgconfig.charge;
                            $.each(size.list, function (kk, vv) {
                                vv.jobcharge = jobcharge.additionalinfo.mfgconfig.charge;
                            });
                        }
                        else {
                            $.each(size.list, function (kk, vv) {
                                var jobcharge = design.sizegroup.items.find(function (item) {
                                    return item.sizeid == vv.id && item.sizegroup == size.name;
                                });
                                if (jobcharge && jobcharge != null)
                                    vv.jobcharge = jobcharge.jobcharge;
                            });
                        }
                    }
                    else {
                        $.each(size.list, function (kk, vv) {
                            var jobcharge = design.sizegroup.items.find(function (item) {
                                return item.sizeid == vv.id && item.sizegroup == size.name;
                            });
                            if (jobcharge && jobcharge != null)
                                vv.jobcharge = jobcharge.jobcharge;
                        });
                    }
                }
            }

            var qty = parseFloat($scope.TailoringOrder.qty);
            if (isNaN(qty)) qty = 0;
            var totalqty = 0;
            var jobcharge = 0;
            if ($scope.TailoringOrder.sizelist && $scope.TailoringOrder.sizelist != null) {
                $.each($scope.TailoringOrder.sizelist, function (k, v) {
                    v.iqty = qty;
                    totalqty += v.iqty;
                    jobcharge += v.iqty * v.jobcharge;
                })
                $scope.TailoringOrder.rqty = totalqty;
            }
            else {
                jobcharge = qty * $scope.TailoringOrder.jobrate;
                totalqty = qty;
                $scope.TailoringOrder.rqty = totalqty;
                $scope.TailoringOrder.receivedon = $scope.TailoringOrder.issuedon;
            }

            $scope.TailoringOrder.jobcharge = jobcharge;
            $scope.TailoringOrder.iqty = totalqty;
            var request = {
                "Item": $scope.TailoringOrder,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgtailoringService/Save",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $scope.SavedList.push(result);
                    $scope.OrderConfig();
                    $rootScope.Toaster.Info("Saved Successfully.!");
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }


        $scope.EmployeeOrdersByDate = function () {
            if ($scope.TailoringOrder.issuedon && $scope.TailoringOrder.issuedon != null) {
                var request = {
                    "Item": {
                        'fromdate': $scope.TailoringOrder.issuedon,
                        'todate': $scope.TailoringOrder.issuedon,
                        'showpending': true
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "MfgtailoringService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.SavedList = [];
                    $scope.SavedList = $scope.SavedList.concat(result);
                    $scope.OrderConfig();
                })
            }
        }

        $scope.EmployeeOrders = function () {
            var request = {
                "Item": {
                    'employeeid': $scope.TailoringOrder.employeeid,
                    'fromdate': $scope.SearchCondition.fromdate,
                    'todate': $scope.SearchCondition.todate,
                    'showpending': true
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgtailoringService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SavedList = [];
                $scope.SavedList = $scope.SavedList.concat(result);
                $scope.OrderConfig();
            })
        }

        $scope.PrintRequest = {};
        $scope.SearchOrders = function () {
            $scope.selectedcount = 0;
            $scope.ShowPayments = false;
            var isDetail = false;
            var url = "MfgtailoringService/SearchSummary";
            if ($scope.SearchCondition.status == 'Issue Detail' ||
                $scope.SearchCondition.status == 'Receive Detail') {
                url = "MfgtailoringService/Search";
                isDetail = true;
            } else if ($scope.SearchCondition.status == 'Paid Detail') {
                url = "MfgtailoringService/Search";
            }
            else if ($scope.SearchCondition.status == 'Report') {
                url = "MfgtailoringService/Report";
                $scope.PrintRequest.f = $scope.SearchCondition.fromdate;
                $scope.PrintRequest.t = $scope.SearchCondition.todate;
                $scope.PrintRequest.p = $scope.SearchCondition.productid;
                $scope.PrintRequest.d = $scope.SearchCondition.designid;
                $scope.PrintRequest.e = $scope.SearchCondition.employeeid;
            }
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: url,
                data: srequest
            }).success(function (result) {
                if (isDetail || $scope.SearchCondition.status == 'Report') {
                    $scope.SavedList = result;
                    $scope.OrderConfig($scope.SearchCondition.status == 'Report');
                    result = [];
                }
                $scope.SearchResult = result;
                $scope.SearchConfig();
            })
        }

        $scope.DeleteSelected = function () {
            var ids = [];
            var items = [];
            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                ids.push(v.id);
                items.push(v);
            });
            if (ids.length == 0) return;

            var request = {
                'Item': { 'ids': ids }
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgtailoringService/Delete",
                data: srequest
            }).success(function (result) {
                $.each(items, function (k, item) {
                    $scope.SavedList.splice($scope.SavedList.indexOf(item), 1);
                })
                $scope.igrid.UpdateData($scope.SavedList);
                $scope.selectedcount = 0;
                $scope.Navigation.delete = false;
            }).error(function (result) {
            });
        }

        $scope.ManageOrder = function (action, item) {
            if (action == 'ReceivedItem') {
                var request = {
                    "Item": item,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "MfgtailoringService/Update",
                    data: srequest
                }).success(function (result) {
                    $scope.OrderConfig();

                })
            }
        }

        $scope.OrderConfig = function (isreport) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '350px', '100%', true, true, true, $scope.mfg_tailoringmode == 0 ? 100 : 50);
            igrid.options.showHeaderRow = true;

            if ($scope.mfg_tailoringmode == 0) {
                igrid.AddFormatedColumn('issuedon', 'Issued On', 80, 'dd-MM-yyyy');

                var column = igrid.AddCustomColumn('receivedon', 'Received On', 80, function (item, col) {
                    var value = $filter('date')(item.receivedon, 'dd-MM-yyyy');
                    return value == '01-01-0001' ? '' : value;
                });

                igrid.AddReferenceColumn('productid', 'Product', 120, $scope.Products);
                igrid.AddReferenceColumn('designid', 'Design No', 60, $scope.DesignList);

                var column = igrid.AddCustomColumn('display', 'Size/Group', 100, function (item, col) {
                    if (item.sizeid > 0)
                        return item.sizename;
                    return item.sizegroupname;
                });

                igrid.AddReferenceColumn('colourid', 'Colour', 120, $scope.AllColourList);
                igrid.AddSimpleColumn('iqty', 'Issued', 70);
                igrid.AddSimpleColumn('rqty', 'Received', 70);
                var commands = "<input type='button' class='btn btn-danger btn-xs' value='Cancel'/> &nbsp;"
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Complete'/> &nbsp;"
                commands += "<input type='button' class='btn btn-secondary btn-xs' value='Part'/> &nbsp;"

                igrid.AddButtonColumn('action', 'ACTION', 180, commands, function (e, args) {
                    $scope.$apply(function () {
                        $scope.ReceiveStock($(e.target).val(), args.item);
                    });
                });

            }
            else {
                igrid.AddFormatedColumn('issuedon', 'Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('remarks', 'Lot#', 70);
                igrid.AddReferenceColumn('approvedby', 'Recorded By', 120, $scope.Employee);
                igrid.AddReferenceColumn('employeeid', 'Employee', 120, $scope.Employee);
                if (isreport) {
                    

                    var column = igrid.AddColumn(
                        {
                            id: 'section', name: 'Section', field: "section",
                            width: 50, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    );

                    column.getValue = function (item, col) {
                        var employ = $scope.Employee.find(function (sitem) {
                            return sitem.id == item.employeeid;
                        });
                        if (employ && employ != null) {
                            var section = $scope.Sections.find(function (item) {
                                return item.id == employ.productgroup;
                            });
                            if (section && section != null) {
                                return section.name;
                            }
                        }
                        return '';
                    };

                    column.formatter = function (row, cell, searchValue, columnDef, dataContext) {
                        var employ = $scope.Employee.find(function (item) {
                            return item.id == dataContext.employeeid;
                        });
                        if (employ && employ != null) {
                            var section = $scope.Sections.find(function (item) {
                                return item.id == employ.productgroup;
                            });
                            if (section && section != null) {
                                return section.name;
                            }
                        }
                        return '';
                    }

                }
                igrid.AddReferenceColumn('jobchargeid', 'Job Work', 120, $scope.JobWorkCharges);
                igrid.AddSimpleColumn('iqty', 'Qty', 70);
                igrid.AddSimpleColumn('jobrate', 'Rate', 70);
                igrid.AddSimpleColumn('jobcharge', 'Wages', 70);
                igrid.AddSimpleColumn('paidcharge', 'Paid', 70);
                igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }


            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                    $scope.Navigation.delete = range.length > 0;
                }, 10);
            };
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command != 'Delete') return;

                var request = {
                    "Item": item,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "MfgtailoringService/Delete",
                    data: srequest
                }).success(function (result) {
                    
                        $scope.SavedList.splice($scope.SavedList.indexOf(item), 1);
                        $scope.igrid.UpdateData($scope.SavedList);
                    
                })
            });
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SavedList);
        }

        $scope.exporttoExcel = function (infygrid, filename) {
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, filename);
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

        $scope.PayValidation = function () {
            var paylist = $scope.sgrid.GetSelectedRows();
            $.each(paylist, function (k, v) {
                v.paidamount = v.jobcharge - v.paidcharge;
            })
            if (parseFloat($scope.TailoringOrder.paidamount) == $scope.TailoringOrder.payableamount) {
                $scope.Pay(paylist);
            }
            else {
                var dialog = ngDialog.open({
                    template: 'TailoringPayment',
                    data: {
                        'SearchResult': paylist,
                        'Employee': $scope.Employee,
                        'SearchRef': $scope.SearchRef
                    },
                    width: '600px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.SearchResult = $scope.ngDialogData.SearchResult;
                        $scope.Employee = $scope.ngDialogData.Employee;
                        $scope.SearchRef = $scope.ngDialogData.SearchRef;
                        $scope.PaidAmount = 0;

                        $scope.PaidChanged = function () {
                            $scope.PaidAmount = 0;
                            $.each($scope.SearchResult, function (k, v) {
                                $scope.PaidAmount += parseFloat(v.paidamount);
                            });
                        }
                        $scope.PaidChanged();

                        $scope.Save = function () {
                            $scope.closeThisDialog({ 'paymentlist': $scope.SearchResult });
                        };
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                            && data.value != '$document') {
                            $scope.Pay(data.value.paymentlist);
                        }
                    }
                });
            }
        }

        $scope.Pay = function (paymentlist) {
            var payment = {
                'paidamount': $scope.TailoringOrder.paidamount,
                'paidon': $scope.TailoringOrder.paidon,
                'fromdate': $scope.SearchCondition.fromdate,
                'todate': $scope.SearchCondition.todate,
                'items': paymentlist
            }
            var request = {
                "Item": payment,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgtailoringService/Payment",
                data: srequest
            }).success(function (result) {
                $scope.sgrid.SetSelectedRows([]);
                $scope.SearchOrders();
            })
        }

        $scope.paymodechanged = function () {
            if ($scope.TailoringOrder.paymode == 'full')
                $scope.TailoringOrder.paidamount = $scope.TailoringOrder.payableamount;
            else
                $scope.TailoringOrder.paidamount = 0;
        }


        $scope.SearchConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();
            var sgrid = new InfyGrid('searchgridContainer', '350px', '100%', true, true, true, 200);
            sgrid.options.showHeaderRow = true;
            sgrid.AddReferenceColumn('employeeid', 'Tailor Name', 135, $scope.Employee);
            if ($scope.SearchCondition.status == 'Unpaid') {
                sgrid.AddSimpleColumn('Qty', 'Received', 80);
                sgrid.AddSimpleColumn('jobcharge', 'Charge', 80);
                var column = sgrid.AddCustomColumn('pending', 'Pending', 100, function (item, col) {
                    return item.jobcharge - item.paidcharge;
                });
                sgrid.onSelectedRangesChanged = function (range, rowmodel) {
                    $timeout(function () {
                        $scope.ShowPayments = range.length > 0;
                        $scope.TailoringOrder.paidon = new Date();
                        //$filter('date')(new Date(), 'dd-MM-yyyy');
                        $scope.TailoringOrder.paidamount = 0;
                        $scope.TailoringOrder.paymode = "full";
                        $.each($scope.sgrid.GetSelectedRows(), function (k, v) {
                            $scope.TailoringOrder.paidamount += v.jobcharge - v.paidcharge;
                        });
                        $scope.TailoringOrder.payableamount = $scope.TailoringOrder.paidamount;
                    }, 10);
                };
            }
            else if ($scope.SearchCondition.status == 'Paid') {
                sgrid.AddSimpleColumn('rqty', 'Received', 80);
                sgrid.AddSimpleColumn('jobcharge', 'Charge', 80);
                sgrid.AddSimpleColumn('paidcharge', 'Paid', 80);
            }
            else if ($scope.SearchCondition.status == 'Pending') {
                sgrid.AddSimpleColumn('iqty', 'Issued', 80);
                sgrid.AddSimpleColumn('rqty', 'Received', 80);
                var column = sgrid.AddCustomColumn('pending', 'Pending', 100, function (item, col) {
                    return item.iqty - item.rqty;
                });;
            }
            else {
                sgrid.AddSimpleColumn('iqty', 'Issued', 80);
                sgrid.AddSimpleColumn('rqty', 'Received', 80);
                sgrid.AddSimpleColumn('jobcharge', 'Charge', 80);
                sgrid.AddSimpleColumn('paidcharge', 'Paid', 80);
            }


            sgrid.Initialize();
            $scope.sgrid = sgrid;
            $scope.sgrid.UpdateData($scope.SearchResult);
        }



        $scope.UpdateCharges = function () {
            var request = {
                "Item": {},
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgtailoringService/UpdateCharge",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Charge Changed Successfully.!");
            })
        }

        $scope.ReceiveStock = function (action, item) {

            if (action == 'Complete') {
                $rootScope.Prompt('Received Qty', '', function (rqty) {
                    rqty = parseFloat(rqty);
                    if (item.iqty - item.cqty == rqty) {
                        var receivedlist = [];
                        item.rqty += item.iqty - (item.rqty + item.cqty);
                        $.each(item.sizelist, function (k, v) {
                            receivedlist.push({
                                'mfgtailoringid': item.id,
                                'employeeid': item.employeeid,
                                'qty': v.iqty - (v.rqty + v.cqty),
                                'receivedon': new Date(),
                                'isactive': true,
                                'version': 1,
                                'sizeid': v.id,
                                'sizename': v.name,
                                'sizegroupname': item.sizegroupname
                            });
                            v.rqty += v.iqty - (v.rqty + v.cqty);
                        });
                        item.receivedon = new Date();
                        var request = {
                            "Item": { 'item': item, 'recieved': receivedlist },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "MfgtailoringService/Received",
                            data: srequest
                        }).success(function (result) {
                            item.version = result.item.version;
                            item.modifiedon = result.item.modifiedon;
                            item.modifiedby = result.item.modifiedby;
                            $scope.igrid.UpdateData($scope.SavedList);
                        })
                    } else {
                        $scope.ReceiveStock('Part', item);
                    }
                });
            }
            else if (action == 'Cancel') {
                var receivedlist = [];
                item.cqty = item.iqty - item.rqty;
                $.each(item.sizelist, function (k, v) {
                    receivedlist.push({
                        'mfgtailoringid': item.id,
                        'employeeid': item.employeeid,
                        'cqty': v.iqty - (v.rqty + v.cqty),
                        'receivedon': new Date(),
                        'isactive': true,
                        'version': 1,
                        'sizeid': v.id,
                        'sizename': v.name,
                        'sizegroupname': item.sizegroupname
                    });
                    v.cqty += v.iqty - (v.rqty + v.cqty);
                });
                item.receivedon = new Date();

                var request = {
                    "Item": { 'item': item, 'recieved': receivedlist },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "MfgtailoringService/Received",
                    data: srequest
                }).success(function (result) {
                    item.version = result.item.version;
                    item.modifiedon = result.item.modifiedon;
                    item.modifiedby = result.item.modifiedby;
                    $scope.igrid.UpdateData($scope.SavedList);
                })

            } else if (action == "Part") {
                var dialog = ngDialog.open({
                    template: 'TailoringRecieved',
                    data: {
                        'Item': item,
                        'Employee': $scope.Employee,
                        'SearchRef': $scope.SearchRef
                    },
                    width: '600px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.Item = $scope.ngDialogData.Item;
                        $scope.Employee = $scope.ngDialogData.Employee;
                        $scope.SearchRef = $scope.ngDialogData.SearchRef;
                        $scope.TotalQty = 0;
                        $scope.allqty = 0;

                        $.each($scope.Item.sizelist, function (k, v) {
                            v.nrqty = (v.iqty - (v.cqty + v.rqty));
                            $scope.TotalQty += v.nrqty;
                        });

                        $scope.AllQtyChanged = function () {
                            $scope.TotalQty = 0;
                            $.each($scope.Item.sizelist, function (k, v) {
                                v.nrqty = $scope.allqty;
                                $scope.TotalQty += v.nrqty;
                            });
                        }

                        $scope.QtyChanged = function () {
                            $scope.TotalQty = 0;
                            $.each($scope.Item.sizelist, function (k, v) {
                                $scope.TotalQty += parseFloat(v.nrqty);
                            });
                        }

                        $scope.Save = function () {
                            $scope.closeThisDialog({ 'Item': $scope.Item });
                        };

                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                            && data.value != '$document') {
                            var item = data.value.Item;
                            item.receivedon = new Date();
                            var receivedlist = [];
                            $.each(item.sizelist, function (k, v) {
                                item.rqty += parseFloat(v.nrqty);
                                v.rqty += parseFloat(v.nrqty);
                                receivedlist.push({
                                    'mfgtailoringid': item.id,
                                    'employeeid': item.employeeid,
                                    'qty': parseFloat(v.nrqty),
                                    'receivedon': new Date(),
                                    'isactive': true,
                                    'version': 1,
                                    'sizeid': v.id,
                                    'sizename': v.name,
                                    'sizegroupname': item.sizegroupname
                                });
                            });
                            var request = {
                                "Item": {
                                    'item': item,
                                    'recieved': receivedlist
                                },
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "MfgtailoringService/Received",
                                data: srequest
                            }).success(function (result) {
                                item.version = result.item.version;
                                item.modifiedon = result.item.modifiedon;
                                item.modifiedby = result.item.modifiedby;
                                $scope.igrid.UpdateData($scope.SavedList);
                            })
                        }
                    }
                });
            }
        }

        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings) {
                $scope.mfg_tailoringmode = $rootScope.UserInfo.appsettings.mfg_tailoringmode;
                $scope.OrderConfig();
                $scope.SearchCondition.fromdate = $filter('date')(new Date(), datePicker.shortDisplay);
                $scope.SearchCondition.todate = $filter('date')(new Date(), datePicker.shortDisplay);
                $scope.SearchCondition.status = 'Report';
                $scope.SearchOrders();
            } else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 10);
            }
        }

        $timeout(function () {
            $scope.LoadAppSetting();
        }, 10);



        
        $scope.PrintPDF = function () {
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "po.pdf";
                link.href = 'MfgtailoringService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + Base64.encode(JSON.stringify($scope.PrintRequest));
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.PrintSettlement = function () {

            $scope.TailoringItems = [];

            var selected = $scope.igrid.GetSelectedRows();

            request = {
                "item": {
                    employeeid: selected[0].employeeid,
                    designid: selected[0].designid,
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgtailoringService/PrintSlip",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    $scope.TailoringItems = result;
                    $scope.PrintTailorSlip();
                }
            }).error(function (result) {
            });
        }

        $scope.PrintTailorSlip = function () {
            var list = [];

            list.push($rootScope.PrintFormater.Bold("         " + $rootScope.UserInfo.CompanyName));
            list.push($rootScope.PrintFormater.Bold("           TAILOR'S SLIP"));
            list.push($rootScope.PrintFormater.Normal("    __________________________________________"));

            list.push($rootScope.PrintFormater.Bold("    ISSUED DATE: " + $filter('date')($scope.TailoringItems[0].issuedon, 'dd-MM-yyyy')));
            list.push($rootScope.PrintFormater.Bold("           TIME: " + $filter('displayMinuteAsTime')($scope.TailoringItems[0].issuedon)));

            list.push($rootScope.PrintFormater.Bold("     MACHINE NO: " + $scope.TailoringItems[0].employeecode));
            list.push($rootScope.PrintFormater.Bold("    TAILOR NAME: " + $scope.SearchRef($scope.TailoringItems[0].employeeid, $scope.Employee)));
            list.push($rootScope.PrintFormater.Bold("    __________________________________________"));

            var tline = [];
            var mline = [];
            var bline = [];
            list.push($rootScope.PrintFormater.Normal("          DESIGN        : " + $scope.SearchRef($scope.TailoringItems[0].designid, $scope.DesignList)));
            list.push($rootScope.PrintFormater.Normal("          SIZE SET      : " + $scope.SearchRef($scope.TailoringItems[0].sizegroupid, $scope.SizeGroupList)));
            list.push($rootScope.PrintFormater.Normal("          COLOUR        : " + $scope.TailoringItems[0].colourname));
            list.push($rootScope.PrintFormater.Normal("          PRODUCT       : " + $scope.SearchRef($scope.TailoringItems[0].productid, $scope.Products)));

            list.push($rootScope.PrintFormater.Normal("          TOTAL QTY     : " + $scope.TailoringItems[0].iqty + " PIECES"));

            list.push("\r\n");
            list.push("\r\n");
            list.push("\r\n");

            list.push($rootScope.PrintFormater.Bold("      APPROVED BY: " + $scope.SearchRef($scope.TailoringItems[0].approvedby, $scope.Employee)));

            list.push($rootScope.PrintFormater.Cutter());
            list.push("\r\n");

            var printdocument = window.btoa(list.join('\r\n'));
            $rootScope.Sent2Printer(printdocument);
        }

        $scope.ChangeMode = function () {
            $scope.Designchanged();
        }

    }]);
