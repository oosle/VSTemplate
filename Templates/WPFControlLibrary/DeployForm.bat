@echo off
echo.
echo Deploying PartnerForm to development server...

rem This is currently hardwired for SGS DEV server, this will change per deployment
set PartnerDev="\\qls-uat-fs01\partner\srs forms\xaml forms\xaml\PartnerForm"

IF exist %PartnerDev% (
    echo Copying to %PartnerDev%...
) else (
    echo Creating %PartnerDev% and copying...
    mkdir %PartnerDev% >nul
)

del %PartnerDev%\*.* /q >nul
copy ..\*.dll %PartnerDev% >nul
copy ..\*.xml %PartnerDev% >nul
copy ..\*.pdb %PartnerDev% >nul
copy *.xml %PartnerDev% >nul
copy *.xaml %PartnerDev% >nul

echo Note: Make sure PartnerFormView is correctly inserted into DB.
echo Deployment to %PartnerDev% Complete.
