var app = angular.module('InfinityLab');
app.controller('BOMRequestController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', '$state', 'cntxService', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, $state, cntxService, $stateParams) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.orderqty = 0;
        $scope.orderamount = 0;
        $scope.BOMCategory = restService.Get('ReferencelistService/FULL-BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategory = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.MfgStages = restService.Get('ReferencelistService/MFGSTAGES', true).then(function (results) {
            $scope.MfgStages = results;
        });
        $scope.BOMList = [];
        $scope.BOMAllList = [];
        restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMAllList = results;
            if ($scope.igrid != null) {
                $scope.ItemsConfig();
            }
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.JumpList = [];
        $scope.ColorList = [];
        $scope.AllColorList = [];
        restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.AllColorList = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.LocationList = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });

        $scope.Modes = [{ id: "Internal", name: 'Internal' },
        { id: "External", name: 'External' },
        { id: "Audit", name: 'Audit' }];

        $scope.FreightList = [{ id: 1, name: 'To Pay' },
        { id: 2, name: 'Paid' }];

        $scope.Modes = [
            { 'id': 1, 'name': 'BOM REQUEST' },
            { 'id': 2, 'name': 'JOBORDER REQUEST' },
            { 'id': 3, 'name': 'WORKORDER REQUEST' },
            { 'id': 4, 'name': 'WORKORDER DC REQUEST' },
            { 'id': 5, 'name': 'JOBORDER DC REQUEST' },
            { 'id': 6, 'name': 'BOM TRANSFER REQUEST' },
        ];

        $scope.bomchange = function () {
            var bom = $scope.BOMAllList.find(function (item) {
                return item.code == $scope.BOMRequest.Details.bomcode;
            });

            if (typeof (bom) != 'undefined' && bom != null) {
                $scope.BOMRequest.Details.bomid = bom.id;
            }
        }

        $scope.searchbominwardcode = function (updatestock) {
            if (updatestock) {

                if ($scope.BOMRequest.fromlocationid == undefined ||
                    $scope.BOMRequest.fromlocationid == null ||
                    $scope.BOMRequest.fromlocationid == '' ||
                    $scope.BOMRequest.fromlocationid <= 0) {
                    $rootScope.Toaster.Error("Please select location!");
                    return;
                }

                if ($scope.BOMRequest.Details.stockbomcode &&
                    $scope.BOMRequest.Details.stockbomcode != null &&
                    $scope.BOMRequest.Details.stockbomcode.length > 0) {
                    restService.Get('BominvoiceitemsService/SearchBOMStock/' + $scope.BOMRequest.Details.stockbomcode, false).then(function (results) {
                        if (results != null && results.bominvoiceitem != null &&
                            results.stocksummary != null) {
                            var locationstock = results.stocksummary.find(function (item) {
                                return item.locationid == $scope.BOMRequest.locationid;
                            });
                            if (locationstock && locationstock != null &&
                                locationstock.closing>0) {
                                var consum = $scope.BOMRequest.items.find(function (item) {
                                    return item.bomid == results.bominvoiceitem.bomid &&
                                        item.colourid == results.bominvoiceitem.colourid;
                                });
                                if (consum.qty < locationstock.closing) {
                                    consum.bomcode = $scope.BOMRequest.Details.stockbomcode;
                                    consum.bominvoiceitemid = results.bominvoiceitem.id;
                                }
                                else {
                                    var consum1 = (JSON.parse(JSON.stringify(consum)));
                                    consum1.qty = consum.qty - locationstock.closing;
                                    consum1.amount = consum1.qty * consum1.rate;
                                    $scope.BOMRequest.items.push(consum1);
                                    consum.qty = locationstock.closing;
                                    consum.amount = consum.qty * consum.rate;
                                }
                                $scope.igrid.UpdateData($scope.BOMRequest.items, '_id');
                                $scope.BOMRequest.Details.stockbomcode = '';
                            }
                        }
                    });
                    
                }
            }
            else if ($scope.BOMRequest.Details.bomcode &&
                $scope.BOMRequest.Details.bomcode != null &&
                $scope.BOMRequest.Details.bomcode.length > 0) {

                $scope.FindBomStock();

                restService.Get('BominvoiceitemsService/SearchCode/' + $scope.BOMRequest.Details.bomcode, false).then(function (results) {
                    if (results != null) {
                        $scope.BOMRequest.Details.bomcategoryid = results.bomcategoryid;
                        $scope.BOMRequest.Details.bominvoiceitemid = results.id;
                        $scope.BOMRequest.Details.bomid = results.bomid;
                        $scope.BOMRequest.Details.colourid = results.colourid;
                        $scope.BOMRequest.Details.width = results.width;
                        $scope.BOMRequest.Details.rate = results.buyingprice;
                        $scope.setFocuswidth();
                    }
                });
            }
        }

        $scope.AutoBOMCode = false;
        $scope.categoryChange = function () {
            if (typeof ($scope.BOMRequest.Details.bomcategoryid) != 'undefined' &&
                $scope.BOMRequest.Details.bomcategoryid != null) {
                var bomcagtegory = $scope.BOMCategory.find(function (item) {
                    return item.id == $scope.BOMRequest.Details.bomcategoryid;
                });

                if (typeof (bomcagtegory) != 'undefined' && bomcagtegory != null) {
                    if (bomcagtegory.additionalinfo.autocreate) {
                        $scope.AutoBOMCode = true;
                        $scope.BOMRequest.Details.bomid = 0;
                        $scope.BOMRequest.Details.taxid = bomcagtegory.additionalinfo.taxid;
                        $scope.BOMRequest.Details.uomid = bomcagtegory.additionalinfo.uomid;
                        $scope.BOMRequest.Details.coretypeid = bomcagtegory.additionalinfo.parentid;

                    }
                    else {
                        $scope.AutoBOMCode = false;
                        $scope.BOMRequest.Details.bomcode = '';

                    }
                    var BOMList = [];
                    $.each($scope.BOMAllList, function (k, v) {
                        if (v.categoryid == $scope.BOMRequest.Details.bomcategoryid)
                            BOMList.push(v);
                    });
                    $scope.BOMList = BOMList;
                }
            }
        }

        $scope.SetMODEFocus = function () {
            if ($scope.BOMRequest.mode == 4 || $scope.BOMRequest.mode == 5)
                $scope.setFocussupplier();
            if ($scope.BOMRequest.mode == 6)
                $scope.setFocustolocationid();
            else
                $scope.setFocuspreparedby()
        }

        $scope.SetBOMFocus = function () {
            if ($scope.AutoBOMCode)
                $scope.setFocusautobomcode();
            else
                $scope.setFocusbomid();
        }

        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });
        $scope.ShowStockView = false;

        $scope.POMode = [{ id: 0, name: 'Size Range Based' },
        { id: 1, name: 'Size Set Based' },
        { id: 2, name: 'Single Size' }];

        $scope.FreightList = [{ id: 1, name: 'To Pay' },
        { id: 2, name: 'Paid' }];

        $scope.POStatus = [{ id: 0, name: 'PENDING' }, { id: 1, name: 'APPROVED' }, { id: 2, name: 'CANCELLED' }];
        $scope.POSearchStatus = [{ id: 0, name: 'PENDING APPROVAL' }, { id: 1, name: 'APPROVED' }, { id: 2, name: 'CANCELLED' }, { id: -9, name: 'ALL' }];
        $scope.POList = [];
        $scope.PendingPOList = [];
        $scope.Entity = {
            "allowconsumptiontype": false,
            "id": 0,
            "expectedate": "",
            "entrydate": new Date(),
            "supplierid": null,
            "locationid": 0,
            "deliverylocationid": 0,
            "entryno": "",
            "termsandcondition": {},
            "status": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "tolerance": 5,
            "preparedby": 0,
            "approvedby": 0,
            "items": [],
            Details: {
                "oqty": 0,
                "bomcategoryid": null,
                "bomid": null,
                "colourid": null,
                "width": "",
                "id": 0,
                "qty": "",
                "rate": "",
                "amount": "",
            },
            orderno: "",
            mfgworkorderid: 0,
            mfgjoborderid: 0
        };
        $scope.BOMRequest = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.SearchCondition = {
            'entryno': null,
            'supplierid': 0,
            'expectedate': null,
            'statusid': -9
        };

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.ErrorMessage = "";

        $scope.Clear = function () {
            $scope.BOMRequest = (JSON.parse(JSON.stringify($scope.Entity)));
            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.UpdateData($scope.BOMRequest.items, '_id');
        };

        $scope.Save = function () {
            if ($scope.BOMRequest.companyid == null || $scope.BOMRequest.companyid == 0) {
                $rootScope.Toaster.Error("Please select company!");
                return;
            }
            if ($scope.BOMRequest.mode == 4 || $scope.BOMRequest.mode == 5) {
                if ($scope.BOMRequest.supplierid == null || $scope.BOMRequest.supplierid == 0) {
                    $rootScope.Toaster.Error("Please select supplier!");
                    return;
                }
            }
            else {
                $scope.BOMRequest.supplierid = null;
            }
            if ($scope.BOMRequest.entrydate == null || $scope.BOMRequest.entrydate == 0) {
                $rootScope.Toaster.Error("Please select PO date!");
                return;
            }

            $scope.BOMRequest.isactive = true;
            var request = {
                "item": $scope.BOMRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMRequestService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with Entry No :" + result.entryno);
                    $scope.BOMRequest = result;
                    $scope.ItemsConfig();
                } 
                else {
                    $rootScope.Toaster.Error("Attempt Failed." + result.ErrorMessage);
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }
                $scope.CalculateOrderQty();
            }).error(function (result) {
            });
        }

        $scope.productChange = function () {
            if ($scope.BOMRequest.Details.bomid != null) {
                var prditem = $scope.BOMList.find(function (item) {
                    return (item.id == $scope.BOMRequest.Details.bomid);
                });
                if (prditem || prditem != null) {
                    $scope.SetColourList(prditem);
                }
            };
        };

        $scope.setColourFocus = function () {
            if ($scope.ColorList.length > 0)
                $scope.setFocuscolourid();
            else
                $scope.setFocuswidth();
        }

        $scope.SetColourList = function (prditem) {
            $scope.BOMRequest.Details.colourid = null;
            $scope.BOMRequest.Details.bominwardcode = '';
            if (prditem.colourmode == 2) {
                $scope.ColorList = [];
                $.each(prditem.colourlist, function (k, v) {
                    var colour = $scope.AllColorList.find(function (item) {
                        return item.id == v.id;
                    });
                    if (colour && colour.id > 0) {
                        colour.bominwardcode = v.name;
                        colour.imageid = v.imageid;
                        $scope.ColorList.push(colour);
                    }
                });
            } else if (prditem.colourmode == 1) {
                $scope.ColorList = [];
                var colour = $scope.AllColorList.find(function (item) {
                    return item.id == prditem.colorid;
                });
                if (colour && colour.id > 0) {
                    colour.bominwardcode = v.name;
                    colour.imageid = v.imageid;
                    $scope.ColorList.push(colour);
                }
            }
            else {
                $scope.ColorList = [];
                $scope.BOMRequest.Details.bominwardcode = prditem.code;
                $scope.BOMRequest.Details.imageid = prditem.imageid;
            }
            if ($scope.ColorList.length == 1) {
                $scope.BOMRequest.Details.colourid = $scope.ColorList[0].id;
                $scope.FindBomStock();
            }
            else if ($scope.ColorList.length == 0) {
                $scope.FindBomStock();
            }
        }

        $scope.ChangeOrderStatus = function () {
            if ($scope.BOMRequest.id == null || $scope.BOMRequest.id == 0) {
                $rootScope.Toaster.Error("Please select purchase order!");
                return;
            }

            var request = {
                "item": $scope.BOMRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMRequestService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Status updated successfully");

            }).error(function (result) {
            });

        }


        $scope.FindBomStock = function () {
            
            var request = {
                "item": {
                    'bomcode': $scope.BOMRequest.Details.bomcode,
                    'bomid': $scope.BOMRequest.Details.bomid,
                    'colourid': $scope.BOMRequest.Details.colourid,
                    'locationid': $scope.BOMRequest.locationid,
                    'companyid': $scope.BOMRequest.companyid,
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMRequestService/FindBOMStock",
                data: srequest
            }).success(function (result) {
                $scope.avaialblestock = result == null ? 'No Stock':result;
            }).error(function (result) {
            });
        }

        $scope.colourchanged = function () {
            $scope.FindBomStock();
            $scope.BOMRequest.Details.colourcode = '';
            var colourcode = $scope.ColorList.find(function (item) {
                return item.id == $scope.BOMRequest.Details.colourid;
            });
            if (colourcode && colourcode != null)
                $scope.BOMRequest.Details.colourcode = colourcode.name;
        }
        $scope.AddItem = function () {
            if ($scope.BOMRequest.companyid == null || $scope.BOMRequest.companyid == 0) {
                $scope.Toaster.Info("Please enter company");
                return;
            }


            if ($scope.BOMRequest.fromlocationid == null || $scope.BOMRequest.fromlocationid == 0) {
                $scope.Toaster.Info("Please select the location");
                return;
            }

            if ($scope.BOMRequest.Details.bomcategoryid == null || $scope.BOMRequest.Details.bomcategoryid == 0) {
                $scope.Toaster.Info("Please Select BOM Category");
                return;
            }

            item = {
                'bomcategoryid': $scope.BOMRequest.Details.bomcategoryid,
                'bomcode': $scope.BOMRequest.Details.bomcode,
                'colourcode': $scope.BOMRequest.Details.colourcode,
                'bomid': $scope.BOMRequest.Details.bomid,
                'qty': $scope.BOMRequest.Details.qty,
                'taxid': $scope.BOMRequest.Details.taxid,
                'uomid': $scope.BOMRequest.Details.uomid,
                'coretypeid': $scope.BOMRequest.Details.coretypeid,
                'isactive': true,
                'version': 1,
                'imageid': $scope.BOMRequest.Details.imageid,
                'colourid': $scope.BOMRequest.Details.colourid,
                'width': $scope.BOMRequest.Details.width,
                'rate': $scope.BOMRequest.Details.rate,
                'amount': 0,
                'id': 0
            };

            if (typeof (item.imageid) != 'undefined' && item.imageid != null &&
                item.imageid.length > 0)
                $scope.BOMRequest.ShowImage = true;

            item.amount = item.rate * item.qty;
            $scope.BOMRequest.items.splice(0, 0, item);
            if ($scope.BOMRequest.items.length == 1)
                $scope.ItemsConfig();
            else
                $scope.igrid.UpdateData($scope.BOMRequest.items, '_id');
            $scope.BOMRequest.Details.id = 0;
            $scope.avaialblestock = '';
            $scope.BOMRequest.Details.qty = '';
            $scope.BOMRequest.Details.rate = '';
            $scope.BOMRequest.Details.imageid = '';
            $scope.CalculateOrderQty();
            $scope.setFocusbomcategoryid();
        };

        $scope.CalculateOrderQty = function () {
            $scope.orderqty = 0;
            $scope.orderamount = 0;
            $.each($scope.BOMRequest.items, function (k, v) {
                if ($rootScope.GetNumber(v.qty) == 'undefined' || $rootScope.GetNumber(v.qty) == 0)
                    v.qty = 0;
                $scope.orderqty += $rootScope.GetNumber(v.qty);
                $scope.orderamount += $rootScope.GetNumber(v.qty) * $rootScope.GetNumber(v.rate);
            });
        };


        $scope.TransactionTypes = [
            { 'id': -1, 'name': 'All' },
            { 'id': 0, 'name': 'Opening' },
            { 'id': 1, 'name': 'Purchase' },
            { 'id': 2, 'name': 'Purchase Return' },
            { 'id': 3, 'name': 'Transfer In' },
            { 'id': 4, 'name': 'TransferOut' },
            { 'id': 5, 'name': 'WorkOrder Consumption' },
            { 'id': 6, 'name': 'JobOrder Consumption' },
            { 'id': 7, 'name': 'JobOrde Stage Consumption' },
            { 'id': 8, 'name': 'JobOrder Outward' },
            { 'id': 9, 'name': 'WorkOrder Outward' },
            { 'id': 10, 'name': 'Journal' }];

        $scope.ManageItem = function (type, item) {
            if (type == 'Edit') {
                $scope.BOMRequest.Details = item;
                var bomid = $scope.BOMRequest.Details.bomid;
                var colourid = $scope.BOMRequest.Details.colourid;
                $scope.categoryChange();
                $scope.productChange();
                $scope.BOMRequest.Details.bomid = null;
                $scope.BOMRequest.Details.colourid = null;
                $timeout(function () {
                    $scope.BOMRequest.Details.bomid = bomid;
                    $scope.BOMRequest.Details.colourid = colourid;
                    $scope.BOMRequest.items.splice($scope.BOMRequest.items.indexOf(item), 1);
                    $scope.ItemsConfig();
                    $scope.CalculateOrderQty();
                }, 10);
            } else if (type == 'Delete') {
                $scope.BOMRequest.items.splice($scope.BOMRequest.items.indexOf(item), 1);
                $scope.ItemsConfig();
                $scope.CalculateOrderQty();
            }
        };

        $scope.ImportBOM = function () {
            var request = {
                "Item": $scope.BOMRequest,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMRequestService/ImportBOM",
                data: srequest
            }).success(function (result) {
                $scope.BOMRequest.items = result;
                $scope.CalculateOrderQty();
                $timeout(function () {
                    if ($scope.igrid != null) {
                        $scope.igrid.destroy();
                        $scope.igrid = null;
                    }
                    $scope.ItemsConfig();
                }, 50);
            });
        }

        $scope.ManageSearchItem = function (command, item) {
            if (command == 'Edit') {
                $scope.BOMRequest = item;
                var request = {
                    "Item": {
                        'id': item.id
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BOMRequestService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.BOMRequest = result[0];
                    //$scope.BOMRequest.mode = $scope.BOMRequest.isoutward ? 'External' : 'Internal';
                    $scope.BOMRequest.allowconsumptiontype = $scope.BOMRequest.mfgworkorderid > 0;
                    $scope.ShowSearch = false;
                    $scope.Navigation.SetMode('EDIT');
                    $scope.CalculateOrderQty();
                    $timeout(function () {
                        if ($scope.igrid != null) {
                            $scope.igrid.destroy();
                            $scope.igrid = null;
                        }
                        $scope.ItemsConfig();
                    }, 50);
                });

            } else if (command == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ?',
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
                                    url: "BOMRequestService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $rootScope.Toaster.Error("Successfully deleted");
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
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
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });
                /*ngDialog.open({
                    preCloseCallback: function (value) {
                        if (confirm('Do you want to delete?')) {
                            return true;
                        }
                        return false;
                    }
                });*/
            }
        };

        $scope.qtyChanged = function () {
            $scope.BOMRequest.Details.oqty = 0;
            $.each($scope.JumpList, function (k, v) {
                if ($rootScope.GetNumber(v.qty) == 'undefined' || $rootScope.GetNumber(v.qty) == 0)
                    $scope.BOMRequest.Details.oqty += 0;
                else
                    $scope.BOMRequest.Details.oqty += $rootScope.GetNumber(v.qty);
            });
        }

        $scope.CheckSize = function () {

            if (typeof ($scope.BOMRequest.Details.size) != 'undefined' &&
                $scope.BOMRequest.Details.size != null && $scope.BOMRequest.mode == 0) {
                $scope.GetJUMPInformation($scope.BOMRequest.Details.size);
            }

            //$rootScope.Prompt('Jump', 'Jump (start-increment-end)', function (count) {
            //});
        }

        $scope.GetJUMPInformation = function (count) {

            if (count.length > 0) {

                var sizeidList = count.split(',');
                var sizeList = sizeidList[0].split('-');
                if (sizeList.length == 2 && /^[a-zA-Z2-6]*$/.test(sizeList[0])) {

                    var sizeIndex = '3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL'.split(',');
                    var startsize = sizeIndex.indexOf(sizeList[0]);
                    var endsize = sizeIndex.indexOf(sizeList[1]);
                    if (startsize < 0 || endsize < 0) {
                        sizeIndex = '3XS,XXS,XS,S,M,L,XL,2XL,3XL,4XL,5XL,6XL,7XL'.split(',');
                        startsize = sizeIndex.indexOf(sizelist[0]);
                        endsize = sizeIndex.indexOf(sizelist[1]);
                    }
                    sizeList = [];
                    for (var index = startsize; index <= endsize; index++) {
                        sizeList.push(sizeIndex[index]);
                    }

                }
                else if (sizeList.length == 3) {

                    var firstPart = sizeList[0].match(/[^-_ \d]+|\d+/g);
                    var lastPart = sizeList[2].match(/[^-_ \d]+|\d+/g);
                    var suffix = '';
                    var startsize = parseInt(sizeList[0]);
                    var increment = parseInt(sizeList[1]);
                    var endsize = parseInt(sizeList[2]);

                    if (firstPart.length == 2 && /^[a-zA-Z2-6]*$/.test(firstPart[1]))
                        suffix = firstPart[1];

                    sizeList = [];
                    for (; startsize <= endsize; startsize = startsize + increment) {
                        var searchsize = startsize.toString() + suffix;

                        var excluded = false;
                        for (var i = 1; i < sizeidList.length; i++) {
                            if (sizeidList[i] == searchsize)
                                excluded = true;
                        }
                        if (!excluded)
                            sizeList.push(searchsize);
                    }
                }

                var idList = [];
                $.each($scope.SizeList, function (k, v) {
                    $.each(sizeList, function (kk, vv) {
                        if (vv == v.name) {
                            if (idList.find(function (item) {
                                return item.name == v.name;
                            }) == null) {
                                idList.push({ 'id': v.id, 'name': v.name, 'qty': 0, 'index': kk });
                            }
                        }
                    });
                });

                idList.sort(function (a, b) {
                    return a.index - b.index;
                });

                $scope.JumpList = idList;
                //$scope.BOMRequest.Details.qty = 0;
                $('#jumplist INPUT').first().focus();
            }
        }

        $scope.SearchItem = function () {

            if ($scope.BOMRequest.Details.productid == null || $scope.BOMRequest.Details.productid == 0) {
                $scope.Toaster.Info("Please select product");
                return;
            }

            $scope.BOMRequest.Details.id = 0;
            $scope.BOMRequest.Details.brandname = $scope.SearchRef($scope.BOMRequest.Details.brandid, $scope.BrandName, '');
            $scope.BOMRequest.Details.printingname = $scope.SearchRef($scope.BOMRequest.Details.productid, $scope.ProductList, '');
            $scope.BOMRequest.Details.patternname = $scope.SearchRef($scope.BOMRequest.Details.patternid, $scope.PatternList, '');
            $scope.BOMRequest.Details.materialname = $scope.SearchRef($scope.BOMRequest.Details.materialid, $scope.MaterialList, '');
            $scope.BOMRequest.Details.colourname = $scope.SearchRef($scope.BOMRequest.Details.colourid, $scope.ColorList, '');
            $scope.BOMRequest.Details.sizename = $scope.SearchRef($scope.BOMRequest.Details.sizeid, $scope.SizeList, '');
            $scope.BOMRequest.Details.stylename = $scope.SearchRef($scope.BOMRequest.Details.styleid, $scope.StyleList, '');
            $scope.BOMRequest.Details.typename = $scope.SearchRef($scope.BOMRequest.Details.typeid, $scope.TypeList, '');
            $scope.BOMRequest.Details.autocreate = true;

            var request = {
                "Item": $scope.BOMRequest.Details,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length == 0) {
                    $rootScope.Toaster.Info("No Item found. Please create the item");
                    return;
                }
                if (result.length == 1) {
                    $scope.BOMRequest.Details = result[0];
                    $scope.BOMRequest.Actual = (JSON.parse(JSON.stringify($scope.BOMRequest.Details)));
                }
                else if (result.length > 1)
                    $scope.ShowProductSearch(result);
            });
        };

        $scope.SearchItemByName = function () {
            var request = {
                "Item": {
                    'name': $scope.BOMRequest.Details.printingname, 'productid': $scope.BOMRequest.Details.productid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/SearchByName",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {
                    $scope.BOMRequest.Details = result[0];
                    $scope.BOMRequest.Actual = (JSON.parse(JSON.stringify($scope.BOMRequest.Details)));
                }
                else if (result.length > 1)
                    $scope.ShowProductSearch(result);
            });
        };

        $scope.Export = function () {
            if (typeof ($scope.igrid) == 'undefined') return;
            $scope.exportHref = $scope.igrid.ExportExcel($scope.igrid.grid, 'Product List');
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
        $scope.Search = function () {
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMRequestService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.SearchConfig();
            });
        };

        $scope.Publish = function () {
            restService.Get('BOMRequestService/Publish/' + $scope.UserInfo.OrganizationID + '/' + $scope.BOMRequest.id, true).then(function (results) {
                if (results != null && results != 'undefined' && results == 'true')
                    $rootScope.Toaster.Info("Purchase Order Sent Successfully.")
                else
                    $rootScope.Toaster.Info("Error in Sending Purchase Order.")
            });
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == 'posummary')
                subtitle = 'PURCHASE ORDER - SUMMARY REPORT';
            else if (tableid == 'podetails')
                subtitle = 'PURCHASE ORDER - ITEM DETAILS REPORT';
            //subtitle = 'PURCHASE ORDER - SUMMARY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company) + " ) ";

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

        $scope.Print = function (id,mode) {
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "po.pdf";
                link.href = 'BOMRequestService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + mode + '/' + id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.fileuploadedcomplete = function (items) {
            if (typeof ($scope.BOMRequest) == 'undefined' || $scope.BOMRequest == null)
                $scope.BOMRequest = {};
            $scope.BOMRequest.items = items;
            $scope.ItemsConfig();
            $scope.uploadfile = null;
        }

        $scope.ShowComponentCreation = function () {
            var dialog = ngDialog.open({
                template: 'clientside/mfg/views/BOMMaster.html?d=' + (new Date()).getTime(),
                data: [$scope],
                width: '650px',
                controller: 'BOMMasterController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;
                    $scope.BOMAllList.push(data.value);
                    $scope.categoryChange();
                }
            });
        }


        $scope.ItemsConfig = function () {
            $.each($scope.BOMRequest.items, function (k, v) {
                v.sno = k + 1;
            });
            if ($scope.igrid != null) {
                //if ($scope.BOMRequest.items.length > 1) {
                //    $scope.igrid.UpdateData($scope.BOMRequest.items, '_id');
                //    return;
                //}
                $scope.igrid.destroy();
            }

            var igrid = new InfyGrid('editGrid', '400px', '100%', true, true, true,
                ($scope.BOMRequest && $scope.BOMRequest.isrequest) ? 30: 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sno', 'S.No', 40);
            igrid.AddReferenceColumn('bomcategoryid', 'Category', 150, $scope.BOMCategory);
            
            var column = igrid.AddCustomColumn('bomid', 'Component', 100, function (item) {
                var bom = $scope.BOMAllList.find(function (k) {
                    return k.id == item.bomid;
                });
                if (bom && bom != null) return bom.title;
                return '-';
            });

            var column = igrid.AddSimpleColumn('bomcode', '#BOM', 100);
            column.editor = Slick.Editors.Text;

            var column = igrid.AddSimpleColumn('colourcode', 'Shade', 50);
            column.editor = Slick.Editors.Text;
            igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
            var column = igrid.AddSimpleColumn('width', 'Width', 50);
            column.editor = Slick.Editors.Text;
            var column = igrid.AddFormatedColumn('qty', 'Qty', 70, 'N2', $scope.IsSampleBased ? 4 : 2);
            column.editor = Slick.Editors.Text;
            column.total = true;
            var column = igrid.AddFormatedColumn('rate', 'Rate', 60, 'N2', 2);
            column.editor = Slick.Editors.Text;
            column.total = true;
            var column = igrid.AddFormatedColumn('amount', 'Amount', 70, 'N2', 2);
            column.editor = Slick.Editors.Text;
            column.total = true;
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var colid = args.grid.getColumns()[args.cell].id;
                if (colid == 'bomcode') {

                    restService.Get('BominvoiceitemsService/SearchCode/' + item.bomcode, false).then(function (results) {
                        if (results != null) {
                            item.bomcategoryid = results.bomcategoryid;
                            item.bominvoiceitemid = results.id;

                            item.bomid = results.bomid;
                            item.colourid = results.colourid;
                            item.width = results.width;
                            item.rate = results.buyingprice;
                            item.amount = $rootScope.GetNumber(item.rate) * $rootScope.GetNumber(item.qty);
                            $timeout(function () {
                                $scope.CalculateOrderQty();
                                args.grid.updateRow(args.row);
                                $scope.igrid.UpdateAllTotals(args.grid);
                            }, 10);
                        }
                    });
                }
                else {
                    item.amount = $rootScope.GetNumber(item.rate) * $rootScope.GetNumber(item.qty);
                    $timeout(function () {
                        $scope.CalculateOrderQty();
                        args.grid.updateRow(args.row);
                        $scope.igrid.UpdateAllTotals(args.grid);
                    }, 10);
                }
            });

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $timeout(function () {
                            $scope.ManageItem('Delete', item);
                        }, 10);
                    }
                    else if (command == 'Edit') {
                        $timeout(function () {
                            $scope.ManageItem('Edit', item);
                        }, 10);
                    }
                });
            });
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.BOMRequest.items, '_id');
        }

        $scope.LoadAppSetting = function () {

            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 50)
                return;
            }
            $scope.IsSampleBased = $rootScope.UserInfo.appsettings.mfg_mode == 6;
            if ($scope.IsSampleBased) {
                $scope.BOMRequest.mode = 6;
            }
        }



        $scope.SearchConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('searchGrid', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('entryno', 'Entry No', 80);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 150, $scope.Suppliers);
            igrid.AddReferenceColumn('consumptiontype', 'Type', 100, $scope.ConsumptionTypes);
            igrid.AddReferenceColumn('preparedby', 'Prepared By', 100, $scope.Employee);
            igrid.AddReferenceColumn('approvedby', 'Approved By', 100, $scope.Employee);

            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            if ($scope.UserInfo && $scope.UserInfo.p_BOMRequest &&
                $scope.UserInfo.p_BOMRequest.print) {
                igrid.AddButtonColumn('Print', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' class='btn btn-success btn-xs'><i class='fa fa-print' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $timeout(function () {
                            $scope.ManageSearchItem('Delete', item);
                        }, 10);
                    }
                    else if (command == 'Edit') {
                        $timeout(function () {
                            $scope.ManageSearchItem('Edit', item);
                        }, 10);
                    }
                    else if (command == 'Print') {
                        $timeout(function () {
                            $scope.Print(item.id, 'SM');
                        }, 10);
                    }
                });
            });


            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.SearchResult);
        }


        $timeout(function () {

            if ($location.search().workorderid != undefined &&
                $location.search().workorderid >= 1) {
                request = {
                    "Item": {
                        mfgworkorderid: $location.search().workorderid,
                        mfgjoborderid: $location.search().joborderid,
                        mfgstageid: $location.search().mfgstageid,
                        mode: $location.search().mode,
                        create:true
                    },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BomrequestService/Create",
                    data: srequest
                }).success(function (result) {
                    $scope.BOMRequest = result[0];
                    $scope.BOMRequest.disablemode = $scope.BOMRequest.mode == 6;
                    $scope.BOMRequest.mode = 6;
                    $scope.ItemsConfig();
                });
            }
            else if ($location.search().requestid != undefined &&
                $location.search().requestid >= 1) {
                request = {
                    "Item": {
                        id: $location.search().requestid,
                    },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BomrequestService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.BOMRequest = result[0];
                    $scope.BOMRequest.disablemode = $scope.BOMRequest.mode == 6;
                    $scope.ItemsConfig();
                });
            }
            else {
                $scope.BOMRequest = (JSON.parse(JSON.stringify($scope.Entity)));
                if ($location.search().transfer != undefined &&
                    $location.search().transfer == 'true') {
                    $scope.BOMRequest.mode = 6;
                    $scope.BOMRequest.disablemode = true;
                }
                $scope.ItemsConfig();
            }
            
            $scope.LoadAppSetting();
        }, 50);

    }]);
