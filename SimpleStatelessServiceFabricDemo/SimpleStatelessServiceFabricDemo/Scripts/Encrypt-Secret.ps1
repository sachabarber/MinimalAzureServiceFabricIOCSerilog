Param(
   [Parameter(Mandatory=$true)]
   [string]$thumbprint,

   [Parameter(Mandatory=$true)]
   [string]$text
)

## Use certificate locally to encrypt passwords ##


Write-Output "Encrypting text: '$text'"
Write-Output "<START ENCRYPTED TEXT>"
Invoke-ServiceFabricEncryptText -CertStore -CertThumbprint $thumbprint -Text $text -StoreLocation LocalMachine -StoreName My
Write-Output "<STOP ENCRYPTED TEXT>"

##############################################
## MANUAL - Update codebase ##
#############################################

Write-Output "MANUAL STEP: Add the certificate thumbprint '$thumbprint' in the application manifest file"
#  ...
#  <Certificates>
#    <!-- The X509FindValue needs to be replaced with the thumbprint of the certificate uploaded to Azure Key Vault-->
#    <SecretsCertificate X509FindValue="32A480B35B8A6CC0B77D8DB77C72C8D2DC7CAF97" Name="MyCert" />
#  </Certificates>
#</ApplicationManifest>

Write-Output "MANUAL STEP: Update ApplicationParameter file for the env you are encrypting e.g. Cloud.xml"
#Add encrypted connectionStrings to the ApplicationParameters files for each environment 
# - ApplicationParameters\Cloud.xml
# We don't need to add these for Local app parameter files, we default to use the un-encrypted app.config value there

