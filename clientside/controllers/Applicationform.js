
var app = angular.module('InfinityLab');

app.controller('ApplicationFormController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.refemployeelist = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.refemployeelist = results;
        });


        $scope.Gender = [{ 'id': 'M', 'name': 'Male' }, { 'id': 'F', 'name': 'Female' }, { 'id': 'T', 'name': 'Transgender' }];
        $scope.Title = [{ 'id': 1, 'name': 'Mr.' }, { 'id': 2, 'name': 'Ms.' }, { 'id': 2, 'name': 'Mrs.' }];

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

        $scope.EducationTypes = restService.Get('ReferencelistService/EDUCATIONTYPE', true).then(function (results) {
            $scope.EducationTypes = results;
        });

        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });

        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
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

        request = {
            "Item": { id: $location.search().id },
            "SecurityToken": "String content"
        }
        var srequest = JSON.stringify(request);

        $http({
            method: 'POST',
            url: "EmployeeService/Search",
            data: srequest
        }).success(function (result) {
            $scope.Application = result[0];

            var educationexperiance = $scope.Application.educationandexperiance.items;
            $scope.Application.education = { 'items': [] };
            $scope.Application.experiance = { 'items': [] };
            $.each(educationexperiance, function (k, v) {
                if (v.mode != null && v.mode == 'Education') {
                    if ($scope.Application.education.items.length < $scope.MaxArraySize)
                        $scope.Application.education.items.push(v);
                }
                else {
                    if ($scope.Application.experiance.items.length < $scope.MaxArraySize)
                        $scope.Application.experiance.items.push(v);
                }
            });

            $scope.SetMaxRows($scope.Application.family.items, 5);
            $scope.SetMaxRows($scope.Application.education.items, 5);
            $scope.SetMaxRows($scope.Application.experiance.items, 5);
            if (typeof ($scope.Application.personal) != 'undefined' && $scope.Application.personal != null)
                $scope.Application.personal.mstatus = $scope.Application.personal.married == true ? 'MARRIED' : 'UNMARRIED';
        }).error(function (result) {
        });

        $scope.SetMaxRows = function(arrayobject,size)
        {
            if (arrayobject.length > size) {
                while (arrayobject.length > $scope.MaxArraySize)
                    arrayobject.splice(arrayobject[arrayobject.length - 1], 1);
            }
            else
            {
                while (arrayobject.length < $scope.MaxArraySize)
                    arrayobject.push({});
            }
        }

        $scope.MaxArraySize = 5;

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
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

        $scope.mergecolumn = function (val1, val2, seperator) {
            var val = '';
            if (typeof (val1) != 'undefined' && val1 != null)
                val = val1;

            if (typeof (val2) != 'undefined' && val2 != null)
            {
                val = val + (val.length > 0 ? seperator : '') + val2;
            }
            return val;
        };

        $scope.hasvalue = function(val,required)
        {
            if (typeof (required) != 'undefined' && required != null)
                return val;

            return ' ';
        }

        $scope.$watch('Application.personal.img_photo', function (newval, oldvalue) {
            if (newval == null) {
                $('#img_photo').attr('src', '');
            }
            else {
                $('#img_photo').attr('src', 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + newval);
            }
        });

        $scope.$watch('Application.companylogo', function (newval, oldvalue) {
            if (newval == null) {
                $('#companylogo').attr('src', '');
            }
            else {
                $('#companylogo').attr('src', 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + newval);
            }
        });
    }]);