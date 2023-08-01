const { execSync } = require('child_process');
const { GitHub } = require('@actions/github');
const path = require('path');
const fs = require('fs');

async function getVersion() {
    const packageJsonPath = path.join(process.cwd(), 'package.json');
    const packageJson = JSON.parse(fs.readFileSync(packageJsonPath, 'utf8'));
    return packageJson.version;
}

async function main() {
    try {
        const githubToken = process.env.GITHUB_TOKEN;
        const githubRepo = process.env.GITHUB_REPOSITORY;
        const version = await getVersion();
        const projectName = 'UnityAnimTool';
        const releaseTitle = `${projectName}_V${version}`;
        const fileName = `${projectName}_V${version}.zip`;

        // 압축 파일 생성 (이 예시에서는 간단하게 zip으로 생성)
        execSync(`zip -r ${fileName} ./your_project_directory`);

        // GitHub 인스턴스 생성 및 레포지토리 가져오기
        const octokit = new GitHub(githubToken);
        const [owner, repo] = githubRepo.split('/');
        const repository = octokit.repo(owner, repo);

        // 릴리스 생성
        const release = await repository.releases.createRelease({
            tag_name: `V${version}`, // 태그를 "V버전" 형식으로 설정
            name: releaseTitle,
            body: 'Release Notes',
        });

        // 릴리스에 파일 첨부
        await repository.releases.uploadReleaseAsset({
            release_id: release.data.id,
            name: fileName,
            data: fs.readFileSync(fileName),
        });

        // 압축 파일 제거
        fs.unlinkSync(fileName);
    } catch (error) {
        console.error('Error occurred during deployment:', error);
        process.exit(1);
    }
}

main();
