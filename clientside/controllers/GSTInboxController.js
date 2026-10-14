
var app = angular.module('InfinityLab');

app.controller('GSTInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.panelheight = { 'height': '500px' };

        $scope.Months = [{ 'id': '01', 'name': 'Jan' },
        { 'id': '02', 'name': 'Feb' }, { 'id': '03', 'name': 'Mar' }, { 'id': '04', 'name': 'Apr' },
        { 'id': '05', 'name': 'May' }, { 'id': '06', 'name': 'June' }, { 'id': '07', 'name': 'July' },
        { 'id': '08', 'name': 'Augest' }, { 'id': '09', 'name': 'September' }, { 'id': '10', 'name': 'October' },
        { 'id': '11', 'name': 'November' }, { 'id': '12', 'name': 'December' } ];

        $scope.Modes = [{ 'id': 1, 'name': 'GSTR - 1' }, { 'id': 2, 'name': 'GSTR - 2A' },
            { 'id': 3, 'name': 'GSTR - 2B' },
        { 'id': 4, 'name': 'E-Invoice' }, { 'id': 5, 'name': 'E-Way Bill' },
        { 'id': 6, 'name': 'Configuration' }, { 'id': 7, 'name': 'Supplier' },
        { 'id': 8, 'name': 'Customer' }];

        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            mode: "",
            companyid: "",
            locationid: "",
            supplierid: "",
            "configuration": {
                "companyid": null, "gst": null, "gst_username": null, "gst_password": null, "otp": null, "eway_username": null, "eway_password": null,
                "einv_username": null, "einv_password": null
            }

        };

        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfilename = null;
            var results = [];
            var i = 0;
            var colIndex = {
                'TradeName': 1, 'GSTN': 2, 'InvoiceNo': 4, 'InvoiceDate': 5,
                'Taxable': 6, 'IGST': 7, 'CGST': 8, 'SGST': 9
            };
            var isTally = false;
            for (; i < value.rows.length; i++) {
                if (value.rows[i].values.length > 0) {
                    if (value.rows[i].values[0] == 'Date' &&
                        value.rows[i].values[1] == 'Particulars') {
                        isTally = true;
                        break;
                    }
                }
            }
            if (!isTally) {
                var colIndex = {
                    'TradeName': -1, 'GSTN': -1, 'InvoiceNo': -1, 'InvoiceDate': -1,
                    'Taxable': -1, 'IGST': -1, 'CGST': -1, 'SGST': -1
                };
                var i = 1;
                $.each(value.columns, function (k, v) {
                    if (v.indexOf('Date') >= 0 || v.indexOf('DATE') >= 0) {
                        colIndex.InvoiceDate = k;
                    } else if (v.indexOf('No') >= 0 || v.indexOf('NO') >= 0) {
                        colIndex.InvoiceNo = k;
                    } else if (v.indexOf('Name') >= 0 || v.indexOf('NAME') >= 0) {
                        colIndex.TradeName = k;
                    } else if (v.indexOf('Taxable') >= 0 || v.indexOf('TAXABLE') >= 0) {
                        colIndex.Taxable = k;
                    } else if (v.indexOf('igst') >= 0 || v.indexOf('IGST') >= 0 || v.indexOf('Integrated Tax') >= 0) {
                        colIndex.IGST = k;
                    } else if (v.indexOf('cgst') >= 0 || v.indexOf('CGST') >= 0 || v.indexOf('Central Tax') >= 0) {
                        colIndex.CGST = k;
                    } else if (v.indexOf('sgst') >= 0 || v.indexOf('SGST') >= 0 || v.indexOf('State Tax') >= 0) {
                        colIndex.SGST = k;
                    }
                });
                var msg = '';
                if (colIndex.TradeName == -1) {
                    msg = 'TradeName';
                } else if (colIndex.GSTN == -1) {
                    msg = 'GSTN';
                } else if (colIndex.InvoiceNo == -1) {
                    msg = 'InvoiceNo';
                } else if (colIndex.InvoiceDate == -1) {
                    msg = 'InvoiceDate';
                } else if (colIndex.Taxable == -1) {
                    msg = 'Taxable';
                } else if (colIndex.IGST == -1) {
                    msg = 'IGST';
                } else if (colIndex.CGST == -1) {
                    msg = 'CGST';
                } else if (colIndex.SGST == -1) {
                    msg = 'SGST';
                }

                if (msg != '') {
                    $rootScope.Toaster.Info(msg + " Missing <br>Expected<br>" + 'TradeName,GSTN,InvoiceNo,InvoiceDate,Taxable,IGST,CGST,SGST');
                    return;
                }
                i = 1;
            }

            var expenseList = [];
            for (; i < value.rows.length; i++) {
                if (value.rows[i].values.length > 0) {
                    var values = value.rows[i].values;
                    expenseList.push({
                        'gstno': values[colIndex.GSTN],
                        'invoiceno': values[colIndex.InvoiceNo],
                        'invoicedate': values[colIndex.InvoiceDate],
                        'reason': 'Import',
                        'tradename': values[colIndex.TradeName],
                        'taxable': values[colIndex.Taxable],
                        'tax': values[colIndex.IGST] +
                            values[colIndex.CGST] +
                            values[colIndex.SGST]
                    });
                }
            }

            var request = {
                "Items": expenseList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "Gst_InboxService/Import",
                data: srequest
            }).success(function (result) {

            }).error(function (result) {
            });
        }



        $scope.GSTInbox = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.GSTInbox = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.Companies = restService.Get('ReferencelistService/FULLCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });


        $scope.ErrorMessage = "";
        $scope.CurrentCompany = null;
        $scope.CompanyChanged = function () {
            $scope.CurrentCompany = null;
            if ($scope.GSTInbox && $scope.GSTInbox.companyid &&
                $scope.GSTInbox.companyid > 0) {
                $scope.CurrentCompany = $scope.Companies.find(function (item) {
                    return item.id == $scope.GSTInbox.companyid;
                });
                $scope.GSTInbox.gstno = $scope.CurrentCompany.tinanddate;
                $scope.GSTInbox.gstusername = $scope.CurrentCompany.gstusername;
                $scope.GSTInbox.gstaccesskey = $scope.CurrentCompany.gstaccesskey;
            }
        }


        $scope.DownloadGSTR1A = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR1A',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "Gst_InboxService/Download",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info(result.apiresponse);
            }).error(function (result) {
            });
        }

        $scope.ResetGSTR1 = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR1',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "Gst_InboxService/ResetGSTR1",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info(result.apiresponse);
            }).error(function (result) {
            });
        }

        $scope.ShowStatusGSTR1 = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR1',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "Gst_InboxService/GSTR1Status",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info(result.apiresponse);
            }).error(function (result) {
            });
        }

        $scope.ValidateHSN = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'ValidateHSN',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "Gst_InboxService/ValidateHSN",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info(result.apiresponse);
            }).error(function (result) {
            });
        }

        $scope.UploadGSTR1 = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR1',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "Gst_InboxService/Upload",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info(result.apiresponse);
            }).error(function (result) {
            });
        }

        $scope.GetPeriod = function () {
            return $scope.GSTInbox.periodmonth + $scope.GSTInbox.periodyear;
        }


        $scope.ApplyOTP = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'ApplyOTP',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstotp': $scope.GSTInbox.gstotp,
            };

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "Gst_InboxService/ApplyOTP",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("OTP Successfully applied");
            }).error(function (result) {
            });
        }

        $scope.RequestOTP = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'RequestOTP',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
            };

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "Gst_InboxService/RequestOTP",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Please enter the OTP");
            }).error(function (result) {
            });
        }

        $scope.DownloadGSTR2A = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR2A',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "Gst_InboxService/Download",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info(result.apiresponse);
            }).error(function (result) {
            });
        }

        $scope.SearchLocal = function (condition) {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'LOCAL',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'type': condition.type,
                'mode': condition.mode,
                'fromdate': $scope.GSTInbox.fromdate,
                'todate': $scope.GSTInbox.todate
            };
            var request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "Gst_InboxService/LocalSearch",
                data: srequest
            }).success(function (result) {


                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 20);
                igrid.options.showHeaderRow = true;
                if (condition.type == 'supplier') {
                    igrid.AddSimpleColumn('suppliername', 'Supplier', 120);
                    igrid.AddSimpleColumn('gstno', 'GST No', 120);
                    igrid.AddFormatedColumn('invoicedate', 'Inv.Date', 120, 'dd-MM-yyyy');
                    igrid.AddSimpleColumn('invoiceno', 'Inv.No', 120);
                    igrid.AddFormatedColumn('taxpercentage', 'Tax %', 70, 'N2', 2).total = true;
                }
                else {
                    igrid.AddSimpleColumn('suppliername', 'Customer', 120);
                    igrid.AddSimpleColumn('gstno', 'GST No', 120);
                    igrid.AddFormatedColumn('invoicedate', 'Bill Date', 120, 'dd-MM-yyyy');
                    igrid.AddSimpleColumn('invoiceno', 'BIll No', 120);
                }
                igrid.AddFormatedColumn('taxable', 'Taxable', 70, 'N2', 2).total = true;
                igrid.AddFormatedColumn('tax', 'Tax', 70, 'N2', 2).total = true;
                if (condition.type == 'supplier') {
                    igrid.AddFormatedColumn('difftax', 'Diff', 70, 'N2', 2).total = true;
                }
                igrid.EnableTotal();
                igrid.Initialize();

                igrid.grid.onDblClick.subscribe(function (e, args) {
                    var columnid = args.grid.getColumns()[args.cell].id;
                    var item = args.grid.getDataItem(args.row);
                    if (columnid == 'suppliername') {
                        var selectedRows = [];
                        for (var i = 0; i < args.grid.getDataLength(); i++) {
                            if (args.grid.getDataItem(i).gstno == item.gstno) {
                                selectedRows.push(i);
                            }
                        }
                        args.grid.setSelectedRows(selectedRows);
                    }
                    else if (columnid == 'gstno') {
                        if ($scope.igrid.columnFilters[columnid] && $scope.igrid.columnFilters[columnid] == item.gstno) {
                            $scope.igrid.ApplyFilter('gstno', '');
                        }
                        else {
                            $scope.igrid.ApplyFilter('gstno', item.gstno);
                        }
                    }
                });
                $scope.CurrentResults = result;
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result);


            }).error(function (result) {
            });
        }

        $scope.ReconcilGSTR2A = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR2A',
                'gstno': $scope.GSTInbox.gstno,
                'showfiling': $scope.GSTInbox.showfiling,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };
            var request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "Gst_InboxService/Reconcil",
                data: srequest
            }).success(function (result) {
                $scope.GSTInboxConfig(result);
            }).error(function (result) {
            });
        }

        $scope.ExcludeList = function () {
            if ($scope.igrid != null) {

                var dialog = ngDialog.open({
                    template: 'ChooseReason',
                    data: [$scope],
                    width: '600px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.Selected = function (item, isreserve) {
                            $scope.closeThisDialog({ 'reason': item, 'isreserve': isreserve, 'apply': false });
                        };
                        $scope.AutoExpenses = function () {
                            $scope.closeThisDialog({ 'reason': '', 'isreserve': false, 'apply': true });
                        }
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' &&
                            data.value != '$document') {

                            var selectedrows = $scope.igrid.GetSelectedRows();

                            $.each(selectedrows, function (k, v) {
                                v.reason = data.value.reason;
                                v.isreserve = data.value.isreserve;
                            })

                            var request = {
                                "Items": selectedrows,
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "Gst_InboxService/Exclude",
                                data: srequest
                            }).success(function (result) {
                                $.each(result, function (k, v) {
                                    var found = selectedrows.find(function (item) {
                                        return item.id == v.id;
                                    });
                                    if (found && found != null)
                                        found.type = v.reason;
                                });
                                $scope.igrid.UpdateData($scope.CurrentResults);
                            }).error(function (result) {
                            });

                        }
                    }
                });
            }
        }

        $scope.ReconciliationGroups = [];
        $scope.GroupReconciliation = function (list) {
            var groups = [];
            $.each(list, function (k, v) {
                var exist = groups.find(function (item) {
                    return item.type == v.type;
                });

                if (typeof (exist) != 'undefined' && exist != null) {
                    exist.taxable += v.taxable;
                    exist.tax += v.tax;
                    exist.count += v.count;
                    var texist = exist.taxlist.find(function (titem) {
                        return titem.rate == v.taxpercentage;
                    });
                    if (typeof (texist) != 'undefined' && texist != null) {
                        texist.taxable += v.taxable;
                        texist.tax += v.tax;
                        texist.count += 1;
                    }
                    else {
                        exist.taxlist.push({ 'rate': v.taxpercentage, 'taxable': v.taxable, 'tax': v.tax, 'count': 1 });
                    }
                }
                else {
                    var exist = { 'taxable': v.taxable, 'tax': v.tax, 'count': v.count, 'type': v.type, 'taxlist': [] };
                    exist.taxlist.push({ 'rate': v.taxpercentage, 'taxable': v.taxable, 'tax': v.tax, 'count': 1 });
                    groups.push(exist);
                }
            })
            $scope.ReconciliationGroups = groups;
        }

        $scope.selectionchanged = function (item) {
            var selected = item.selected;
            $.each($scope.ReconciliationGroups, function (k, v) {
                v.selected = false;
            })
            item.selected = selected;
            $scope.igrid.ApplyFilter('type', selected ? item.type : '');
        }

        $scope.DownloadGSTR2B = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR2B',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };

            var request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "Gst_InboxService/Download",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info(result.apiresponse);
            }).error(function (result) {
            });
        }
        $scope.ReconcilGSTR2B = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR2B',
                'gstno': $scope.GSTInbox.gstno,
                'showfiling': $scope.GSTInbox.showfiling,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };
            var request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "Gst_InboxService/Reconcil",
                data: srequest
            }).success(function (result) {
                $scope.GSTInboxConfig(result);
            }).error(function (result) {
            });
        }

        $scope.SearchCondition = {
            fromdate: "",
            todate: ""
        }

        $scope.New = function () {
            $scope.GSTInbox = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.Save = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gstno': $scope.GSTInbox.gstno,
                'gst_username': $scope.GSTInbox.gst_username,
                'eway_username': $scope.GSTInbox.eway_username,
                'einv_username': $scope.GSTInbox.einv_username
            };

            request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "CompanyService/SaveGSTConfiguration",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Configuration saved successfully.");
            }).error(function (result) {
            });
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrentryService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.GSTInboxConfig();
            }).error(function (result) {
                $rootScope.Toaster.Info("Configuration saved successfully.");
            });

        }

        $scope.igrid = null;
        $scope.GSTInboxConfig = function (result) {

            $scope.GroupReconciliation(result);

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('type', 'type', 120);
            igrid.AddSimpleColumn('suppliername', 'Supplier', 120);
            igrid.AddSimpleColumn('gstno', 'gstno', 120);
            igrid.AddFormatedColumn('invoicedate', 'Inv.Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('filedmonth', 'Filed Month', 120);
            igrid.AddFormatedColumn('fileddate', 'Filed On', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('ptinvoiceno', 'PrtlInv.No', 120);
            igrid.AddSimpleColumn('qtinvoiceno', 'Inv.No', 120);
            igrid.AddSimpleColumn('qtentryno', 'Entry No', 120);
            igrid.AddFormatedColumn('qtentrydate', 'Entry Date', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('taxfrm', 'State', 50);
            igrid.AddSimpleColumn('rchrg', 'RCM', 50);
            igrid.AddFormatedColumn('taxable', 'Taxable', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tax', 'Tax', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxpercentage', '%', 70, 'N2', 2);
            igrid.AddFormatedColumn('difftax', 'Tax Diff', 70, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onDblClick.subscribe(function (e, args) {
                var columnid = args.grid.getColumns()[args.cell].id;
                var item = args.grid.getDataItem(args.row);
                if (columnid == 'suppliername') {
                    var selectedRows = [];
                    for (var i = 0; i < args.grid.getDataLength(); i++) {
                        if (args.grid.getDataItem(i).gstno == item.gstno) {
                            selectedRows.push(i);
                        }
                    }
                    args.grid.setSelectedRows(selectedRows);
                }
                else if (columnid == 'gstno') {
                    if ($scope.igrid.columnFilters[columnid] && $scope.igrid.columnFilters[columnid] == item.gstno) {
                        $scope.igrid.ApplyFilter('gstno', '');
                    }
                    else {
                        $scope.igrid.ApplyFilter('gstno', item.gstno);
                    }
                }
            });
            $scope.CurrentResults = result;
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result);
        }

        $scope.PrepareGSTR1 = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR1A',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };
            var request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "Gst_InboxService/PrepareGSTR1",
                data: srequest
            }).success(function (result) {
                $scope.ShowGSTR1AOptions = true;
                $scope.GSTR1AResult = result;
                $scope.GSTR1AConfig($scope.GSTR1AResult, 'B2B');
            }).error(function (result) {
            });
        }

        $scope.ShowGSTR1 = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR1A',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };
            var request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "Gst_InboxService/ShowGSTR1",
                data: srequest
            }).success(function (result) {
                $scope.GSTR1Config(result, 'B2B');
            }).error(function (result) {
            });
        }

        $scope.DownloadGSTR1 = function () {
            var item = {
                'id': $scope.CurrentCompany.id,
                'gsttype': 'GSTR1A',
                'gstno': $scope.GSTInbox.gstno,
                'gstusername': $scope.GSTInbox.gstusername,
                'gstaccesskey': $scope.GSTInbox.gstaccesskey,
                'gstperiod': $scope.GetPeriod()
            };
            var request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "Gst_InboxService/Download",
                data: srequest
            }).success(function (result) {
                $scope.ShowGSTR1AOptions = true;
                $scope.GSTR1AResult = result;
                $scope.GSTR1AConfig($scope.GSTR1AResult, 'B2B');
            }).error(function (result) {
            });
        }

        $scope.ShowGSTR1AOptions = false;
        $scope.GSTR1AConfig = function (result, option) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;

            if (option == 'B2B') {

                var list = [];
                $.each(result.b2b, function (k, v) {
                    $.each(v.inv, function (kk, vv) {
                        $.each(vv.itms, function (kkk, vvv) {
                            list.push({ 'itms': vvv, 'inv': vv, 'ctin': v.ctin });
                        });
                    });
                });

                igrid.AddSimpleColumn('ctin', 'GST No', 120);
                column = igrid.AddCustomColumn('inum', 'Bill No', 150, function (item, col) {
                    return item.inv.inum;
                });
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.inv.inum;
                }
                column = igrid.AddCustomColumn('idt', 'Bill Date', 100, function (item, col) {
                    return item.inv.idt;
                });
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.inv.idt;
                }
                column = igrid.AddCustomColumn('val', 'Value', 100, function (item, col) {
                    return item.inv.val;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.inv.val;
                }
                column.total = true;
                column = igrid.AddCustomColumn('num', 'Qty', 50, function (item, col) {
                    return item.itms.num;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.num;
                }
                column = igrid.AddCustomColumn('txval', 'Taxabl', 100, function (item, col) {
                    return item.itms.itm_det.txval;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.txval;
                }
                column.total = true;
                column = igrid.AddCustomColumn('rt', '%', 50, function (item, col) {
                    return item.itms.itm_det.rt;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.rt;
                }
                column = igrid.AddCustomColumn('iamt', 'IGST', 75, function (item, col) {
                    return item.itms.itm_det.iamt;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.iamt;
                }
                column.total = true;
                column = igrid.AddCustomColumn('camt', 'CGST', 75, function (item, col) {
                    return item.itms.itm_det.camt;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.camt;
                }
                column.total = true;
                column = igrid.AddCustomColumn('samt', 'SGST', 75, function (item, col) {
                    return item.itms.itm_det.samt;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.samt;
                }
                column.total = true;
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.UpdateData(list);
            }

            if (option == 'B2C') {
                var list = result.b2cs;
                igrid.AddFormatedColumn('rt', '%', 100, 'N2', 2);
                igrid.AddFormatedColumn('txval', 'Taxable', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('camt', 'CGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('samt', 'SGST', 100, 'N2', 2).total = true;
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.UpdateData(list);
            }

            if (option == 'B2CL') {

                var list = [];
                $.each(result.b2cl, function (k, v) {
                    $.each(v.inv, function (kk, vv) {
                        $.each(vv.itms, function (kkk, vvv) {
                            list.push({ 'itms': vvv, 'inv': vv, 'ctin': v.ctin });
                        });
                    });
                });

                column = igrid.AddCustomColumn('inum', 'Bill No', 150, function (item, col) {
                    return item.inv.inum;
                });
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.inv.inum;
                }
                column = igrid.AddCustomColumn('idt', 'Bill Date', 100, function (item, col) {
                    return item.inv.idt;
                });
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.inv.idt;
                }
                column = igrid.AddCustomColumn('val', 'Value', 100, function (item, col) {
                    return item.inv.val;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.inv.val;
                }
                column.total = true;
                column = igrid.AddCustomColumn('num', 'Qty', 50, function (item, col) {
                    return item.itms.num;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.num;
                }
                column = igrid.AddCustomColumn('txval', 'Taxabl', 100, function (item, col) {
                    return item.itms.itm_det.txval;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.txval;
                }
                column.total = true;
                column = igrid.AddCustomColumn('rt', '%', 50, function (item, col) {
                    return item.itms.itm_det.rt;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.rt;
                }
                column = igrid.AddCustomColumn('iamt', 'IGST', 75, function (item, col) {
                    return item.itms.itm_det.iamt;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.iamt;
                }
                column.total = true;
                column = igrid.AddCustomColumn('camt', 'CGST', 75, function (item, col) {
                    return item.itms.itm_det.camt;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.camt;
                }
                column.total = true;
                column = igrid.AddCustomColumn('samt', 'SGST', 75, function (item, col) {
                    return item.itms.itm_det.samt;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.itm_det.samt;
                }
                column.total = true;
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.UpdateData(list);
            }

            if (option == 'EXPORT') {

                var list = [];
                $.each(result.exp, function (k, v) {
                    $.each(v.inv, function (kk, vv) {
                        $.each(vv.itms, function (kkk, vvv) {
                            list.push({ 'itms': vvv, 'inv': vv, 'exp_typ': v.exp_typ });
                        });
                    });
                });

                igrid.AddSimpleColumn('exp_typ', 'Type', 120);
                column = igrid.AddCustomColumn('inum', 'Bill No', 150, function (item, col) {
                    return item.inv.inum;
                });
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.inv.inum;
                }
                column = igrid.AddCustomColumn('idt', 'Bill Date', 100, function (item, col) {
                    return item.inv.idt;
                });
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.inv.idt;
                }
                column = igrid.AddCustomColumn('val', 'Value', 100, function (item, col) {
                    return item.inv.val;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.inv.val;
                }
                column.total = true;
                column = igrid.AddCustomColumn('txval', 'Taxabl', 100, function (item, col) {
                    return item.itms.txval;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.txval;
                }
                column.total = true;
                column = igrid.AddCustomColumn('rt', '%', 50, function (item, col) {
                    return item.itms.rt;
                });
                column.cssClass = 'rightalign';
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    return item.itms.rt;
                }
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.UpdateData(list);
            }

            if (option == 'HSN') {
                var list = result.hsn.data;
                igrid.AddFormatedColumn('num', 'S.No', 50, 'N0', 0);
                igrid.AddSimpleColumn('hsn_sc', 'HSN', 100);
                igrid.AddSimpleColumn('uqc', 'UQC', 50);
                igrid.AddFormatedColumn('qty', 'Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rt', '%', 50, 'N2', 2);
                igrid.AddFormatedColumn('txval', 'Taxable', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('iamt', 'IGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('camt', 'CGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('samt', 'SGST', 100, 'N2', 2).total = true;

                var column = igrid.AddCustomColumn('total', 'Total', 100, function (item, col) {
                    return $filter('IndianNumber')(item.txval + item.iamt + item.camt + item.samt, 2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return item.txval + item.iamt + item.camt + item.samt;
                };
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.UpdateData(list);
            }

            if (option == 'HSNError') {
                var list = result.hsn.data;
                igrid.AddFormatedColumn('num', 'S.No', 50, 'N0', 0);
                igrid.AddSimpleColumn('hsn_sc', 'HSN', 100);
                igrid.AddSimpleColumn('uqc', 'UQC', 50);
                igrid.AddFormatedColumn('qty', 'Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rt', '%', 50, 'N2', 2);
                igrid.AddFormatedColumn('txval', 'Taxable', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('iamt', 'IGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('camt', 'CGST', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('samt', 'SGST', 100, 'N2', 2).total = true;

                var column = igrid.AddCustomColumn('total', 'Total', 100, function (item, col) {
                    return $filter('IndianNumber')(item.txval + item.iamt + item.camt + item.samt, 2);
                });
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    return item.txval + item.iamt + item.camt + item.samt;
                };
                igrid.EnableTotal();
                igrid.Initialize();
                igrid.UpdateData(list);
            }
            $scope.igrid = igrid;
        }


        $scope.ShowGSTR1Options = false;
        $scope.GSTR1Config = function (result, option) {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('billno', 'Bill No', 120);
            igrid.AddFormatedColumn('billdate', 'Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('gstno', 'GST No', 120);
            igrid.AddSimpleColumn('customer', 'Customer', 120);
            igrid.AddFormatedColumn('qty', 'Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tax', 'Tax', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivable', 'Taxable', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('filedtax', 'Filed Tax', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('filedreceivable', 'Filed Taxable', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            igrid.UpdateData(result.B2B);
        }

        $scope.SaveGST1AXLSX = function () {
            $rootScope.Download('Gst_InboxService/DownloadGSTR1/' + $scope.UserInfo.OrganizationID + '/' + $scope.GSTInbox.companyid
                + '/' + $scope.GSTInbox.periodyear + '/' + $scope.GSTInbox.periodmonth,
                'gstr1' + $scope.GSTInbox.periodyear + '_' + $scope.GSTInbox.periodmonth + '.xlsx', 'application/pdf');
        }

        $scope.exporttoExcel = function (infygrid, filename) {
            var title = 'GST Inbox';
            var additionalrows = [];
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, '', true, additionalrows);
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

        $scope.SaveGST1AJSON = function () {

            var blob = new Blob([JSON.stringify($scope.GSTR1AResult)],
                { type: "text/plain;charset=utf-8" });

            if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                window.navigator.msSaveOrOpenBlob(blob, 'gstr1.json');
            }
            else {
                var e = document.createEvent('MouseEvents'),
                    a = document.createElement('a');
                a.download = 'gstr1.json';
                a.href = window.URL.createObjectURL(blob);
                a.dataset.downloadurl = 'gstr1.json';
                e.initEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
                a.dispatchEvent(e);
                window.URL.revokeObjectURL(a.href);
            }
        }

        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' &&
                typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 75) + 'px';
            }
        }, 100);

    }]);