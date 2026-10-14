
var app = angular.module('InfinityLab');

app.controller('BOMConsumptionController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', '$state', 'cntxService', '$stateParams',
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

        $scope.ConsumptionTypes = [{ id: 0, name: 'Opening' },
        { id: 1, name: 'Purchase' },
        { id: 2, name: 'Purchase Return' },
        { id: 3, name: 'Transfer In' },
        { id: 4, name: 'Transfer Out' },
        { id: 5, name: 'WorkOrder' },
        { id: 6, name: 'JobOrder' },
        { id: 7, name: 'JobOrde Stage' },
        { id: 8, name: 'JobOrder Outward' },
        { id: 9, name: 'WorkOrder Outward' },
        { id: 10, name: 'Journal' },
        { id: 11, name: 'Internal' },
            { id: 12, name: 'External' },
            { id: 15, name: 'Request' }];

        $scope.bomchange = function () {
            var bom = $scope.BOMAllList.find(function (item) {
                return item.code == $scope.BOMConsumption.Details.bomcode;
            });

            if (typeof (bom) != 'undefined' && bom != null) {
                $scope.BOMConsumption.Details.bomid = bom.id;
            }
        }

        $scope.searchbominwardcode = function (updatestock) {
            if (updatestock) {

                if ($scope.BOMConsumption.locationid == undefined ||
                    $scope.BOMConsumption.locationid == null ||
                    $scope.BOMConsumption.locationid == '' ||
                    $scope.BOMConsumption.locationid <= 0) {
                    $rootScope.Toaster.Error("Please select location!");
                    return;
                }

                if ($scope.BOMConsumption.Details.stockbomcode &&
                    $scope.BOMConsumption.Details.stockbomcode != null &&
                    $scope.BOMConsumption.Details.stockbomcode.length > 0) {
                    restService.Get('BominvoiceitemsService/SearchBOMStock/' + $scope.BOMConsumption.Details.stockbomcode, false).then(function (results) {
                        if (results != null && results.bominvoiceitem != null &&
                            results.stocksummary != null) {
                            var locationstock = results.stocksummary.find(function (item) {
                                return item.locationid == $scope.BOMConsumption.locationid;
                            });
                            if (locationstock && locationstock != null &&
                                locationstock.closing>0) {
                                var consum = $scope.BOMConsumption.BOMConsumptionitems.find(function (item) {
                                    return item.bomid == results.bominvoiceitem.bomid &&
                                        item.colourid == results.bominvoiceitem.colourid;
                                });
                                if (consum.qty < locationstock.closing) {
                                    consum.bomcode = $scope.BOMConsumption.Details.stockbomcode;
                                    consum.bominvoiceitemid = results.bominvoiceitem.id;
                                }
                                else {
                                    var consum1 = (JSON.parse(JSON.stringify(consum)));
                                    consum1.qty = consum.qty - locationstock.closing;
                                    consum1.amount = consum1.qty * consum1.rate;
                                    $scope.BOMConsumption.BOMConsumptionitems.push(consum1);
                                    consum.qty = locationstock.closing;
                                    consum.amount = consum.qty * consum.rate;
                                }
                                $scope.igrid.UpdateData($scope.BOMConsumption.BOMConsumptionitems, '_id');
                                $scope.BOMConsumption.Details.stockbomcode = '';
                            }
                        }
                    });
                    
                }
            }
            else if ($scope.BOMConsumption.Details.bomcode &&
                $scope.BOMConsumption.Details.bomcode != null &&
                $scope.BOMConsumption.Details.bomcode.length > 0) {

                $scope.FindBomStock();

                restService.Get('BominvoiceitemsService/SearchCode/' + $scope.BOMConsumption.Details.bomcode, false).then(function (results) {
                    if (results != null) {
                        $scope.BOMConsumption.Details.bomcategoryid = results.bomcategoryid;
                        $scope.BOMConsumption.Details.bominvoiceitemid = results.id;
                        $scope.BOMConsumption.Details.bomid = results.bomid;
                        $scope.BOMConsumption.Details.colourid = results.colourid;
                        $scope.BOMConsumption.Details.width = results.width;
                        $scope.BOMConsumption.Details.rate = results.buyingprice;
                        $scope.setFocuswidth();
                    }
                });
            }
        }

        $scope.AutoBOMCode = false;
        $scope.categoryChange = function () {
            if (typeof ($scope.BOMConsumption.Details.bomcategoryid) != 'undefined' &&
                $scope.BOMConsumption.Details.bomcategoryid != null) {
                var bomcagtegory = $scope.BOMCategory.find(function (item) {
                    return item.id == $scope.BOMConsumption.Details.bomcategoryid;
                });

                if (typeof (bomcagtegory) != 'undefined' && bomcagtegory != null) {
                    if (bomcagtegory.additionalinfo.autocreate) {
                        $scope.AutoBOMCode = true;
                        $scope.BOMConsumption.Details.bomid = 0;
                        $scope.BOMConsumption.Details.taxid = bomcagtegory.additionalinfo.taxid;
                        $scope.BOMConsumption.Details.uomid = bomcagtegory.additionalinfo.uomid;
                        $scope.BOMConsumption.Details.coretypeid = bomcagtegory.additionalinfo.parentid;

                    }
                    else {
                        $scope.AutoBOMCode = false;
                        $scope.BOMConsumption.Details.bomcode = '';

                    }
                    var BOMList = [];
                    $.each($scope.BOMAllList, function (k, v) {
                        if (v.categoryid == $scope.BOMConsumption.Details.bomcategoryid)
                            BOMList.push(v);
                    });
                    $scope.BOMList = BOMList;
                }
            }
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
            "BOMConsumptionitems": [],
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
        $scope.BOMConsumption = (JSON.parse(JSON.stringify($scope.Entity)));
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
            $scope.BOMConsumption = (JSON.parse(JSON.stringify($scope.Entity)));
            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.UpdateData($scope.BOMConsumption.BOMConsumptionitems, '_id');
        };

        $scope.Save = function () {
            if ($scope.BOMConsumption.companyid == null || $scope.BOMConsumption.companyid == 0) {
                $rootScope.Toaster.Error("Please select company!");
                return;
            }
            if ($scope.BOMConsumption.mode == 'External') {
                if ($scope.BOMConsumption.supplierid == null || $scope.BOMConsumption.supplierid == 0) {
                    $rootScope.Toaster.Error("Please select supplier!");
                    return;
                }
            }
            if ($scope.BOMConsumption.entrydate == null || $scope.BOMConsumption.entrydate == 0) {
                $rootScope.Toaster.Error("Please select PO date!");
                return;
            }

            var request = {
                "item": $scope.BOMConsumption,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMConsumptionService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with Entry No :" + result.entryno);
                    $scope.BOMConsumption.entryno = result.entryno;
                    $scope.BOMConsumption.id = result.id;
                    $scope.BOMConsumption.BOMConsumptionitems = result.BOMConsumptionitems;
                    $scope.ItemsConfig();
                } else if (result != null && result.bomrequestid > 0) {
                    $rootScope.Toaster.Info("Successfully saved with Entry No :" + result.entryno);
                    $scope.BOMConsumption.entryno = result.entryno;
                    $scope.BOMConsumption.bomrequestid = result.bomrequestid;
                    $scope.BOMConsumption.BOMConsumptionitems = result.BOMConsumptionitems;
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
            if ($scope.BOMConsumption.Details.bomid != null) {
                var prditem = $scope.BOMList.find(function (item) {
                    return (item.id == $scope.BOMConsumption.Details.bomid);
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
            $scope.BOMConsumption.Details.colourid = null;
            $scope.BOMConsumption.Details.bominwardcode = '';
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
                $scope.BOMConsumption.Details.bominwardcode = prditem.code;
                $scope.BOMConsumption.Details.imageid = prditem.imageid;
            }
            if ($scope.ColorList.length == 1) {
                $scope.BOMConsumption.Details.colourid = $scope.ColorList[0].id;
                $scope.FindBomStock();
            }
            else if ($scope.ColorList.length == 0) {
                $scope.FindBomStock();
            }
        }

        $scope.ChangeOrderStatus = function () {
            if ($scope.BOMConsumption.id == null || $scope.BOMConsumption.id == 0) {
                $rootScope.Toaster.Error("Please select purchase order!");
                return;
            }

            var request = {
                "item": $scope.BOMConsumption,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMConsumptionService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Status updated successfully");

            }).error(function (result) {
            });

        }


        $scope.FindBomStock = function () {
            
            var request = {
                "item": {
                    'bomcode': $scope.BOMConsumption.Details.bomcode,
                    'bomid': $scope.BOMConsumption.Details.bomid,
                    'colourid': $scope.BOMConsumption.Details.colourid,
                    'locationid': $scope.BOMConsumption.locationid,
                    'companyid': $scope.BOMConsumption.companyid,
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMConsumptionService/FindBOMStock",
                data: srequest
            }).success(function (result) {
                $scope.avaialblestock = result == null ? 'No Stock':result;
            }).error(function (result) {
            });
        }

        $scope.colourchanged = function () {
            $scope.FindBomStock();
            $scope.BOMConsumption.Details.colourcode = '';
            var colourcode = $scope.ColorList.find(function (item) {
                return item.id == $scope.BOMConsumption.Details.colourid;
            });
            if (colourcode && colourcode != null)
                $scope.BOMConsumption.Details.colourcode = colourcode.name;
        }
        $scope.AddItem = function () {
            if ($scope.BOMConsumption.companyid == null || $scope.BOMConsumption.companyid == 0) {
                $scope.Toaster.Info("Please enter company");
                return;
            }


            if ($scope.BOMConsumption.locationid == null || $scope.BOMConsumption.locationid == 0) {
                $scope.Toaster.Info("Please select the location");
                return;
            }

            if ($scope.BOMConsumption.Details.bomcategoryid == null || $scope.BOMConsumption.Details.bomcategoryid == 0) {
                $scope.Toaster.Info("Please Select BOM Category");
                return;
            }

            item = {
                'bomcategoryid': $scope.BOMConsumption.Details.bomcategoryid,
                'bomcode': $scope.BOMConsumption.Details.bomcode,
                'colourcode': $scope.BOMConsumption.Details.colourcode,
                'bomid': $scope.BOMConsumption.Details.bomid,
                'qty': $scope.BOMConsumption.Details.qty,
                'taxid': $scope.BOMConsumption.Details.taxid,
                'uomid': $scope.BOMConsumption.Details.uomid,
                'coretypeid': $scope.BOMConsumption.Details.coretypeid,
                'isactive': true,
                'version': 1,
                'imageid': $scope.BOMConsumption.Details.imageid,
                'colourid': $scope.BOMConsumption.Details.colourid,
                'width': $scope.BOMConsumption.Details.width,
                'rate': $scope.BOMConsumption.Details.rate,
                'amount': 0,
                'id': 0
            };

            if (typeof (item.imageid) != 'undefined' && item.imageid != null &&
                item.imageid.length > 0)
                $scope.BOMConsumption.ShowImage = true;

            item.amount = item.rate * item.qty;
            $scope.BOMConsumption.BOMConsumptionitems.splice(0, 0, item);
            if ($scope.BOMConsumption.BOMConsumptionitems.length == 1)
                $scope.ItemsConfig();
            else
                $scope.igrid.UpdateData($scope.BOMConsumption.BOMConsumptionitems, '_id');
            $scope.BOMConsumption.Details.id = 0;
            $scope.avaialblestock = '';
            $scope.BOMConsumption.Details.qty = '';
            $scope.BOMConsumption.Details.rate = '';
            $scope.BOMConsumption.Details.imageid = '';
            $scope.CalculateOrderQty();
            $scope.setFocusbomcategoryid();
        };

        $scope.CalculateOrderQty = function () {
            $scope.orderqty = 0;
            $scope.orderamount = 0;
            $.each($scope.BOMConsumption.BOMConsumptionitems, function (k, v) {
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
                $scope.BOMConsumption.Details = item;
                var bomid = $scope.BOMConsumption.Details.bomid;
                var colourid = $scope.BOMConsumption.Details.colourid;
                $scope.categoryChange();
                $scope.productChange();
                $scope.BOMConsumption.Details.bomid = null;
                $scope.BOMConsumption.Details.colourid = null;
                $timeout(function () {
                    $scope.BOMConsumption.Details.bomid = bomid;
                    $scope.BOMConsumption.Details.colourid = colourid;
                    $scope.BOMConsumption.BOMConsumptionitems.splice($scope.BOMConsumption.BOMConsumptionitems.indexOf(item), 1);
                    $scope.ItemsConfig();
                    $scope.CalculateOrderQty();
                }, 10);
            } else if (type == 'Delete') {
                $scope.BOMConsumption.BOMConsumptionitems.splice($scope.BOMConsumption.BOMConsumptionitems.indexOf(item), 1);
                $scope.ItemsConfig();
                $scope.CalculateOrderQty();
            }
        };

        $scope.ImportBOM = function () {
            var request = {
                "Item": $scope.BOMConsumption,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMConsumptionService/ImportBOM",
                data: srequest
            }).success(function (result) {
                $scope.BOMConsumption.BOMConsumptionitems = result;
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
                $scope.BOMConsumption = item;
                var request = {
                    "Item": {
                        'id': item.id
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "BOMConsumptionService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.BOMConsumption = result[0];
                    $scope.BOMConsumption.mode = $scope.BOMConsumption.isoutward ? 'External' : 'Internal';
                    $scope.BOMConsumption.allowconsumptiontype = $scope.BOMConsumption.mfgworkorderid > 0;
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
                                    url: "BOMConsumptionService/Delete",
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
            $scope.BOMConsumption.Details.oqty = 0;
            $.each($scope.JumpList, function (k, v) {
                if ($rootScope.GetNumber(v.qty) == 'undefined' || $rootScope.GetNumber(v.qty) == 0)
                    $scope.BOMConsumption.Details.oqty += 0;
                else
                    $scope.BOMConsumption.Details.oqty += $rootScope.GetNumber(v.qty);
            });
        }

        $scope.CheckSize = function () {

            if (typeof ($scope.BOMConsumption.Details.size) != 'undefined' &&
                $scope.BOMConsumption.Details.size != null && $scope.BOMConsumption.mode == 0) {
                $scope.GetJUMPInformation($scope.BOMConsumption.Details.size);
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
                //$scope.BOMConsumption.Details.qty = 0;
                $('#jumplist INPUT').first().focus();
            }
        }

        $scope.SearchItem = function () {

            if ($scope.BOMConsumption.Details.productid == null || $scope.BOMConsumption.Details.productid == 0) {
                $scope.Toaster.Info("Please select product");
                return;
            }

            $scope.BOMConsumption.Details.id = 0;
            $scope.BOMConsumption.Details.brandname = $scope.SearchRef($scope.BOMConsumption.Details.brandid, $scope.BrandName, '');
            $scope.BOMConsumption.Details.printingname = $scope.SearchRef($scope.BOMConsumption.Details.productid, $scope.ProductList, '');
            $scope.BOMConsumption.Details.patternname = $scope.SearchRef($scope.BOMConsumption.Details.patternid, $scope.PatternList, '');
            $scope.BOMConsumption.Details.materialname = $scope.SearchRef($scope.BOMConsumption.Details.materialid, $scope.MaterialList, '');
            $scope.BOMConsumption.Details.colourname = $scope.SearchRef($scope.BOMConsumption.Details.colourid, $scope.ColorList, '');
            $scope.BOMConsumption.Details.sizename = $scope.SearchRef($scope.BOMConsumption.Details.sizeid, $scope.SizeList, '');
            $scope.BOMConsumption.Details.stylename = $scope.SearchRef($scope.BOMConsumption.Details.styleid, $scope.StyleList, '');
            $scope.BOMConsumption.Details.typename = $scope.SearchRef($scope.BOMConsumption.Details.typeid, $scope.TypeList, '');
            $scope.BOMConsumption.Details.autocreate = true;

            var request = {
                "Item": $scope.BOMConsumption.Details,
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
                    $scope.BOMConsumption.Details = result[0];
                    $scope.BOMConsumption.Actual = (JSON.parse(JSON.stringify($scope.BOMConsumption.Details)));
                }
                else if (result.length > 1)
                    $scope.ShowProductSearch(result);
            });
        };

        $scope.SearchItemByName = function () {
            var request = {
                "Item": {
                    'name': $scope.BOMConsumption.Details.printingname, 'productid': $scope.BOMConsumption.Details.productid
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
                    $scope.BOMConsumption.Details = result[0];
                    $scope.BOMConsumption.Actual = (JSON.parse(JSON.stringify($scope.BOMConsumption.Details)));
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
                url: "BOMConsumptionService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.SearchConfig();
            });
        };

        $scope.Publish = function () {
            restService.Get('BOMConsumptionService/Publish/' + $scope.UserInfo.OrganizationID + '/' + $scope.BOMConsumption.id, true).then(function (results) {
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
                link.href = 'BOMConsumptionService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + mode + '/' + id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.fileuploadedcomplete = function (items) {
            if (typeof ($scope.BOMConsumption) == 'undefined' || $scope.BOMConsumption == null)
                $scope.BOMConsumption = {};
            $scope.BOMConsumption.BOMConsumptionitems = items;
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
            $.each($scope.BOMConsumption.BOMConsumptionitems, function (k, v) {
                v.sno = k + 1;
            });
            if ($scope.igrid != null) {
                //if ($scope.BOMConsumption.BOMConsumptionitems.length > 1) {
                //    $scope.igrid.UpdateData($scope.BOMConsumption.BOMConsumptionitems, '_id');
                //    return;
                //}
                $scope.igrid.destroy();
            }

            var igrid = new InfyGrid('editGrid', '400px', '100%', true, true, true,
                ($scope.BOMConsumption && $scope.BOMConsumption.isrequest) ? 30: 0);
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
            var column = igrid.AddFormatedColumn('qty', 'Qty', 70, 'N2', 2);
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
            $scope.igrid.UpdateData($scope.BOMConsumption.BOMConsumptionitems, '_id');
        }

        $scope.ShowExisting = function (items) {

            var dialog = ngDialog.open({
                template: 'ChooseExisting',
                data: {
                    'List': items,
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.List = $scope.ngDialogData.List;
                    $scope.ParentScope = $scope.ngDialogData.scope;
                    $scope.Selected = function (item) {
                        $scope.closeThisDialog(item);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$escape' && data.value != '$document') {
                        if (data.value.id > 0) {
                            $scope.ManageSearchItem('Edit', data.value);
                        }
                    }
                }
            });

        }

        $scope.SearchConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('searchGrid', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('entryno', 'Entry No', 80);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('workorderno', 'Work Order No', 80);
            igrid.AddSimpleColumn('joborderno', 'Job Order No', 80);
            igrid.AddSimpleColumn('referenceno', 'Reference No', 100);
            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 150, $scope.Suppliers);
            igrid.AddReferenceColumn('consumptiontype', 'Type', 100, $scope.ConsumptionTypes);
            igrid.AddReferenceColumn('preparedby', 'Prepared By', 100, $scope.Employee);
            igrid.AddReferenceColumn('approvedby', 'Approved By', 100, $scope.Employee);

            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            if ($scope.UserInfo && $scope.UserInfo.p_BOMCONSUMPTION &&
                $scope.UserInfo.p_BOMCONSUMPTION.print) {
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

   
        if ($location.search().workorderid != undefined &&
            $location.search().workorderid >= 1) {
            request = {
                "Item": {
                    mfgworkorderid: $location.search().workorderid,
                    mfgjoborderid: $location.search().joborderid,
                    mfgstageid: $location.search().mfgstageid,
                    'create': true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMConsumptionService/Search",
                data: srequest
            }).success(function (result) {

                // alert(result.items.length);
                $scope.BOMConsumption.workorderno = result[0].workorderno;
                $scope.BOMConsumption.joborderno = result[0].joborderno;
                $scope.BOMConsumption.companyid = result[0].companyid;
                $scope.BOMConsumption.locationid = result[0].locationid;
                $scope.BOMConsumption.supplierid = result[0].supplierid;
                $scope.BOMConsumption.mfgworkorderid = result[0].mfgworkorderid;
                $scope.BOMConsumption.mfgjoborderid = result[0].mfgjoborderid;
                $scope.BOMConsumption.allowconsumptiontype = true;
                $scope.BOMConsumption.isoutward = $location.search().isoutward == 'true';
                $scope.BOMConsumption.isconsumption = $location.search().isconsumption == 'true';
                $scope.BOMConsumption.isrequest = $location.search().isrequest == 'true';
                $scope.BOMConsumption.mode = $scope.BOMConsumption.isoutward ? 'External' : 'Internal';
                if ($scope.BOMConsumption.isrequest) {
                    $scope.BOMConsumption.consumptiontype = 15;
                    $scope.BOMConsumption.mfgstageid = result[0].mfgstageid;
                    $scope.BOMConsumption.BOMConsumptionitems = result[0].BOMConsumptionitems;
                    $scope.ItemsConfig();
                }
                else {
                    if ($location.search().joborderid && $location.search().joborderid > 0)
                        $scope.BOMConsumption.consumptiontype = 8;
                    else if ($location.search().workorderid && $location.search().workorderid > 0)
                        $scope.BOMConsumption.consumptiontype = 9;
                }

                if (result[0].id > 0) {
                    $scope.ShowExisting(result);
                }
            });
        }
        else if ($location.search().requestid != undefined &&
            $location.search().requestid >= 1) {
            request = {
                "Item": {
                    id: $location.search().requestid
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BomrequestService/Search",
                data: srequest
            }).success(function (result) {
                if (result[0].bomconsumptionid > 0) {
                    $scope.ManageSearchItem('Edit',{ 'id': result[0].bomconsumptionid })
                }
                else {
                    $scope.BOMConsumption.bomrequestid = result[0].id;
                    $scope.BOMConsumption.workorderno = result[0].workorderno;
                    $scope.BOMConsumption.joborderno = result[0].joborderno;
                    $scope.BOMConsumption.companyid = result[0].companyid;
                    $scope.BOMConsumption.locationid = result[0].fromlocationid;
                    $scope.BOMConsumption.supplierid = result[0].supplierid;
                    $scope.BOMConsumption.mfgworkorderid = result[0].mfgworkorderid;
                    $scope.BOMConsumption.mfgjoborderid = result[0].mfgjoborderid;
                    $scope.BOMConsumption.isoutward = result[0].mode == 4 || result[0].mode == 5;
                    $scope.BOMConsumption.isconsumption = !$scope.BOMConsumption.isoutward;
                    $scope.BOMConsumption.mode = $scope.BOMConsumption.isoutward ? 'External' : 'Internal';
                    $scope.BOMConsumption.allowconsumptiontype = true;
                    $scope.BOMConsumption.BOMConsumptionitems = result[0].items;
                    switch (result[0].mode) {
                        case 1:
                            $scope.BOMConsumption.consumptiontype = 7;
                            break;
                        case 2:
                            $scope.BOMConsumption.consumptiontype = 6;
                            break;
                        case 3:
                            $scope.BOMConsumption.consumptiontype = 5;
                            break;
                        case 4:
                            $scope.BOMConsumption.consumptiontype = 9;
                            break;
                        case 5:
                            $scope.BOMConsumption.consumptiontype = 8;
                            break;
                        default:
                            $scope.BOMConsumption.allowconsumptiontype = false;
                            break;
                    }
                    $scope.BOMConsumption.isrequest = true;
                    $scope.ItemsConfig();
                }
            });
        }
        else {
            $scope.BOMConsumption = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.BOMConsumption.isoutward = $location.search().isoutward == 'true';
            $scope.ItemsConfig();
        }

    }]);
