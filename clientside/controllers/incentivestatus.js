
var app = angular.module('InfinityLab');

app.controller('IncentiveStatusController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    'restService', '$timeout', 'ngDialog', 'Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, Excel) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Modes = [
            //{ 'id': 10, 'name': 'Section Price Range' },
            //{ 'id': 1, 'name': 'Product Price Range' },
            //{ 'id': 2, 'name': 'Employee Price Range' },
            //{ 'id': 3, 'name': 'Section By Age' },
            //{ 'id': 4, 'name': 'Product By Age' },
            { 'id': 5, 'name': 'Employee By Product Age' },
            //{ 'id': 17, 'name': 'Employee By Section' },
            { 'id': 22, 'name': 'Tailoring By Employee' },
            { 'id': 21, 'name': 'Product - Manufacture' },
            { 'id': 15, 'name': 'Product By Age and Price' },
            
            //{ 'id': 11, 'name': 'Product By Qty/Price' },
            { 'id': 6, 'name': 'Barcode Prefix By Price' },
            //{ 'id': 66, 'name': 'Barcode Prefix With Product' },
            { 'id': 7, 'name': 'Sales Value By Slab' },
            { 'id': 77, 'name': 'Target Value By Slab' },
            { 'id': 8, 'name': 'Target / Gross / Days' },
            { 'id': 9, 'name': 'Company Level' },
            { 'id': 88, 'name': 'Employee Role' },
            //{ 'id': 16, 'name': 'Employee Target' }
        ];

        $scope.TargetSources = [
            { 'id': 1, 'name': 'Sales Value' },
            { 'id': 2, 'name': 'Product Age' },
            { 'id': 3, 'name': 'Barcode Prefix' }];


        $scope.EmployeeRoles = [
            { 'id': 1, 'name': 'Primary' },
            { 'id': 2, 'name': 'Secondary' },
            { 'id': 3, 'name': 'Superviser' }];

        $scope.TargetComputation = [
            { 'id': 1, 'name': 'Additional % Sale Value - Single Slab' },
            { 'id': 2, 'name': 'Additional % Sale Value - Multiple Slab' },
            { 'id': 3, 'name': 'Additional Per Piece Value - Single Slab' },
            { 'id': 4, 'name': 'Additional Per Piece Value - Multiple Slab' },
            { 'id': 5, 'name': 'Minimum' }];

        $scope.selectedcount = 0;
        $scope.Entity = {
            "isactive": true,
            "createdby": "",
            "modifiedby": "",
            "version": 0,
            "createdon": "",
            "modifiedon": "",
            "type": "",
            "additionalinfo": {
                "deductsalary": "", "formula": "", "paytype": "", "fixedincentive": "", "dynamicincentive": "",
                "eligbleday": "", "present": "", "dispressmentmode": "", "salesincentive": [],
                "splpaytypelistid": "", "fromtime": "", "totime": "", "iswarehouse": "", "autosettlement": "",
                "reportconfig": {
                    Bata_Minimum_EntryTime: "",
                    Bata_Male_BeforeTime: "",
                    Bata_Male_AfterTime: "",
                    Bata_Male_BefAmount: "",
                    Bata_Male_AftAmount: "",
                    Bata_Female_BeforeTime: "",
                    Bata_Female_AfterTime: "",
                    Bata_Female_BefAmount: "",
                    Bata_Female_AftAmount: "",
                    Bonus_Percentage: "",
                    OT_Male_BeforeTime: "",
                    OT_Male_AfterTime: "",
                    OT_Male_BefAmount: "",
                    OT_Male_AftAmount: ""
                },
                "permissionset": [],
                "salesincentive": [],
                "fromdate": null,
                "todate": null
            },
            "locationid":0,
            "sortingindex": "",
            "id": 0,
            "code": "",
            "name": "",
            productgroup: 0,
            referenceid: ""
        };

        $scope.Referencelist = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.ItemList = [];
        $scope.SearchResult = [];
        $scope.SearchCondition = {
            "id": 0,
            "mode": 1,
            "locationid": 0,
            "barcode": "",
            "modeid": 0
        };


        $scope.nsplitqty = 0;
        $scope.npdiscount = 0;

        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
            results.push({ 'id': 0, 'name': 'All Company' });
        });

        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'id': 0, 'name': 'All Location' });
        });

        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
            results.push({ 'id': 0, 'name': 'All Section' });
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
            results.push({ 'id': 0, 'name': 'All Products' });
            results.push({ 'id': -1, 'name': 'Add All Products' });
        });

        $scope.EmployeeTypes = [{ 'id': 0, 'name': 'All SALESMEN' },{ 'id': -1, 'name': 'All SUPERVISOR' },{ 'id': -2, 'name': 'All MANAGEMENT STAFF' }];

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
            results.push({ 'id': 0, 'name': 'All SALESMEN' });
            results.push({ 'id': -1, 'name': 'All SUPERVISOR' });
            results.push({ 'id': -2, 'name': 'All MANAGEMENT STAFF' });
        });

        $scope.DeleteItem = function (item) {
            $scope.Referencelist.additionalinfo.salesincentive.splice($scope.Referencelist.additionalinfo.salesincentive.indexOf(item), 1);
        }

        $scope.GetMode = function (mode) {
            switch (mode) {
                case 1:
                    return 'INCENTIVE-PRODUCT';
                case 2:
                    return 'INCENTIVE-EMPLOYEE';
                case 3:
                    return 'INCENTIVE-SECTION-AGE';
                case 4:
                    return 'INCENTIVE-PRODUCT-AGE';
                case 21:
                    return 'INCENTIVE-PRODUCT-MFG';
                case 22:
                    return 'INCENTIVE-EMPLOYEE-TAILORING';
                case 5:
                    return 'INCENTIVE-EMPLOYEE-AGE';
                case 6:
                    return 'INCENTIVE-BARCODE';
                case 66:
                    return 'INCENTIVE-BARCODE-PRODUCT';
                case 7:
                    return 'INCENTIVE-SALES-VALUE';
                case 77:
                    return 'INCENTIVE-TARGET-SLAB';
                case 8:
                    return 'INCENTIVE-TARGET-GROSS-DAYS';
                case 88:
                    return 'INCENTIVE-EMP-ROLE';
                case 9:
                    return 'INCENTIVE-COMPANY';
                case 10:
                    return 'INCENTIVE-SECTION';
                case 11:
                    return 'INCENTIVE-LEVEL-PRODUCT';
                case 15:
                    return 'INCENTIVE-PRODUCT-AGEPRICE';
                case 16:
                    return 'INCENTIVE-TARGET';
                case 17:
                    return 'INCENTIVE-EMP-SECTION';

            }
            return null;
        }

        $scope.GetName = function (referenceid) {
            var sourcedata = null;
            switch ($scope.SearchCondition.modeid) {
                case 0:
                case 3:
                    sourcedata = $scope.Sections;
                    break;
                case 1:
                case 4:
                case 21:
                case 11:
                case 15:
                case 66:
                    sourcedata = $scope.Products;
                    break;
                case 2:
                case 8:
                case 7:
                case 77:
                case 5:
                case 16:
                case 17:
                case 22:
                    sourcedata = $scope.Employees;
                    break;
                case 9:
                    sourcedata = $scope.Company;
                    break;
                case 88:
                    sourcedata = $scope.EmployeeRoles;
                    break;
            }
            if (sourcedata != null)
                return $scope.SearchRef(referenceid, sourcedata)
            return '';
        }

        $scope.Search = function (searchall) {
            $scope.Item = null;

            if (typeof ($scope.SearchCondition.locationid) == 'undefined' || $scope.SearchCondition.locationid == null || $scope.SearchCondition.locationid == 0) {
                $rootScope.Toaster.Info("Please select the location for search.!");
                return false;
            }
            if (!searchall) {
                if (typeof ($scope.SearchCondition.barcode) == 'undefined' || $scope.SearchCondition.barcode == null || $scope.SearchCondition.barcode == '') {
                    $rootScope.Toaster.Info("Please enter the barcode for search.!");
                    return false;
                }
            }
            request = {
                "item": {
                    barcode: $scope.SearchCondition.barcode,
                    locationid: $scope.SearchCondition.locationid,
                    type: $scope.GetMode($scope.SearchCondition.modeid),
                    searchall: searchall
                },
                "securitytoken": "String content"
            }

            if (searchall)
                $scope.ItemList = [];

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "IncentiveconfigService/SearchBarcode",
                data: srequest
            }).success(function (result) {
                if (result != 'undefined') {
                    if (typeof ($scope.ItemList) == 'undefined' || typeof ($scope.ItemList.length) == 'undefined')
                        $scope.ItemList = [];

                    $.each(result, function (k, v) {
                        $scope.ItemList.push(v);
                    })
                    $scope.SearchCondition.barcode = '';
                    $scope.igrid.UpdateData($scope.ItemList, '_barcode');
                }
            }).error(function (result) {
            });
        }


        $scope.Save = function () {

            var request = {
                "Item": {
                    'locationid': $scope.SearchCondition.locationid,
                    'incentive': $scope.SearchCondition.incentive,
                    'items': [],
                }
            };

            var list = [];
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                request.Item.items.push(v);
                list.push(v);
            });


            //$.each($scope.ItemList, function (k, v) {
            //    if (typeof (v.selected) != 'undefined' && v.selected) {
            //        request.Item.items.push(v);
            //        list.push(v);
            //    }
            //});


            $scope.igrid.UpdateData($scope.ItemList, '_barcode');

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "IncentiveconfigService/SaveAll",
                data: srequest
            }).success(function (result) {
                if (result.success)
                    $rootScope.Toaster.Info("Successfully saved");
                else
                    $rootScope.Toaster.Info(result.exception);

            }).error(function (result) {

            });
        }

        $scope.fileuploadedcomplete = function (value) {

            if (typeof ($scope.SearchCondition.locationid) == 'undefined' || $scope.SearchCondition.locationid == null || $scope.SearchCondition.locationid == 0) {
                $rootScope.Toaster.Info("Please select the location for search.!");
                return false;
            }

            var barcodes = [];

            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0 && barcode.toLowerCase() != 'barcode') {
                    var found = barcodes.find(function (item) {
                        return item == barcode;
                    });
                    if (typeof (found) == 'undefinied' || found == null) {
                        barcodes.push(barcode);
                    }
                }
            });

            $scope.Item = null;
            request = {
                "item": {
                    barcodes: barcodes,
                    type: $scope.GetMode($scope.SearchCondition.modeid),
                    locationid: $scope.SearchCondition.locationid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentiveconfigService/SearchBarcode",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $scope.ItemList = result;
                    if (value.rows[0].values.length == 2 &&
                        value.rows[0].values[0].toLowerCase() == 'barcode' &&
                        value.rows[0].values[1].toLowerCase() == 'incentive') {

                        $.each(value.rows, function (k, v) {
                            var barcode = v.values[0];
                            if (barcode != null && barcode != '' && barcode.length > 0 && barcode.toLowerCase() != 'barcode') {
                                var rowvalue = result.find(function (item) {
                                    return item.barcode == barcode;
                                });
                                if (rowvalue && rowvalue != null) {
                                    if (v.values[1].toLowerCase().indexOf('%')>0) {
                                        rowvalue.incentive = parseFloat(v.values[1].toLowerCase().replace('%', ''));
                                        rowvalue.ispercentage = true;
                                    }
                                    else {
                                        rowvalue.incentive = parseFloat(v.values[1]);
                                    }
                                }
                            }
                        });

                    }
                }
                $scope.igrid.UpdateData($scope.ItemList, '_barcode');
            }).error(function (result) {
            });
        }

      

        $scope.AddIncentiveConfig = function () {

            if (($scope.SearchCondition.modeid == 15 
                || $scope.SearchCondition.modeid == 21) &&
                $scope.Referencelist.referenceid == -1) {
                $.each($scope.Products, function (k, v) {

                    if (v.id > 0) {
                        var exist = $scope.Referencelist.additionalinfo.salesincentive.find(function (item) {
                            return item.referenceid == v.id;
                        });

                        if (exist == undefined || exist == null) {
                            $scope.Referencelist.additionalinfo.salesincentive.push({
                                'from': $scope.SalesInc.fromvalue,
                                'to': $scope.SalesInc.tovalue,
                                'fromprice': $scope.SalesInc.fromprice,
                                'toprice': $scope.SalesInc.toprice,
                                'incentive': $scope.SalesInc.percentage,
                                'prefix': $scope.SalesInc.prefix,
                                'targetsource': $scope.SalesInc.targetsource,
                                'targetcomputation': $scope.SalesInc.targetcomputation,
                                'referenceid': v.id,
                                'name': $scope.GetName(v.id),
                            });
                        }
                    }
                });
            }
            else {
                $scope.Referencelist.additionalinfo.salesincentive.push({
                    'from': $scope.SalesInc.fromvalue,
                    'to': $scope.SalesInc.tovalue,
                    'fromprice': $scope.SalesInc.fromprice,
                    'toprice': $scope.SalesInc.toprice,
                    'incentive': $scope.SalesInc.percentage,
                    'prefix': $scope.SalesInc.prefix,
                    'targetsource': $scope.SalesInc.targetsource,
                    'targetcomputation': $scope.SalesInc.targetcomputation,
                    'referenceid': $scope.Referencelist.referenceid,
                    'name': $scope.GetName($scope.Referencelist.referenceid),
                });
            }
        }



        $scope.SelectionChanged = function (all) {
            if (all) {
                $.each($scope.ItemList, function (k, v) {
                    v.selected = $scope.SearchCondition.selected;
                });
                $scope.selectedcount = $scope.SearchCondition.selected ? $scope.ItemList.length : 0;
            }
            else {
                $scope.selectedcount = 0;
                $.each($scope.ItemList, function (k, v) {
                    if (typeof (v.selected) != 'undefined' && v.selected) {
                        $scope.selectedcount++;
                    }
                });
            }
        }

        $scope.ConfigGrid = function () {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('productname', 'Product Name', 80);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);

            igrid.AddSimpleColumn('sectionname', 'Section Name', 120);
            igrid.AddSimpleColumn('price', 'Price', 80);
            igrid.AddSimpleColumn('from', 'From', 150);

            igrid.AddSimpleColumn('to', 'To', 200);
            igrid.AddSimpleColumn('sectionincentive', 'Section Incentive', 100);
            
            var column = igrid.AddCustomColumn('incentive', 'Incentive', 100, function (item, col) {
                if (item.ispercentage)
                    return item.incentive + '%';
                return item.incentive.toString();
            });
            column.cssClass = 'rightalign';
            column.sortable = true;
            column.enablefilter = true;
            column.getValue = function (item, col) {
                if (item.ispercentage)
                    return item.incentive + '%';
                return item.incentive.toString();
            }

            igrid.onSelectedRangesChanged = function (range, evt) {
                var rows = evt.getSelectedRows();

                $scope.$apply(function () {
                    $scope.selectedcount = rows.length;
                });

            };
            igrid.Initialize();
            $scope.igrid = igrid;
        }();

        $scope.Apply = function () {
            var ispercentage = $scope.SearchCondition.incentive.toString().endsWith('%');
            var incentive = ispercentage ? parseFloat($scope.SearchCondition.incentive.toString().replace('$', '')) : parseFloat($scope.SearchCondition.incentive);

            /*$.each($scope.ItemList, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected) {
                    v.incentive = incentive;
                    v.ispercentage = ispercentage;
                }
            });*/

            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                v.incentive = incentive;
                v.ispercentage = ispercentage;
            });

            $scope.igrid.UpdateData($scope.ItemList, '_barcode');
        }

        $scope.CollectIncentiveInformation = function () {

            $scope.Referencelist.productwise = $scope.SearchCondition.modeid == 1;
            var request = {
                "item": $scope.Referencelist,
                "securitytoken": "String content"
            }

            if ($scope.Referencelist.referenceid == 0) return;

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/SectionWiseSalesIncentive",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {

                    /*if (typeof ($scope.Referencelist.additionalinfo.salesincentive) != 'undefined' &&
                        $scope.Referencelist.additionalinfo.salesincentive != null &&
                        $scope.Referencelist.additionalinfo.salesincentive.length > 0) {
                        $.each(result, function (item) {
                            item.newincentive = item.incentive;
                            var existingitem = $scope.Referencelist.additionalinfo.salesincentive.find(function (sitem) {
                                return sitem.from == item.from && sitem.to == item.to;
                            });
                            if (existingitem != null)
                                item.incentive = existingitem.incentive; 
                        });

                    }*/
                    $scope.Referencelist.additionalinfo.salesincentive = result;
                    $scope.Referencelist.additionalinfo.showincentiveconfig = true;
                    $scope.Referencelist.additionalinfo.calculatedincentive = 0;
                    $scope.Referencelist.additionalinfo.calculatednewincentive = 0;
                    $scope.calculatedincentive();
                }
            }).error(function (result) {
            });
        }

        $scope.calculatedincentive = function () {
            if (typeof ($scope.Referencelist.additionalinfo.showincentiveconfig) != 'undefined' &&
                $scope.Referencelist.additionalinfo.showincentiveconfig) {
                $scope.Referencelist.additionalinfo.calculatedincentive = 0;
                $scope.Referencelist.additionalinfo.calculatednewincentive = 0;
                $.each($scope.Referencelist.additionalinfo.salesincentive, function (index, item) {
                    $scope.Referencelist.additionalinfo.calculatedincentive += (parseFloat(item.incentive) * item.qty);
                    $scope.Referencelist.additionalinfo.calculatednewincentive += (item.newincentive * item.qty);
                });
            }
        }

        $scope.AddIncentive = function () {
            if (typeof ($scope.Referencelist.additionalinfo) == 'undefined' || $scope.Referencelist.additionalinfo == null)
                $scope.Referencelist.additionalinfo = {};
            if (typeof ($scope.Referencelist.additionalinfo.salesincentive) == 'undefined' || $scope.Referencelist.additionalinfo.salesincentive == null)
                $scope.Referencelist.additionalinfo.salesincentive = [];

            var from = parseFloat($scope.SalesInc.from);
            var step = parseFloat($scope.SalesInc.step);
            var to = parseFloat($scope.SalesInc.to);

            for (; from < to; from = from + step) {

                var incentiveitem = $scope.Referencelist.additionalinfo.salesincentive.find(function (item) {
                    return item.from == from + 1;
                });

                if (typeof (incentiveitem) == 'undefined' || incentiveitem == null) {
                    $scope.Referencelist.additionalinfo.salesincentive.push(
                        {
                            'from': from + 1,
                            'to': from + step,
                            'incentive': 0
                        });
                }
            }
        }

        $scope.Delete = function () {

            $.confirm({
                title: 'Delete',
                content: 'Do you want to delete?',
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

                            var request = {
                                "item": $scope.Referencelist,
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "ReferencelistService/Delete",
                                data: srequest
                            }).success(function (result) {
                                

                                $timeout(function () {
                                    $rootScope.Toaster.Error('Deleted Successfully');
                                    $scope.Referencelist.additionalinfo.salesincentive = [];
                                    $scope.Referencelist.id = 0;
                                    $scope.$digest();
                                }, 10);

                            }).error(function (result) {
                            });

                            return true;
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

        $scope.SaveIncentive = function () {
            if (typeof ($scope.Referencelist.additionalinfo) == 'undefined' || $scope.Referencelist.additionalinfo == null)
                $scope.Referencelist.additionalinfo = {};
            if (typeof ($scope.Referencelist.additionalinfo.salesincentive) == 'undefined' || $scope.Referencelist.additionalinfo.salesincentive == null)
                $scope.Referencelist.additionalinfo.salesincentive = [];

            if ($scope.Referencelist.additionalinfo.salesincentive.length == 0) {
                $scope.Delete();
                return;
            }

            $scope.Referencelist.type = $scope.GetMode($scope.SearchCondition.modeid);
            $scope.Referencelist.additionalinfo.fixedincentive = $scope.Referencelist.incentivepercentage;
            $scope.Referencelist.additionalinfo.fromdate = $scope.Referencelist.fromdate;
            $scope.Referencelist.additionalinfo.todate = $scope.Referencelist.todate;
            var request = {
                "item": $scope.Referencelist,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ReferencelistService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info(result.name + "Saved Successfully.!");
                    $scope.Referencelist.version = result.version;
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });
        }


        $scope.LoadIncentive = function (recompute) {
            if (!recompute) return;
            if ($scope.SearchCondition.modeid == 0) return;

            if (typeof ($scope.Referencelist.locationid) == 'undefined' ||
                $scope.Referencelist.locationid == null)
                $scope.Referencelist.locationid = 0;

            if (typeof ($scope.Referencelist.referenceid) == 'undefined' ||
                $scope.Referencelist.referenceid == null)
                $scope.Referencelist.referenceid = 0;

            var referenceid = $scope.Referencelist.referenceid;
            if ($scope.SearchCondition.modeid == 1)
                referenceid = 0;

            var request = {
                "Item": {
                    'locationid': $scope.Referencelist.locationid,
                    'referenceid': referenceid,
                    'type': $scope.GetMode($scope.SearchCondition.modeid)
                },
                "SecurityToken": "String content"
            }
            if (request.Item.type == null) {
                $rootScope.Toaster.Error("Please select the mode.");
                return;
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ReferencelistService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {

                    var item = result[0];
                    if (result.length > 1) {
                        var item = result.find(function (itm) {
                            return itm.locationid == $scope.Referencelist.locationid &&
                                item.referenceid == $scope.Referencelist.referenceid;
                        });
                        if (typeof (item) == 'undefined' || item == null) {
                            item = result.find(function (itm) {
                                return itm.locationid == $scope.Referencelist.locationid;
                            });
                        }
                        if (typeof (item) == 'undefined' || item == null) {
                            item = result.find(function (itm) {
                                return itm.referenceid == $scope.Referencelist.referenceid;
                            });
                        }
                        if (typeof (item) == 'undefined' || item == null)
                            item = result[0];
                    }

                    if (item.locationid != $scope.Referencelist.locationid) {
                        item.locationid = $scope.Referencelist.locationid;
                        //item.id = 0;
                        $rootScope.Toaster.Info("Other location data loaded.");
                    }
                    if (item.referenceid != $scope.Referencelist.referenceid) {
                        if ($scope.SearchCondition.modeid != 5) {
                            item.referenceid = $scope.Referencelist.referenceid;
                            //item.id = 0;
                            $rootScope.Toaster.Info("Other reference data loaded.");
                        }
                    }

                    $scope.Referencelist = item;
                    $scope.Referencelist.incentivepercentage = $scope.Referencelist.additionalinfo.fixedincentive;
                    $scope.Referencelist.fromdate = $scope.Referencelist.additionalinfo.fromdate;
                    $scope.Referencelist.todate = $scope.Referencelist.additionalinfo.todate;
                    if (result.length > 1) {
                        var reftype = $scope.GetMode($scope.SearchCondition.modeid)
                        $.each(result, function (k, v) {
                            if (v.id != item.id && v.type == reftype &&
                                v.locationid == $scope.Referencelist.locationid &&
                                v.referenceid == $scope.Referencelist.referenceid) {
                                request.Item = v;
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "ReferencelistService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                });
                            }
                        });
                    }
                }
                else {
                    $scope.Referencelist.id = 0;
                    $scope.Referencelist.additionalinfo.salesincentive = [];
                }
            }).error(function (result) {
            });
        }

        $scope.exporttoExcel = function (tableid, title, removeactioncolumn) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, title);
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.RemoveItem = function (item) {

            var request = {
                "Item": { 'ids': [] }
            };

            var list = [];
            //$.each($scope.ItemList, function (k, v) {
            //    if (typeof (v.selected) != 'undefined' && v.selected) {
            //        list.push(v);
            //        request.Item.ids.push(v.id);
            //    }
            //});

            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                list.push(v);
                request.Item.ids.push(v.id);
            });



            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentiveconfigService/DeleteAll",
                data: srequest
            }).success(function (result) {

                $.each(list, function (k, v) {
                    $scope.ItemList.splice($scope.ItemList.indexOf(v), 1);
                });

                $scope.igrid.UpdateData($scope.ItemList,'_barcode');

            }).error(function (result) {
            });

        };

        $timeout(function () {
            $scope.setFocusfirst();
            $scope.Referencelist.locationid = 0;
            $scope.Referencelist.referenceid = 0;
            $scope.LoadIncentive(true);
        }, 10);

    }]);