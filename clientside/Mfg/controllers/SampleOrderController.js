var app = angular.module('InfinityLab');
app.controller('SampleOrderController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', '$state', 'cntxService', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, $state, cntxService, $stateParams) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.orderqty = 0;
        $scope.orderamount = 0;
        $scope.ProductForms = restService.Get('ReferencelistService/MFGPRDFORM', true).then(function (results) {
            $scope.ProductForms = results;
        });
        $scope.Productlist = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Productlist = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.Machines = restService.Get('ReferencelistService/FULL-MFGPRDMACHINE', true).then(function (results) {
            $scope.Machines = results;
        });
        $scope.Status = restService.Get('ReferencelistService/FULL-MFGSAMPLESTATUS', true).then(function (results) {
            $scope.Status = results;
        });
        $scope.YarnList = [];
        $scope.BOMAllList = [];
        restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMAllList = results;
            $.each(results, function (k, v) {
                if (v.coretypeid == 3)
                    $scope.YarnList.push(v);
            })
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'SampleOrder.companyid');
            $rootScope.BindCompany($scope, 'Entity.companyid');
        });

        $scope.JumpList = [];
        $scope.ColorList = [];
        $scope.AllColorList = [];
        restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });
        $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMER', true).then(function (results) {
            //results.push({ 'id': -1, 'name': 'ALL CUSTOMER' });
            $scope.Customers = results;

        });
        $scope.LocationList = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.LocationList = results;
            $rootScope.BindLocation($scope, 'SampleOrder.locationid');
            $rootScope.BindLocation($scope, 'Entity.locationid');
        });
        $scope.SampleQualityList = restService.Get('ReferencelistService/MFGSAMPLEQUALITY', true).then(function (results) {
            $scope.SampleQualityList = results;
        });


        $scope.ProductDivisionChanged = function () {
            var bom = $scope.ProductDivision.find(function (item) {
                return item.id == $scope.SampleOrder.prddivisionid;
            });
            $scope.SampleOrder.sampleconfig = {};
            if (bom && bom != null &&
                bom.additionalinfo != null &&
                bom.additionalinfo.mfglabelconfig != null) {
                $scope.SampleOrder.mfglabelconfig = bom.additionalinfo.mfglabelconfig;
            }
        }

        $scope.CreateSize = function () {
            $rootScope.Prompt('Size', 'Size', function (code) {
                code = code.toString();
                if (typeof (code) != 'undefined' && code.length > 0) {
                    var exist = $scope.SizeList.find(function (item) {
                        return item.name == code || item.code == code;
                    });
                    if (exist == undefined || exist == null) {
                        var request = {
                            "Item": { 'isactive': true, 'name': code, 'type': 'SIZE' },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "ReferencelistService/ManageAttribute",
                            data: srequest
                        }).success(function (result) {
                            $scope.SizeList.push(result);
                            var lastlist = $scope.SizeList;
                            $scope.SizeList = null;
                            $timeout(function () {
                                $scope.SizeList = lastlist;
                            }, 10);
                        });
                    }
                    else {
                        $rootScope.Toaster.Error("This size already exist!");
                    }
                }
            })
        }

        $scope.AddSize = function () {
            if ($scope.SampleOrder.Sizes == undefined || $scope.SampleOrder.Sizes == null)
                $scope.SampleOrder.Sizes = [];
            $scope.SampleOrder.Sizes.push({ 'id': $scope.SampleOrder.sizeid });
            $scope.SizeConfig();
        }

        $scope.SizeConfig = function () {
            if ($scope.SampleOrder.Sizes == undefined || $scope.SampleOrder.Sizes == null)
                $scope.SampleOrder.Sizes = [];
            $.each($scope.SampleOrder.Sizes, function (k, v) {
                v.sno = k + 1;
            });
            if ($scope.sizegrid != null) {
                $scope.sizegrid.destroy();
            }
            var igrid = new InfyGrid('sizeGrid', '400px', '100%', true, true, true, 100);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('id', 'Size', 100, $scope.SizeList);
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $timeout(function () {
                            $scope.ManageItem('DeleteSize', item);
                        }, 10);
                    }
                });
            });
            $scope.sizegrid = igrid;
            $scope.sizegrid.UpdateData($scope.SampleOrder.Sizes, '_id');
        }

        $scope.DesignInfo = function (id) {
            restService.Get('MfgsampleorderService/MfgDesign/' + id, true).then(function (results) {
                $scope.MfgDesignInfo = results;
                $rootScope.Toaster.Info(results.message);
            });
        }

        $scope.CurrentDesignItem = null;
        $scope.ManageItem = function (command, item) {

            if (command == 'EditDesignItem') {

                $scope.SampleOrder.DesignItems.splice($scope.SampleOrder.DesignItems.indexOf(item), 1);
                $scope.dgrid.UpdateData($scope.SampleOrder.DesignItems, '_id');
                var designitem = item;
                $scope.SampleOrder.design = {};
                $scope.SampleOrder.design.sno = item.sno;
                $scope.SampleOrder.design.itemuid = item.uid;
                if ($scope.SampleOrder.mfglabelconfig.design_bom) {
                    $scope.SampleOrder.design.design_bom = designitem.design_bom;
                }
                if ($scope.SampleOrder.mfglabelconfig.design_colour) {
                    $scope.SampleOrder.design.design_colour = designitem.design_colour;
                }
                if ($scope.SampleOrder.mfglabelconfig.design_thickness) {
                    $scope.SampleOrder.design.design_thickness = designitem.design_thickness;
                }
                if ($scope.SampleOrder.mfglabelconfig.design_shade) {
                    $scope.SampleOrder.design.design_shade = designitem.design_shade;
                }
                if ($scope.SampleOrder.mfglabelconfig.design_picks) {
                    $scope.SampleOrder.design.design_picks = designitem.design_picks;
                }
                if ($scope.SampleOrder.mfglabelconfig.design_frontcolour) {
                    $scope.SampleOrder.design.design_frontcolour = designitem.design_frontcolour;
                }
                if ($scope.SampleOrder.mfglabelconfig.design_backcolour) {
                    $scope.SampleOrder.design.design_backcolour = designitem.design_backcolour;
                }
                if ($scope.SampleOrder.mfglabelconfig.design_no) {
                    $scope.SampleOrder.design.design_no = designitem.design_no;
                }
                if ($scope.SampleOrder.mfglabelconfig.design_yarnqty) {
                    designitem.design_yarnqty = $scope.SampleOrder.design.design_yarnqty;
                }
            }

            if (command == 'EditDesign') {
                if ($scope.CurrentDesignItem != null) {
                    $rootScope.Toaster.Error("Please add edited design before editing another design!");
                    return;
                }
                $scope.CurrentDesignItem = item;
                $scope.SampleOrder.design = {};
                $scope.SampleOrder.design.uid = item.uid;
                $scope.SampleOrder.design.imageid = item.imageid;
                $scope.SampleOrder.design.design_name = item.design_name;
                $scope.SampleOrder.design.requiredhours = item.requiredhours;
                $scope.SampleOrder.design.requiredbom = item.requiredbom;
                $scope.SampleOrder.design.requiredyarn = item.requiredyarn;
                $scope.SampleOrder.design.requiredcost = item.requiredcost;
                $scope.SampleOrder.design.requiredprice = item.requiredprice;
                $scope.SampleOrder.design.tpicks = item.tpicks;

                $scope.SampleOrder.DesignItems = item.items;
                $scope.SampleOrder.Designs.splice($scope.SampleOrder.Designs.indexOf(item), 1);
                $scope.designgrid.UpdateData($scope.SampleOrder.Designs, '_id');
                $scope.ConfigDesignItemGrid();
            }

            if (command == 'DeleteDesignItem') {
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
                                $scope.SampleOrder.DesignItems.splice($scope.SampleOrder.DesignItems.indexOf(item), 1);
                                $timeout(function () {
                                    $scope.dgrid.UpdateData($scope.SampleOrder.DesignItems, '_id');
                                }, 20);
                            }
                        },
                        cancel: function () {
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
            }

            if (command == 'DeleteDesign') {
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
                                $scope.SampleOrder.Designs.splice($scope.SampleOrder.Designs.indexOf(item), 1);
                                $timeout(function () {
                                    $scope.designgrid.UpdateData($scope.SampleOrder.Designs, '_id');
                                }, 20);
                            }
                        },
                        cancel: function () {
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
            }
            if (command == 'DeleteSize') {
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
                                $scope.SampleOrder.Sizes.splice($scope.SampleOrder.Sizes.indexOf(item), 1);
                                $timeout(function () {
                                    $scope.sizegrid.UpdateData($scope.SampleOrder.Sizes, '_id');
                                }, 20);
                            }
                        },
                        cancel: function () {
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
            }
        }

        $scope.AddDesign = function () {
            if ($scope.SampleOrder.mfglabelconfig == undefined || $scope.SampleOrder.mfglabelconfig == null || $scope.SampleOrder.design == undefined || $scope.SampleOrder.design == null) return;
            var designitem = {
                'imageid': $scope.SampleOrder.design.imageid,
                'requiredhours': $scope.SampleOrder.design.requiredhours,
                'requiredbom': $scope.SampleOrder.design.requiredbom,
                'requiredyarn': $scope.SampleOrder.design.requiredyarn,
                'requiredcost': $scope.SampleOrder.design.requiredcost,
                'requiredprice': $scope.SampleOrder.design.requiredprice,
                'design_name': $scope.SampleOrder.design.design_name,
                'items': $scope.SampleOrder.DesignItems
            };
            $scope.SampleOrder.Designs.push(designitem);
            $scope.CurrentDesignItem = null;
            $scope.ItemsConfig();
            $scope.SampleOrder.design = {};
            $scope.SampleOrder.DesignItems = [];
            $scope.ConfigDesignItemGrid();
        }

        $scope.CurrentDesignChanged = function () {
            $scope.CurrentDesign = $scope.SampleOrder.Designs.find(function (item) {
                return item.design_name == $scope.currentdesign_name;
            });
            if ($scope.CurrentDesign && $scope.CurrentDesign != null) {
                $scope.SampleOrder.DesignItems = $scope.CurrentDesign.items;
                $scope.ConfigDesignItemGrid();
            }
        }

        $scope.ItemsConfig = function () {
            if ($scope.SampleOrder.Designs == undefined || $scope.SampleOrder.Designs == null)
                $scope.SampleOrder.Designs = [];
            $.each($scope.SampleOrder.Designs, function (k, v) {
                v.sno = k + 1;
                v.tpicks = 0;
                if (v.items && v.items != null) {
                    $.each(v.items, function (kk, vv) {
                        v.tpicks += vv.design_picks;
                    });
                }
            });
            if ($scope.designgrid != null) {
                $scope.designgrid.destroy();
            }
            var igrid = new InfyGrid('designGrid', '400px', '100%', true, true, true, 290);
            igrid.options.showHeaderRow = true;
            igrid.options.rowHeight = 55;
            igrid.AddSimpleColumn('sno', 'S.No', 50);

            var column = igrid.AddSimpleColumn('design_name', 'Name', 200);
            if ($scope.SampleOrder.mfglabelconfig && $scope.SampleOrder.mfglabelconfig.tpicks) {
                igrid.AddSimpleColumn('tpicks', 'T.Picks', 80);
            }

            column = igrid.AddCustomColumn('ImageID', '', 150, function (item, col) {
                if (item.imageid && item.imageid != null && item.imageid != '') {
                    var urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';
                    var imgtemplate = '<div style="height: 50px;display: flex;align-items: center;justify-content: center;"><img style="width:100%;height:100%" src="' + urlprefix + item.imageid + '"/></div>';
                    return $("<div />").append(imgtemplate).html();
                }
                return '';
            });

            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $timeout(function () {
                            $scope.ManageItem('DeleteDesign', item);
                        }, 10);
                    }
                    else if (command == 'Edit') {
                        $timeout(function () {
                            $scope.ManageItem('EditDesign', item);
                        }, 10);
                    }
                });
            });
            $scope.designgrid = igrid;
            $scope.designgrid.UpdateData($scope.SampleOrder.Designs, '_id');
        }

        $scope.AddDesignItem = function () {
            if ($scope.SampleOrder.mfglabelconfig == undefined || $scope.SampleOrder.mfglabelconfig == null) return;

            var designitem = {};

            if ($scope.SampleOrder.design.sno) {
                designitem.sno = $scope.SampleOrder.design.sno;
                $scope.SampleOrder.design.sno = null;
            }

            if ($scope.SampleOrder.mfglabelconfig.design_bom) {
                designitem.design_bom = $scope.SampleOrder.design.design_bom;
                $scope.SampleOrder.design.design_bom = null;
            }
            if ($scope.SampleOrder.mfglabelconfig.design_colour) {
                designitem.design_colour = $scope.SampleOrder.design.design_colour;
                $scope.SampleOrder.design.design_colour = null;
            }
            if ($scope.SampleOrder.mfglabelconfig.design_thickness) {
                designitem.design_thickness = $scope.SampleOrder.design.design_thickness;
                $scope.SampleOrder.design.design_thickness = null;
            }
            if ($scope.SampleOrder.mfglabelconfig.design_shade) {
                designitem.design_shade = $scope.SampleOrder.design.design_shade;
                $scope.SampleOrder.design.design_shade = null;
            }
            if ($scope.SampleOrder.mfglabelconfig.design_picks) {
                designitem.design_picks = $scope.SampleOrder.design.design_picks;
                $scope.SampleOrder.design.design_picks = null;
            }
            if ($scope.SampleOrder.mfglabelconfig.design_frontcolour) {
                designitem.design_frontcolour = $scope.SampleOrder.design.design_frontcolour;
                $scope.SampleOrder.design.design_frontcolour = null;
            }
            if ($scope.SampleOrder.mfglabelconfig.design_backcolour) {
                designitem.design_backcolour = $scope.SampleOrder.design.design_backcolour;
                $scope.SampleOrder.design.design_backcolour = null;
            }
            if ($scope.SampleOrder.mfglabelconfig.design_no) {
                designitem.design_no = $scope.SampleOrder.design.design_no;
                $scope.SampleOrder.design.design_no = null;
            }
            if ($scope.SampleOrder.mfglabelconfig.design_yarnqty) {
                designitem.design_yarnqty = $scope.SampleOrder.design.design_yarnqty;
                $scope.SampleOrder.design.design_yarnqty = null;
            }
            if ($scope.SampleOrder.DesignItems == undefined || $scope.SampleOrder.DesignItems == null)
                $scope.SampleOrder.DesignItems = [];

            if ($scope.SampleOrder.design == undefined || $scope.SampleOrder.design == null)
                $scope.SampleOrder.design = {};
            $scope.SampleOrder.design.itemuid = '';
            if ($scope.SampleOrder.DesignItems.indexOf(designitem) < 0)
                $scope.SampleOrder.DesignItems.push(designitem);
            $scope.ConfigDesignItemGrid();
        }

        $scope.ConfigDesignItemGrid = function () {
            if ($scope.SampleOrder.DesignItems == undefined || $scope.SampleOrder.DesignItems == null)
                $scope.SampleOrder.DesignItems = [];
            $.each($scope.SampleOrder.DesignItems, function (k, v) {
                if (v.sno == undefined || v.sno == null || v.sno == 0)
                    v.sno = k + 1;
            });

            if ($scope.dgrid != null) {
                $scope.dgrid.destroy();
            }
            var igrid = new InfyGrid('designItemGrid', '400px', '100%', true, true, true, 300);
            igrid.options.showHeaderRow = true;

            //igrid.AddSimpleColumn('sno', 'S.No', 40);
            igrid.AddColumn(
                {
                    id: "sno", name: "S.No", field: "sno",
                    width: 50, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            if ($scope.SampleOrder.mfglabelconfig == undefined || $scope.SampleOrder.mfglabelconfig == null) {
                igrid.AddReferenceColumn('design_colour', 'Colour', 100, $scope.ColorList);
                var column = igrid.AddSimpleColumn('design_thickness', 'Thickness', 50);
                var column = igrid.AddSimpleColumn('design_picks', 'Picks', 50);
            }
            else {
                if ($scope.SampleOrder.mfglabelconfig.design_bom) {
                    igrid.AddReferenceColumn('design_bom', 'BOM', 100, $scope.BOMAllList);
                }
                if ($scope.SampleOrder.mfglabelconfig.design_colour) {
                    igrid.AddReferenceColumn('design_colour', 'Colour', 100, $scope.ColorList);
                }
                if ($scope.SampleOrder.mfglabelconfig.design_thickness) {
                    var column = igrid.AddSimpleColumn('design_thickness', 'Thickness', 50);
                }
                if ($scope.SampleOrder.mfglabelconfig.design_colour) {
                    igrid.AddReferenceColumn('design_shade', 'Shade', 100, $scope.YarnList);
                }
                if ($scope.SampleOrder.mfglabelconfig.design_picks) {
                    var column = igrid.AddSimpleColumn('design_picks', 'Picks', 50);
                }
                if ($scope.SampleOrder.mfglabelconfig.design_frontcolour) {
                    igrid.AddReferenceColumn('design_frontcolour', 'Front Colour', 100, $scope.ColorList);
                }
                if ($scope.SampleOrder.mfglabelconfig.design_backcolour) {
                    igrid.AddReferenceColumn('design_backcolour', 'Back Colour', 100, $scope.ColorList);
                }
                if ($scope.SampleOrder.mfglabelconfig.design_yarnqty) {
                    var column = igrid.AddSimpleColumn('design_yarnqty', 'Y.Qty', 50);
                }
            }
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Delete') {
                        $timeout(function () {
                            $scope.ManageItem('DeleteDesignItem', item);
                        }, 10);
                    }
                    else if (command == 'Edit') {
                        $timeout(function () {
                            $scope.ManageItem('EditDesignItem', item);
                        }, 10);
                    }
                });
            });
            $scope.dgrid = igrid;
            $scope.dgrid.UpdateData($scope.SampleOrder.DesignItems, '_id');
        }

        $scope.designimageuploaded = function (value) {
            $scope.SampleOrder.design.imageid = value[0];
            $scope.SampleOrder.design.timageid = null;
        }

        $scope.imageuploaded = function (value) {
            $scope.SampleOrder.imageid = value[0];
            $scope.SampleOrder.timageid = null;
        }


        restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agents = results;
        });
        $scope.ShowStockView = false;

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
        $scope.SampleOrder = (JSON.parse(JSON.stringify($scope.Entity)));
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
            $scope.SampleOrder = (JSON.parse(JSON.stringify($scope.Entity)));
            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.UpdateData($scope.SampleOrder.Designs, '_id');
        };

        $scope.Print = function (id) {
            $rootScope.Download('MfgsampleorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + id, 'sampleorder-' + id + '.pdf', 'application/pdf');
        };

        $scope.SaveRequest = false;
        $scope.Save = function () {

            //if ($scope.SampleOrder.designno == undefined || $scope.SampleOrder.designno == null || $scope.SampleOrder.designno == '') {
            //    $rootScope.Toaster.Error("Please enter the design name!");
            //    return;
            //}

            if ($scope.SampleOrder.companyid == null || $scope.SampleOrder.companyid == 0) {
                $rootScope.Toaster.Error("Please select company!");
                return;
            }
            if ($scope.SampleOrder.customerid == null || $scope.SampleOrder.customerid == 0) {
                $rootScope.Toaster.Error("Please select customer!");
                return;
            }

            if ($scope.SampleOrder.entrydate == null || $scope.SampleOrder.entrydate == 0) {
                $rootScope.Toaster.Error("Please select sample date!");
                return;
            }

            if ($scope.CurrentDesignItem != null) {
                $rootScope.Toaster.Error("Please add edited design before saving design!");
                return;
            }

            if ($scope.SaveRequest)
                return;

            if ($scope.SampleOrder && $scope.SampleOrder.id && $scope.SampleOrder.id > 0) {
                var dialog = ngDialog.open({
                    template: 'HistoryDialog',
                    data: {
                        'scope': $scope,
                        'SampleOrder': $scope.SampleOrder
                    },
                    width: '500px',
                    controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                        $scope.Parent = $scope.ngDialogData.scope;
                        $scope.SampleOrder = $scope.ngDialogData.SampleOrder;
                        if ($scope.SampleOrder.history == undefined || $scope.SampleOrder.history == null)
                            $scope.SampleOrder.history = [];

                        $scope.Selected = function () {
                            var exist = $scope.SampleOrder.history.find(function (item) {
                                return item.actionby == undefined || item.actionby == 0
                            });
                            if (exist == undefined || exist == null) {
                                exist = { 'actionby': 0 };
                                $scope.SampleOrder.history.push(exist);
                            }

                            exist.employee = $scope.Parent.SearchRef($scope.approvedby, $scope.Parent.Employee);
                            exist.approvedby = $scope.approvedby;
                            exist.reason = $scope.reason;
                            exist.user = $scope.UserInfo.userName;
                            $scope.closeThisDialog($scope.SampleOrder);
                        };
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$escape' && data.value != '$document') {
                        $scope.InternalSave();
                    }
                });
            }
            else {
                $scope.InternalSave();
            }
        }

        $scope.InternalSave = function () {
            if (!$scope.SaveRequest) {
                $scope.SaveRequest = true;
            }
            else {
                return;
            }

            var request = {
                "item": $scope.SampleOrder,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgsampleorderService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with Entry No :" + result.entryno);
                    $scope.SampleOrder.id = result.id;
                    $scope.SampleOrder.history = result.history;
                    $scope.SampleOrder.entryno = result.entryno;
                    $scope.SampleOrder.modifiedby = result.modifiedby;
                    $scope.SampleOrder.modifiedon = result.modifiedon;
                    $scope.SampleOrder.version = result.version;
                }
                else {
                    $rootScope.Toaster.Error("Attempt Failed." + result.ErrorMessage);
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }
            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }

        $scope.productChange = function () {
            if ($scope.SampleOrder.Details.bomid != null) {
                var prditem = $scope.BOMList.find(function (item) {
                    return (item.id == $scope.SampleOrder.Details.bomid);
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
            $scope.SampleOrder.Details.colourid = null;
            $scope.SampleOrder.Details.bominwardcode = '';
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
                $scope.SampleOrder.Details.bominwardcode = prditem.code;
                $scope.SampleOrder.Details.imageid = prditem.imageid;
            }
            if ($scope.ColorList.length == 1) {
                $scope.SampleOrder.Details.colourid = $scope.ColorList[0].id;
                $scope.FindBomStock();
            }
            else if ($scope.ColorList.length == 0) {
                $scope.FindBomStock();
            }
        }

        $scope.ChangeOrderStatus = function () {
            if ($scope.SampleOrder.id == null || $scope.SampleOrder.id == 0) {
                $rootScope.Toaster.Error("Please select purchase order!");
                return;
            }

            var request = {
                "item": $scope.SampleOrder,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgsampleorderService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Status updated successfully");

            }).error(function (result) {
            });

        }


        $scope.ManageSearchItem = function (command, item) {
            if (command == 'Edit') {
                $scope.SampleOrder = item;
                var request = {
                    "Item": {
                        'id': item.id
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "MfgsampleorderService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.SampleOrder = result[0];
                    $scope.ShowSearch = false;
                    $scope.Navigation.SetMode('EDIT');
                    $timeout(function () {
                        $scope.ProductDivisionChanged();
                        $scope.ItemsConfig();
                        $scope.ConfigDesignItemGrid();
                        $scope.SizeConfig();
                    }, 20);
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
                                    url: "MfgsampleorderService/Delete",
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
                url: "MfgsampleorderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.SearchConfig();
            });
        };

        $scope.Publish = function () {
            restService.Get('MfgsampleorderService/Publish/' + $scope.UserInfo.OrganizationID + '/' + $scope.SampleOrder.id, true).then(function (results) {
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

        $scope.fileuploadedcomplete = function (items) {
            if (typeof ($scope.SampleOrder) == 'undefined' || $scope.SampleOrder == null)
                $scope.SampleOrder = {};
            $scope.SampleOrder.items = items;
            $scope.ItemsConfig();
            $scope.uploadfile = null;
        }


        $scope.SearchConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('searchGrid', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('entryno', 'Entry No', 80);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('bookorderno', 'Booking No', 80);
            igrid.AddFormatedColumn('bookingdate', 'Booking Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('customerid', 'Customer Name', 150, $scope.Customers);
            igrid.AddReferenceColumn('statusid', 'Status', 100, $scope.Status);
            igrid.AddReferenceColumn('prddivisionid', 'Division', 100, $scope.ProductDivision);
            igrid.AddReferenceColumn('preparedby', 'Prepared By', 100, $scope.Employee);
            igrid.AddReferenceColumn('approvedby', 'Approved By', 100, $scope.Employee);

            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            if ($scope.UserInfo && $scope.UserInfo.p_SampleOrder &&
                $scope.UserInfo.p_SampleOrder.print) {
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

        if ($location.search().id != undefined &&
            $location.search().id >= 1) {
            $scope.ProductDivision = restService.Get('ReferencelistService/FULL-PRODUCTDIVISION', true).then(function (results) {
                $scope.ProductDivision = results;
                request = {
                    "Item": {
                        id: $location.search().id,
                    },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "MfgsampleorderService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.SampleOrder = result[0];
                    $scope.ShowSearch = false;
                    $scope.Navigation.SetMode('EDIT');
                    $timeout(function () {
                        $scope.ProductDivisionChanged();
                        $scope.ItemsConfig();
                        $scope.ConfigDesignItemGrid();
                        $scope.SizeConfig();
                    }, 20);
                });
            });
        }
        else {
            $scope.ProductDivision = restService.Get('ReferencelistService/FULL-PRODUCTDIVISION', true).then(function (results) {
                $scope.ProductDivision = results;
                $scope.SampleOrder = (JSON.parse(JSON.stringify($scope.Entity)));
                $scope.ItemsConfig();
                $scope.ConfigDesignItemGrid();
                $scope.SizeConfig();
            });
        }
    }]);
