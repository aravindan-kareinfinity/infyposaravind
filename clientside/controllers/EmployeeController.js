
var app = angular.module('InfinityLab');

app.controller('EmployeeController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();  

        $scope.Types = restService.Get('ReferencelistService/Types', true).then(function (results) {
            $scope.Types = results;
        });

        $scope.refemployeelist = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.refemployeelist = results;
        });

        $scope.Relieving = true;
        $scope.Checked = function () {
            $scope.Relieving = !$scope.Relieving;
        };

        $scope.Married = true;
        $scope.Ismarried = function () {
            $scope.Married = !$scope.Married;
        };

        $scope.SearchRef = restService.SearchReference;

        $scope.ResignReasons = [{ 'id': 0, 'name': 'Without Reason.' },
        { 'id': 1, 'name': 'On Leave.' },
        { 'id': 2, 'name': 'Left Service.' },
        { 'id': 3, 'name': 'Retired.' },
        { 'id': 4, 'name': 'Out of Coverage.' },
        { 'id': 5, 'name': 'Expired.' },
        { 'id': 6, 'name': 'Non Implemented area.' },
        { 'id': 7, 'name': 'Compliance by Immediate Employer.' },
        { 'id': 8, 'name': 'Suspension of work.' },
        { 'id': 9, 'name': 'Strike/Lockout.' },
        { 'id': 10, 'name': 'Retrenchment.' },
        { 'id': 11, 'name': 'No Work.' },
        { 'id': 12, 'name': 'Doesnt Belong To This Employer.' },
        { 'id': 13, 'name': 'Duplicate IP.' }];

        $scope.Title = [{ 'id': 1, 'name': 'Mr.' }, { 'id': 2, 'name': 'Ms.' }, { 'id': 3, 'name': 'Mrs.' }];
        $scope.MaritalStatus = [{ 'id': 'Married', 'name': 'Married' }, { 'id': 'Un married', 'name': 'Un married' }, { 'id': 'Divorced', 'name': 'Divorced' }];

        $scope.Gender = [{ 'id': 'M', 'name': 'Male' }, { 'id': 'F', 'name': 'Female' }, { 'id': 'T', 'name': 'Transgender' }];
        $scope.WorkingModes = [{ 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.SalaryModes = [{ 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];
        $scope.Encashment = [{ 'id': 0, 'name': 'None' }, { 'id': 1, 'name': 'Additional' }, { 'id': 2, 'name': 'Adjustment' }, { 'id': 3, 'name': 'Automatic' }, { 'id': 4, 'name': 'Fixed' }];
        $scope.LeaveDays = [{ 'id': 0, 'name': 'Dynamic' }, { 'id': 1, 'name': 'Sunday' }, { 'id': 2, 'name': 'Monday' }, { 'id': 3, 'name': 'Tuesday' },
                            { 'id': 4, 'name': 'Wednesday' }, { 'id': 5, 'name': 'Thursday' }, { 'id': 6, 'name': 'Friday' }, { 'id': 7, 'name': 'Saturday' }];

        $scope.refstructurelist = restService.Get('ReferencelistService/STRUCTURE', true).then(function (results) {
            $scope.refstructurelist = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });


        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });

        $scope.Roles = restService.Get('ReferencelistService/Roles', true).then(function (results) {
            $scope.Roles = results;
        });

        $scope.Designations = restService.Get('ReferencelistService/DESIGNATION', true).then(function (results) {
            $scope.Designations = results;
        });

        $scope.Departments = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.Departments = results;
        });

        $scope.WorkingHours = restService.Get('ReferencelistService/WORKINGHOURS', true).then(function (results) {
            $scope.WorkingHours = results;
        });

        $scope.EducationTypes = restService.Get('ReferencelistService/EDUCATIONTYPE', true).then(function (results) {
            $scope.EducationTypes = results;
        });

        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });

        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });

        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });

        $scope.Countries = restService.Get('ReferencelistService/COUNTRY', true).then(function (results) {
            $scope.Countries = results;
        });

        $scope.Community = restService.Get('ReferencelistService/COMMUNITY', true).then(function (results) {
            $scope.Community = results;
        });

        $scope.Caste = restService.Get('ReferencelistService/CASTE', true).then(function (results) {
            $scope.Caste = results;
        });

        $scope.Bloodgroup = restService.Get('ReferencelistService/BLOODGROUP', true).then(function (results) {
            $scope.Bloodgroup = results;
        });

        $scope.ShowApplicationForm = function (item) {
            $location.url('/applicationform?id=' + item.id);
        }


        // model for User creation
        $scope.Entity = {
            "id": 0, "name": null, "title": "", "emailid": null, "contactno": null, "desingnationid": 0, refempid: "", "companyid": 0, "roleid": 0, "isactive": true, "createdby": 0, "modifiedby": 0, "version": 0,
            "createdon": "", "modifiedon": "", "dateofjoining": "", "relievingdate": "", "surname": "",
            "allowsystemaccess": false, "username": null, "workingmode": null, "workinghour": null, "salarymode": 0, "employeecode": null, "cardcode": null, "gender": null,
            "personal": {
                "bloodgroup": null, "dob": null, "fathername": null, "mothername": null, "fatherno": null, "motherno": null, "caste": null, "community": null, "languagesknown": null, "religion": null,
                "nationality": null, "marrieddate": null, "smartcardno": null, "img_idcard": null, "img_photo": null, "img_signature": null, "img_thumb": null, "img_aadhar": null, "img_bankbook": null,
                "married": false, "strength": null, "expdesignation": null, "expsalary": null, "spousename": null, "spouseno": null, "livewithwife": null, "livewithmother": null, "livewithfather": null,
                "alivewithfather": null, "alivewithmother": null, "alivewithwife": null, "panno": null, maritalstatus:""
            },
            "family": { "items": [] },
            "pfandesi": { "pfno": null, "pfuan": null, "esino": null, "esienabled": false, "pfenabled": false, "esienabledon": "", "pfenabledon": "", "pffrom": null, "pfto": null, "restrictpffrom": null, "restrictpfto": null, "zeropffrom": null, "zeropfto": null},
            "presentaddress": { "residentno": null, "residentname": null, "street": null, "area": null, "city": null, "district": null, "state": null, "pincode": null, "village": null },
            "permanentaddress": { "residentno": null, "residentname": null, "street": null, "area": null, "city": null, "district": null, "state": null, "pincode": null, "village": null },
            "aadharno": null, "pancardno": null, "releavingreason": 0,
            "remarks": { "text": null },
            "locationid": 0, "floorid": 0, "departmentid": 0, "sectionid": 0, "bankid": 0,
            "classification": { "Items": [] },
            "addionalinfo": { "Items": [] },
            "passport": { "number": null, "issuedat": null, "issuedon": "", "expiryon": "" },
            "trainings": { "Items": [] },
            "accident": { "Items": [] },
            "disciplinary": { "Items": [] },
            "educationandexperiance": { "Items": [] },
            "license": { "no": null, "issuedon": "", "issuedat": null, "expiryon": "" },
            "castecategory": 0, "qualification": 0, "nationality": 0, "language": 0, "images_photo": null, "images_signature": null, "images_thumb": null,
            "bank": { "bankid": 0, "accountname": null, "ifsc": null, "accountno": null }
        };

        $scope.Employee = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.Employee.addionalinfo.items = [];
        $scope.Employee.classification.items = [];
        $scope.Employee.family.items = [];
        $scope.Employee.educationandexperiance.items = [];
        $scope.Employee.addionalinfo.items = [];
        $scope.Employee.trainings.items = [];
        $scope.Employee.accident.items = [];
        $scope.Employee.disciplinary.items = [];

        $scope.New = function () {
            $scope.Employee = (JSON.parse(JSON.stringify($scope.Entity)));
        }
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Employee = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.$watch('Employee.personal.img_photo', function (newval, oldvalue) {
            if (newval == null) {
                $('#img_photo').attr('src', '');
            }
            else {
                $('#img_photo').attr('src', 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + newval);
            }
        });

        $scope.SearchCondition = {
            name: "",
            contactno: "",
            hasleft: "",
            showdetail: false,
            workingmode:""

        }
        $scope.New = function () {
            $scope.Employee = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.exporttoExcel = function () {
            $scope.exportHref = $scope.igrid.ExportExcel($scope.igrid.grid, 'Employee List');
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "Employees.xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.SaveClassification = function () {
            if (typeof ($scope.Employee.classification.items) == 'undefined' || $scope.Employee.classification.items == null)
                $scope.Employee.classification.items = [];

            $scope.Employee.classification.items.push(
                {
                    'validfrom': $scope.Employee.classification.validfrom,
                    'validto': $scope.Employee.classification.validto,
                    'designation': $scope.Employee.classification.designation,
                    'department': $scope.Employee.classification.department,
                    'division': $scope.Employee.classification.division,
                    'floor': $scope.Employee.classification.floor
                });
        }

        $scope.SaveAdditionalInfo = function () {
            if (typeof ($scope.Employee.addionalinfo.items) == 'undefined' || $scope.Employee.addionalinfo.items == null)
                $scope.Employee.addionalinfo.items = [];
            $scope.Employee.addionalinfo.items.push(
                {
                    'key': $scope.Employee.addionalinfo.key,
                    'value': $scope.Employee.addionalinfo.value,
                    'recordedon': $scope.Employee.addionalinfo.recordedon
                });
        }

        $scope.SaveEducationQualify = function () {
            if (typeof ($scope.Employee.educationandexperiance.items) == 'undefined' || $scope.Employee.educationandexperiance.items == null)
                $scope.Employee.educationandexperiance.items = [];

            $scope.Employee.educationandexperiance.items.push((JSON.parse(JSON.stringify($scope.Employee.educationexperiance))));
            $scope.Employee.educationexperiance = {};
        }

        $scope.SaveFamilyDetail = function () {
            if (typeof ($scope.Employee.family.items) == 'undefined' || $scope.Employee.family.items == null)
                $scope.Employee.family.items = [];
            $scope.Employee.family.items.push(
                          {
                              'name': $scope.Employee.family.name,
                              'relation': $scope.Employee.family.relation,
                              'gender': $scope.Employee.family.gender,
                              'age': $scope.Employee.family.age,
                              'remarks': $scope.Employee.family.remarks,
                              'qualification': $scope.Employee.family.qualification,
                              'dateofbirth': $scope.Employee.family.dateofbirth
                          });
        }

        $scope.SaveTrainingDetails = function () {
            if (typeof ($scope.Employee.trainings.items) == 'undefined' || $scope.Employee.trainings.items == null)
                $scope.Employee.trainings.items = [];
            $scope.Employee.trainings.items.push(
                       {
                           'name': $scope.Employee.trainings.name,
                           'givenon': $scope.Employee.trainings.givenon,
                           'remark': $scope.Employee.trainings.remark,
                           'confirmationdate': $scope.Employee.trainings.confirmationdate,
                           'completiondate': $scope.Employee.trainings.completiondate,
                           'noticeperiod': $scope.Employee.trainings.noticeperiod

                       });
        }

        $scope.SaveDiciplinary = function () {
            if (typeof ($scope.Employee.disciplinary.items) == 'undefined' || $scope.Employee.disciplinary.items == null)
                $scope.Employee.disciplinary.items = [];
            $scope.Employee.disciplinary.items.push(
                       {
                           'name': $scope.Employee.disciplinary.name,
                           'identifiedby': $scope.Employee.disciplinary.identifiedby,
                           'occuredon': $scope.Employee.disciplinary.occuredon,
                           'completiondate': $scope.Employee.disciplinary.completiondate,
                           'remark': $scope.Employee.disciplinary.remark
                       });
        }

        $scope.SaveEmployeeAccidentDetails = function () {
            if (typeof ($scope.Employee.accident.items) == 'undefined' || $scope.Employee.accident.items == null)
                $scope.Employee.accident.items = [];
            $scope.Employee.accident.items.push(
                                 {
                                     'name': $scope.Employee.accident.name,
                                     'accidenton': $scope.Employee.accident.accidenton,
                                     'hospital': $scope.Employee.accident.hospital,
                                     'bodylocation': $scope.Employee.accident.bodylocation,
                                     'accidentlocation': $scope.Employee.accident.accidentlocation
                                 });
        }



        $scope.Active = true;

        $scope.Save = function () {



            request = {
                "Item": $scope.Employee,
                "securitytoken": "String content"

            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeeService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Employee Saved Successfully.!");
                    $scope.Employee = (JSON.parse(JSON.stringify($scope.Entity)));

                }
                else {
                    $rootScope.Toaster.Info("Problem With Employee Saving.!");
                    $scope.ErrorMessage = result.ErrorMessage;
                    $rootScope.Toaster.Info("Problem With Employee Saving.!");
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }

        $scope.DeletefromList = function (source, removeitem) {
            source.splice(source.indexOf(removeitem), 1);
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
                                    url: "EmployeeService/Delete",
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
            else if (action == "Print") {
                $rootScope.Download('EmployeeService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id, 'application.pdf', 'application/pdf');
                return;

                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "application.pdf";
                    link.href = 'EmployeeService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
            else if (action == "Edit") {
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.Employee = item;
                if ($scope.Employee.classification == null || $scope.Employee.classification.items == null) {
                    $scope.Employee.classification = { 'items': [] };
                }
                if ($scope.Employee.family == null || $scope.Employee.family.items == null) {
                    $scope.Employee.family = { 'items': [] };
                }

                if ($scope.Employee.educationandexperiance == null || $scope.Employee.educationandexperiance.items == null) {
                    $scope.Employee.educationandexperiance = { 'items': [] };
                }
                if ($scope.Employee.addionalinfo == null || $scope.Employee.addionalinfo.items == null) {
                    $scope.Employee.addionalinfo = { 'items': [] };
                }
                if ($scope.Employee.trainings == null || $scope.Employee.trainings.items == null) {
                    $scope.Employee.trainings = { 'items': [] };
                }
                if ($scope.Employee.accident == null || $scope.Employee.accident.items == null) {
                    $scope.Employee.accident = { 'items': [] };
                }
                //if ($scope.Employee.permanentaddress == null || $scope.Employee.permanentaddress.items == null) {
                //    $scope.Employee.permanentaddress = { 'items': [] };
                //}
                //if ($scope.Employee.presentaddress == null || $scope.Employee.presentaddress.items == null) {
                //    $scope.Employee.presentaddress = { 'items': [] };
                //}
                if ($scope.Employee.disciplinary == null || $scope.Employee.disciplinary.items == null) {
                    $scope.Employee.disciplinary = { 'items': [] };
                }
                if (item.personal != null && item.personal.img_photo != null && item.personal.img_photo != '') {
                    $('#img_photo').attr('src', 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + item.personal.img_photo);
                }
                else {
                    $('#img_photo').attr('src', '');
                }

            }
        }

        $scope.MoveRight = function () {
            //var remove = [];
            //$.each($scope.Employee.presentaddress, function (k, v) {
            //    $scope.Employee.permanentaddress.push(v);
            //});  
            //$scope.Employee.permanentaddress.push($scope.Employee.presentaddress);
            $scope.Employee.permanentaddress.residentno = $scope.Employee.presentaddress.residentno;
            $scope.Employee.permanentaddress.residentname = $scope.Employee.presentaddress.residentname;
            $scope.Employee.permanentaddress.street = $scope.Employee.presentaddress.street;
            $scope.Employee.permanentaddress.area = $scope.Employee.presentaddress.area;
            $scope.Employee.permanentaddress.village = $scope.Employee.presentaddress.village;
            $scope.Employee.permanentaddress.city = $scope.Employee.presentaddress.city;
            $scope.Employee.permanentaddress.district = $scope.Employee.presentaddress.district;
            $scope.Employee.permanentaddress.state = $scope.Employee.presentaddress.state;
            $scope.Employee.permanentaddress.pincode = $scope.Employee.presentaddress.pincode;
        }

        $scope.PrintBarcode = function () {
            var list = [];
            $.each($scope.SearchResult, function (k, v) {
                if (v.selected)
                    list.push({ 'id': v.id, 'code': v.employeecode });
            });
            if (list.length == 0) {
                $rootScope.ShowAlert('Salesman barcode', 'Please select the employees');
                return;
            }
            $rootScope.Prompt('Barcode', 'No Of Copies', function (count) {

                var requestitem = { 'copy': count, 'items': list };
                $scope.ChoseReport(requestitem);

            });
        };


        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('EMPLOYEECODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "EmployeeService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.Sent2Printer = function (printdocument, printerData) {

            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            //$.each(printdocument, function (k, v) {
                //var request = {
                //    'id': printerData.printerid,
                //    'content': v,
                //    'encoded': 'BASE64'
                //};
                //var srequest = JSON.stringify(request);
                //$http({
                //    method: 'POST',
                //    url: printerData.service,
                //    data: srequest
                //}).success(function (result) {

                //}).error(function (error) {

                //});
            //});
        }


        $scope.SelectAllChanged = function () {
            $.each($scope.SearchResult, function (k, v) {
                v.selected = $scope.SearchCondition.SelectAll;
            });
        }

        $scope.SearchDirect = function (code) {
            request = {
                "Item": { 'employeecode': code , 'includeleft':true },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeeService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.Employee = result[0];
                }
                else {
                    $rootScope.Toaster.Info("Employee Code Not found!");
                }
            }).error(function (result) {
            });
        };

        $scope.ShowApplicationForm = function (item) {
            $location.url('/applicationform?id=' + item.id);
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeeService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.EmployeeConfig();
            }).error(function (result) {
            });

        }


        $scope.EmployeeConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, false, true,10);
            igrid.options.showHeaderRow = true;

            igrid.EnableSerial($scope.SearchResult);

          
            igrid.AddSimpleColumn('employeecode', 'EMP ID', 100);
            igrid.AddSimpleColumn('name', 'Name', 300);

            igrid.AddSimpleColumn('gender', 'Gender', 75);
            if ($rootScope.UserInfo.p_SALARYMGMT.view)
                igrid.AddSimpleColumn('grosspay', 'Gross Salary', 100);
            igrid.AddFormatedColumn('dateofbirth', 'Date Of Birth', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('dateofjoining', 'Date Of Joining', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('bloodgroup', 'Blood Group', 100);
            igrid.AddSimpleColumn('emailid', 'EmailId', 100);
            igrid.AddSimpleColumn('contactno', 'Contact No', 100);
            igrid.AddSimpleColumn('aadharno', 'Aadhar No', 100);
            igrid.AddSimpleColumn('pfno', 'PF No', 100);
            igrid.AddSimpleColumn('pfuan', 'PF UAN', 100);
            igrid.AddSimpleColumn('esino', 'ESI No', 100);

            igrid.AddReferenceColumn('floorid', 'Floor', 150, $scope.Floors);
            igrid.AddReferenceColumn('departmentid', 'Department', 150, $scope.Departments);
            igrid.AddReferenceColumn('sectionid', 'Section', 150, $scope.Divisions);
            igrid.AddReferenceColumn('desingnationid', 'Desingnation', 150, $scope.Designations);
            igrid.AddReferenceColumn('locationid', 'Locations', 150, $scope.Locations);


            if ($rootScope.UserInfo.p_EMPLOYEE.modify)
                var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/>&nbsp;"
            if ($rootScope.UserInfo.p_EMPLOYEE.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>&nbsp;"
            if ($rootScope.UserInfo.p_EMPLOYEE.print)
                commands += "<input type='button' class='btn btn-info btn-xs' value='Print'/>"
            if ($rootScope.UserInfo.p_EMPLOYEE.modify || $rootScope.UserInfo.p_EMPLOYEE.delete || $rootScope.UserInfo.p_EMPLOYEE.print) {
                igrid.AddButtonColumn('action', 'ACTION', 180, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Print')
                            $scope.ManageSearchItem('Print', args.item);
                    });
                });
            }


            //igrid.AddButtonColumn('Edit', 'Action', 110, 'Edit', function (e, args) {

            //    $scope.$apply(function () {
            //        $scope.ManageSearchItem('Edit', args.item);
            //    });
            //    return true;
            //});

            //igrid.AddButtonColumn('Delete', '', 110, 'Delete', function (e, args) {

            //    $scope.$apply(function () {
            //        $scope.ManageSearchItem('Delete', args.item);
            //    });
            //    return true;
            //});

            //igrid.AddButtonColumn('id', '', 100, 'Print', function (e, args) {

            //    $scope.$apply(function () {
            //        $scope.ManageSearchItem('id', args.item);
            //    });
            //    return true;
            //});

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.grid.setOptions({ 'frozenColumn': 3 });
            $scope.igrid.UpdateData($scope.SearchResult);

        }

        $timeout(function () {
            $('.nav-tabs a').click(function (e) {
                e.preventDefault();
                $(this).tab('show');
                if ($(this).is(".dropdown-menu li a")) {
                    $("#dropdownTabName").html($(this).html());
                    $("#dropDownTabsLink").dropdown("toggle");
                }
            });
            $scope.setFocusfirst();

        }, 10);
    }]);